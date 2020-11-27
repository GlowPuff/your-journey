using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectSaveSlot : MonoBehaviour
{
	public CampaignScreen campaignScreen;
	public SelectJourney selectJourney;
	public ConfirmDelete confirmDelete;
	public NameGameDialog nameDialog;
	public Image finalFader;
	public Text selectedName, selectedDate, warningMsg, headingText, nextText, loadedGameScenario, itemType;
	public Button nextButton, cancelButton;
	public Toggle toggle;
	public GameObject warning, campaignSaveWarning;
	public SaveSlotButton[] saveSlotButtons;

	StateItem[] stateItems;
	StateItem selectedState = null;
	int selectedIndex = -1;
	//int slotMode;//0=choose a new game slot, 1=choose a game to load
	string gameName;
	TitleMetaData titleMetaData;

	public void ActivateScreen( TitleMetaData metaData )
	{
		titleMetaData = metaData;
		gameObject.SetActive( true );
		warning.SetActive( false );
		nextButton.interactable = false;
		cancelButton.interactable = false;
		if ( titleMetaData.slotMode == 1 )//|| titleMetaData.slotMode == 1 )
			toggle.gameObject.SetActive( false );
		else
			toggle.gameObject.SetActive( true );
		toggle.isOn = false;
		selectedName.text = selectedDate.text = loadedGameScenario.text = itemType.text = "";
		selectedState = null;
		selectedIndex = -1;

		if ( titleMetaData.slotMode == 0 )
		{
			headingText.text = "New Save Slot";
			nextText.text = "Next";
			campaignSaveWarning.SetActive( true );
		}
		else
		{
			headingText.text = "Load A Saved Game";
			nextText.text = "Begin";
			campaignSaveWarning.SetActive( false );
		}

		//reset all buttons
		for ( int i = 0; i < saveSlotButtons.Length; i++ )
		{
			saveSlotButtons[i].ResetButton();
			if ( titleMetaData.slotMode == 0 )//disable by default
				saveSlotButtons[i].EnableButton();
		}

		stateItems = GameState.GetSaveItems().ToArray();
		for ( int i = 0; i < stateItems.Length/*Mathf.Min( saveSlotButtons.Length, stateItems.Length )*/; i++ )
		{
			if ( stateItems[i] != null )
				saveSlotButtons[i].Init( stateItems[i] );
		}

		finalFader.DOFade( 0, .5f ).OnComplete( () =>
		{
			cancelButton.interactable = true;
		} );
	}

	public void OnToggleNoSave()
	{
		selectedIndex = -1;
		gameName = "";
		selectedName.text = selectedDate.text = itemType.text = "";

		if ( toggle.isOn )//don't save
		{
			nextButton.interactable = true;
			RefreshButtons();
		}
		else
		{
			nextButton.interactable = false;
			RefreshButtons();
		}
	}

	/// <summary>
	/// sets selectedIndex=-1, selectedSate=null, empty strings, button enabling/disabling
	/// </summary>
	void RefreshButtons()
	{
		selectedIndex = -1;
		selectedState = null;
		selectedName.text = selectedDate.text = itemType.text = "";

		for ( int i = 0; i < saveSlotButtons.Length; i++ )
		{
			saveSlotButtons[i].ResetButton();
			if ( titleMetaData.slotMode == 0 && !toggle.isOn )
				saveSlotButtons[i].EnableButton();
		}

		for ( int i = 0; i < Mathf.Min( saveSlotButtons.Length, stateItems.Length ); i++ )
		{
			saveSlotButtons[i].Init( stateItems[i] );
			if ( toggle.isOn )//no save?
				saveSlotButtons[i].EnableButton( false );
		}
	}

	public void OnSelectSlot( int index )
	{
		RefreshButtons();
		warning.SetActive( false );

		if ( titleMetaData.slotMode == 0 )//new game
		{
			if ( toggle.isOn )//no save is ON
				return;

			if ( stateItems[index] == null )
			{
				selectedIndex = index;
				selectedState = null;

				selectedName.text = "Empty Slot";
				selectedDate.text = itemType.text = "";

				//name new game
				nameDialog.Show( OnYes, OnNo );

				return;
			}
			else//chose existing slot, stateitem is not null
			{
				selectedIndex = index;
				selectedState = stateItems[index];
				//confirm overwrite save
				confirmDelete.Show( selectedState, () =>
				{//yes
					saveSlotButtons[selectedIndex].ResetButton();
					selectedName.text = selectedDate.text = itemType.text = "";
					stateItems[index] = null;
					//name deleted slot
					nameDialog.Show( OnYes, OnNo );
				}, () =>
				{//no
					selectedIndex = -1;
					selectedState = null;
					RefreshButtons();
				} );
			}
		}
		else if ( titleMetaData.slotMode == 1 )//continue game
		{
			selectedIndex = index;
			selectedState = stateItems[index];

			gameName = selectedState.gameName;
			nextButton.interactable = true;
			warning.SetActive( false );
			selectedName.text = stateItems[index].gameName;
			selectedDate.text = stateItems[index].gameDate;
			itemType.text = "This is a Standalone Scenario";

			Debug.Log( "OnSelectSlot::" + index + "::" + selectedState.projectType );
			//check file version for standalone scenario
			if ( selectedState.projectType == ProjectType.Standalone )
			{
				nextText.text = "Begin";
				Scenario s = Bootstrap.LoadScenarioFromFilename( selectedState.scenarioFilename );
				if ( s != null )
				{
					loadedGameScenario.text = s.scenarioName;
					if ( s.scenarioGUID != selectedState.stateGUID )
					{
						warningMsg.text = "WARNING\r\nThe selected item was saved with a different version of the Scenario than you have.";
						warning.SetActive( true );
					}
				}
				else
				{
					loadedGameScenario.text = "";
					selectedDate.text = "";
					itemType.text = "";
					selectedIndex = -1;
					selectedState = null;
					nextButton.interactable = false;
					warningMsg.text = "WARNING\r\nThere was a problem loading the Scenario this item was saved in.";
					warning.SetActive( true );
				}
			}
			else//campaign
			{
				nextText.text = "Next";
				selectedDate.text = selectedState.campaignState.gameDate;
				itemType.text = "This is a Campaign";
				loadedGameScenario.text = selectedState.campaignState.campaign.campaignName;
				Campaign c = FileManager.LoadCampaign( selectedState.campaignState.campaign.campaignGUID.ToString() );
				//check file version for campaign
				if ( c.fileVersion != selectedState.fileVersion )
				{
					warningMsg.text = "WARNING\r\nThe selected Campaign state was saved with a different version of the Campaign than you have.";
					warning.SetActive( true );
				}
			}
		}
	}

	/// <summary>
	/// from new game name dialog
	/// </summary>
	public void OnYes( string gname )
	{
		gameName = gname;
		selectedName.text = gname;
		nextButton.interactable = true;
		saveSlotButtons[selectedIndex].SetName( gameName );
		saveSlotButtons[selectedIndex].SetIsEmpty( false );
	}

	public void OnNo()
	{
		nextButton.interactable = false;
		saveSlotButtons[selectedIndex].ResetButton();
		saveSlotButtons[selectedIndex].EnableButton( titleMetaData.slotMode == 0 );
		selectedIndex = -1;
		gameName = "";
		selectedState = null;
	}

	public void OnDeleteSlot( int index )
	{
		warning.SetActive( false );
		confirmDelete.Show( stateItems[index], () =>
		 {//yes
			 if ( selectedState == stateItems[index] )
			 {
				 //deselect it if previously selected
				 selectedIndex = -1;
				 selectedState = null;
				 selectedName.text = selectedDate.text = "";
				 nextButton.interactable = false;
			 }
			 stateItems[index] = null;
			 saveSlotButtons[index].ResetButton();
			 saveSlotButtons[index].EnableButton( titleMetaData.slotMode == 0 );
		 }, () =>
		 {//no
		 } );
	}

	public void OnNext()
	{
		nextButton.interactable = false;
		cancelButton.interactable = false;

		if ( toggle.isOn )//do not save
		{
			titleMetaData.saveStateIndex = -1;
		}

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			if ( titleMetaData.slotMode == 0 )//new game
			{
				gameObject.SetActive( false );
				titleMetaData.saveStateIndex = selectedIndex;
				titleMetaData.gameName = gameName;

				selectJourney.ActivateScreen( titleMetaData );
			}
			else//restore game
			{
				if ( selectedState != null )
				{
					//load gameboard into standalone scenario
					if ( selectedState.projectType == ProjectType.Standalone )
					{
						gameObject.SetActive( false );
						//bootstrap the game state
						GameStarter gameStarter = new GameStarter();

						if ( !toggle.isOn )//DO save
						{
							gameStarter.gameName = gameName;
							gameStarter.saveStateIndex = selectedIndex;
						}
						else
						{
							gameStarter.saveStateIndex = -1;
						}
						gameStarter.scenarioFileName = selectedState.scenarioFilename;
						gameStarter.heroes = selectedState.heroArray;
						gameStarter.isNewGame = false;

						Bootstrap.campaignState = null;
						Bootstrap.gameStarter = gameStarter;

						SceneManager.LoadScene( "gameboard" );
					}
					else//else it's a campaign
					{
						//fill in metadata
						titleMetaData.campaignState = GameState.LoadState( selectedIndex ).campaignState;
						titleMetaData.saveStateIndex = selectedIndex;
						titleMetaData.previousScreen = TitleScreen.SelectSlot;

						gameObject.SetActive( false );
						campaignScreen.ActivateScreen( titleMetaData );
					}
				}
				else
				{
					warningMsg.text = "WARNING\r\nUnexpected DATA for the selected item.";
					warning.SetActive( true );
				}
			}
		} );
	}

	public void OnCancel()
	{
		nextButton.interactable = false;
		cancelButton.interactable = false;

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			FindObjectOfType<TitleManager>().ResetScreen();
		} );
	}
}
