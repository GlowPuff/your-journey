﻿using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CampaignScreen : MonoBehaviour
{
	public SelectSaveSlot selectSaveSlot;
	public SelectHeroes selectHeroes;
	public StoryBox storyBox;
	public Image finalFader;
	public Button continueButton, backButton, continueReplayButton;
	public GameObject fileItemButtonPrefab;
	public RectTransform itemContainer;
	public GameObject replaybox;
	public Text xpLoreText, currentScenarioText, replayText, replayStatusText;

	TitleMetaData titleMetaData;
	Campaign campaign;
	CampaignState campaignState;
	List<FileItemButton> fileItemButtons = new List<FileItemButton>();
	int selectedIndex = -1;

	//players can CONTINUE latest scenario or REPLAY any scenario, keeping only the highest xp/lore earned from it

	public void ActivateScreen( TitleMetaData metaData )
	{
		titleMetaData = metaData;
		selectedIndex = -1;
		//when LOADING from state, some title metadata is unset:
		//difficulty, gameName, projectItem, selectedHeroes
		campaign = titleMetaData.campaignState.campaign;
		campaignState = titleMetaData.campaignState;
		replaybox.SetActive( false );
		var xp = campaignState.scenarioXP.Aggregate( ( acc, cur ) => acc + cur );
		var lore = campaignState.scenarioLore.Aggregate( ( acc, cur ) => acc + cur );
		xpLoreText.text = lore + " / " + xp;
		replayText.text = "";
		currentScenarioText.text = campaignState.campaign.scenarioCollection[campaignState.currentScenarioIndex].scenarioName;

		bool finishedCampaign = !campaignState.scenarioStatus.Any( x => x == ScenarioStatus.NotPlayed );

		for ( int i = 0; i < campaign.scenarioCollection.Count; i++ )
		{
			var go = Instantiate( fileItemButtonPrefab, itemContainer ).GetComponent<FileItemButton>();
			go.transform.localPosition = new Vector3( 0, ( -110 * i ) );
			go.Init( i, campaign.scenarioCollection[i].scenarioName, ProjectType.Standalone, ( index ) => OnSelectScenario( index ) );
			if ( campaignState.currentScenarioIndex != i || finishedCampaign )
				go.RemoveRing();
			go.SetSuccess( campaignState.scenarioStatus[i] );
			fileItemButtons.Add( go );
		}
		itemContainer.sizeDelta = new Vector2( 772, fileItemButtons.Count * 110 );

		CheckCampaignStatus();

		gameObject.SetActive( true );

		finalFader.DOFade( 0, .5f ).OnComplete( () =>
		{
			backButton.interactable = true;
		} );
	}

	void CheckCampaignStatus()
	{
		//check status
		//1 - no replay in progress, no current campaign in progress
		//2 - current campaign in progress
		//3 - replay in progress
		bool finishedCampaign = !campaignState.scenarioStatus.Any( x => x == ScenarioStatus.NotPlayed );

		var gs = GameState.LoadState( campaignState.saveStateIndex );
		if ( gs.partyState == null )//1
		{
			continueReplayButton.interactable = false;
			continueButton.interactable = true;

			replayStatusText.text = "No game is in progress.";
			//check if campaign is finished
			if ( !finishedCampaign )
				replayStatusText.text += "  You may continue the Campaign from the current Scenario with the Continue Campaign button.";
			else
			{
				replayStatusText.text += "  The campaign has been finished.  Only Scenario Replays are available to play.";
			}
		}
		else//2 or 3 - a game is in progress
		{
			//3 - different scenario than latest
			if ( gs.campaignState.scenarioPlayingIndex != campaignState.currentScenarioIndex )
			{
				replayStatusText.text = "A Replay is in progress.";
				continueReplayButton.interactable = true;
				if ( !finishedCampaign )
					replayStatusText.text += "  You may continue the Replay or forfeit it and continue the Campaign from the current Scenario.";
				else
					replayStatusText.text += "  You may continue the Replay, but the Campaign has been finished.";
			}
			else//3 - state scenario is same as current scenario, could be a replay or current campaign play. Check its scenarioStatus
			{
				if ( gs.campaignState.scenarioStatus[gs.campaignState.scenarioPlayingIndex] == ScenarioStatus.NotPlayed )//2 - current campaign state
					replayStatusText.text = "A game is in progress.  Continue it with the Continue Campaign button.";
				else//3 - replay
				{
					replayStatusText.text = "A Replay is in progress.  Continue it with the Continue Replay button.";
					if ( campaignState.scenarioStatus.Any( x => x == ScenarioStatus.NotPlayed ) )
						replayStatusText.text += "  The campaign has been finished.";
				}
			}
		}

		//disable continue campaign button if all scenarios have been played
		if ( campaignState.scenarioStatus.Any( x => x == ScenarioStatus.NotPlayed ) )
			continueButton.interactable = true;
		else
			continueButton.interactable = false;
	}

	public void OnSelectScenario( int index )
	{
		for ( int i = 0; i < fileItemButtons.Count; i++ )
		{
			if ( i != index )
				fileItemButtons[i].ResetColor();
		}

		selectedIndex = index;

		//if selected scenario has been played (fail or success) activated replay option
		if ( campaignState.scenarioStatus[index] != ScenarioStatus.NotPlayed )
		{
			replaybox.SetActive( true );
			replayText.text = campaign.scenarioCollection[index].scenarioName;
		}
		else
		{
			replaybox.SetActive( false );
			replayText.text = "";
			//show haven't played message
		}
	}

	public void OnContinueCampaign()
	{
		campaignState.scenarioPlayingIndex = campaignState.currentScenarioIndex;

		int sIndex = campaignState.currentScenarioIndex;

		//bootstrap the game state
		GameStarter gameStarter = new GameStarter();
		gameStarter.gameName = campaignState.gameName;
		gameStarter.saveStateIndex = campaignState.saveStateIndex;
		gameStarter.scenarioFileName = campaignState.campaign.scenarioCollection[sIndex].fileName;
		gameStarter.heroes = campaignState.heroes;
		gameStarter.difficulty = campaignState.difficulty;

		Bootstrap.campaignState = campaignState;
		Bootstrap.gameStarter = gameStarter;

		//check for a saved state
		GameState gs = GameState.LoadState( campaignState.saveStateIndex );
		if ( gs.partyState == null )//no saved state
		{
			gameStarter.isNewGame = true;//start scenario fresh
			Debug.Log( "NEW GAME" );
		}
		else//saved state found, is it current scenario or replay?
		{
			if ( gs.partyState.scenarioFileName == gameStarter.scenarioFileName )//it's the current scenario
			{
				gameStarter.isNewGame = false;//otherwise start scenario fresh
				Debug.Log( "CONTINUING SAVED GAME" );
			}
			else//it's just a replay, toast it and start fresh
			{
				gameStarter.isNewGame = true;
				Debug.Log( "NEW GAME" );
			}
		}

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			SceneManager.LoadScene( "gameboard" );
		} );
	}

	public void OnReplayScenario()
	{
		campaignState.scenarioPlayingIndex = selectedIndex;

		//bootstrap the game state
		GameStarter gameStarter = new GameStarter();
		gameStarter.gameName = campaignState.gameName;
		gameStarter.saveStateIndex = campaignState.saveStateIndex;
		gameStarter.scenarioFileName = campaignState.campaign.scenarioCollection[selectedIndex].fileName;
		gameStarter.heroes = campaignState.heroes;
		gameStarter.difficulty = campaignState.difficulty;
		gameStarter.isNewGame = true;//start scenario fresh

		Bootstrap.campaignState = campaignState;
		Bootstrap.gameStarter = gameStarter;

		var scenario = FileManager.LoadScenario( FileManager.GetFullPathWithCampaign( gameStarter.scenarioFileName, campaign.campaignGUID.ToString() ) );

		if ( !string.IsNullOrEmpty( scenario.specialInstructions ) )
		{
			storyBox.Show( scenario.specialInstructions, () =>
			{
				finalFader.DOFade( 1, .5f ).OnComplete( () =>
				{
					gameObject.SetActive( false );
					SceneManager.LoadScene( "gameboard" );
				} );
			} );
		}
		else
		{
			finalFader.DOFade( 1, .5f ).OnComplete( () =>
			{
				gameObject.SetActive( false );
				SceneManager.LoadScene( "gameboard" );
			} );
		}
	}

	public void OnContinueReplay()
	{
		campaignState.scenarioPlayingIndex = selectedIndex;

		var gs = GameState.LoadState( campaignState.saveStateIndex );

		//bootstrap the game state
		GameStarter gameStarter = new GameStarter();
		gameStarter.gameName = campaignState.gameName;
		gameStarter.saveStateIndex = campaignState.saveStateIndex;
		gameStarter.scenarioFileName = gs.partyState.scenarioFileName;
		gameStarter.heroes = campaignState.heroes;
		gameStarter.difficulty = campaignState.difficulty;
		gameStarter.isNewGame = false;

		Bootstrap.campaignState = campaignState;
		Bootstrap.gameStarter = gameStarter;

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			SceneManager.LoadScene( "gameboard" );
		} );
	}

	public void OnBack()
	{
		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );

			if ( titleMetaData.skippedToCampaignScreen )
			{
				FindObjectOfType<TitleManager>().ResetScreen();
			}
			else
			{
				if ( titleMetaData.previousScreen == TitleScreen.SelectHeroes )
					selectHeroes.ActivateScreen( titleMetaData );
				else if ( titleMetaData.previousScreen == TitleScreen.SelectSlot )
					selectSaveSlot.ActivateScreen( titleMetaData );
			}
		} );
	}

	public void OnShowStory()
	{
		storyBox.Show( campaignState.campaign.storyText, null );
	}
}
