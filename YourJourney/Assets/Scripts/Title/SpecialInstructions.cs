﻿using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpecialInstructions : MonoBehaviour
{
	public SelectHeroes selectHeroes;
	public Image finalFader;
	public Button beginButton, cancelButton, backButton, increaseLoreButton, decreaseLoreButton, increaseXPButton, decreaseXPButton;
	public TextMeshProUGUI loreText, xpText, instructions;
	public AudioSource music;

	RectTransform itemContainer;
	TitleMetaData titleMetaData;
	Scenario s;

	public void ActivateScreen( TitleMetaData metaData )
	{
		titleMetaData = metaData;
		gameObject.SetActive( true );
		//itemContainer = instructions.rectTransform;

		loreText.text = "";
		xpText.text = "";
		instructions.text = "";

		finalFader.DOFade( 0, .5f ).OnComplete( () =>
		{
			beginButton.interactable = true;
			backButton.interactable = true;
			cancelButton.interactable = true;
			s = Bootstrap.LoadScenarioFromFilename( titleMetaData.projectItem.fileName );
			if ( s != null )
			{
				if ( !string.IsNullOrEmpty( s.specialInstructions ) )
					SetText( s.specialInstructions );
				else
					SetText( "There are no special instructions for this Scenario." );
				loreText.text = s.loreStartValue.ToString();
				xpText.text = s.xpStartValue.ToString();
				if (s.projectType == ProjectType.Standalone)
				{
					//Set buttons visible
					increaseLoreButton.gameObject.SetActive(true);
					decreaseLoreButton.gameObject.SetActive(true);
					increaseXPButton.gameObject.SetActive(true);
					decreaseXPButton.gameObject.SetActive(true);
					OnAdjustLore(0); //This will set the buttons interactable
					OnAdjustXP(0); //This will set the buttons interactable
				}
			}
			else
			{
				SetText( "There was a problem loading the Scenario." );
				beginButton.interactable = false;
			}
		} );
	}

	void SetText( string t )
	{
		instructions.text = t;

		//TextGenerator textGen = new TextGenerator();
		//TextGenerationSettings generationSettings = instructions.GetGenerationSettings( instructions.rectTransform.rect.size );

		//float height = textGen.GetPreferredHeight( t, generationSettings );

		//itemContainer.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, height + 20 );
	}

	public void OnBegin()
	{
		//bootstrap into the scenario
		GameStarter gameStarter = new GameStarter();
		gameStarter.gameName = titleMetaData.gameName;
		gameStarter.saveStateIndex = titleMetaData.saveStateIndex;
		gameStarter.scenarioFileName = titleMetaData.projectItem.fileName;
        gameStarter.heroes = titleMetaData.selectedHeroes;
		gameStarter.heroesIndex = titleMetaData.selectedHeroesIndex;
        gameStarter.difficulty = titleMetaData.difficulty;
		gameStarter.isNewGame = true;
		gameStarter.loreStartValue = s.loreStartValue;
		gameStarter.xpStartValue = s.xpStartValue;
		gameStarter.coverImage = s.coverImage;

		Bootstrap.gameStarter = gameStarter;
		Bootstrap.campaignState = null;

		DOTween.To( () => music.volume, setter => music.volume = setter, 0f, .5f );
		//gameObject.SetActive( false );
		gameObject.SetActive( false ); //hide the SpecialInstructions form but leave scenarioOverlay with coverImage showing
		TitleManager tm = FindObjectOfType<TitleManager>();
		tm.gameTitle.SetActive(false);
		tm.gameTitleFlash.SetActive(false);
		tm.settingsButton.SetActive(false);
		tm.bannerTop.SetActive(false);
		tm.bannerBottom.SetActive(false);
		SceneManager.LoadSceneAsync( "gameboard" );
	}

	public void OnBack()
	{
		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			selectHeroes.ActivateScreen( titleMetaData );
			gameObject.SetActive( false );
		} );
	}

	public void OnCancel()
	{
		beginButton.interactable = false;
		backButton.interactable = false;
		cancelButton.interactable = false;

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			FindObjectOfType<TitleManager>().ResetScreen();
		} );
	}

	public void OnAdjustLore(int amount)
	{
		Debug.Log("Adjust Lore " + amount);

		int minLore = 0;
		int maxLore = 105;

		s.loreStartValue += amount;
		if(s.loreStartValue < minLore) { s.loreStartValue = minLore; }
		else if(s.loreStartValue > maxLore) { s.loreStartValue = maxLore; }

		loreText.text = s.loreStartValue.ToString();

		decreaseLoreButton.interactable = s.loreStartValue > minLore;
		increaseLoreButton.interactable = s.loreStartValue < maxLore;
	}

	public void OnAdjustXP(int amount)
	{
		Debug.Log("Adjust XP " + amount);

		int minXP = 0;
		int maxXP = 105;

		s.xpStartValue += amount;
		if (s.xpStartValue < minXP) { s.xpStartValue = minXP; }
		else if (s.xpStartValue > maxXP) { s.xpStartValue = maxXP; }

		xpText.text = s.xpStartValue.ToString();

		decreaseXPButton.interactable = s.xpStartValue > minXP;
		increaseXPButton.interactable = s.xpStartValue < maxXP;
	}
}

