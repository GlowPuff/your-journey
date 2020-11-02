using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectSaveSlot : MonoBehaviour
{
	public SelectJourney selectJourney;
	public ConfirmDelete confirmDelete;
	public NameGameDialog nameDialog;
	public Image finalFader;
	public Text selectedName, selectedDate, warningMsg, headingText, nextText, loadedGameScenario;
	public Button nextButton, cancelButton;
	public Toggle toggle;
	public GameObject warning;
	public SaveSlotButton[] saveSlotButtons;

	StateItem[] stateItems;
	StateItem selectedState = null;
	int selectedIndex = -1;
	int slotMode;//0=choose a new game slot, 1=choose a game to load
	string gameName;

	public void ActivateScreen( int mode, bool isCampaign = false )
	{
		gameObject.SetActive( true );
		warning.SetActive( false );
		nextButton.interactable = false;
		cancelButton.interactable = false;
		if ( ( mode == 0 && isCampaign ) || mode == 1 )
			toggle.gameObject.SetActive( false );
		else
			toggle.gameObject.SetActive( true );
		toggle.isOn = false;
		selectedName.text = selectedDate.text = loadedGameScenario.text = "";
		slotMode = mode;
		selectedState = null;
		selectedIndex = -1;

		if ( slotMode == 0 )
		{
			headingText.text = "New Save Slot";
			nextText.text = "Next";
		}
		else
		{
			headingText.text = "Load A Saved Game";
			nextText.text = "Begin";
		}

		//reset all buttons
		for ( int i = 0; i < saveSlotButtons.Length; i++ )
		{
			saveSlotButtons[i].ResetButton();
			if ( slotMode == 0 )//disable by default
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
		if ( toggle.isOn )//don't save
		{
			selectedIndex = -1;
			selectedName.text = selectedDate.text = "";
			nextButton.interactable = true;
			RefreshButtons();
		}
		else
		{
			RefreshButtons();
			nextButton.interactable = false;
		}
	}

	/// <summary>
	/// sets selectedIndex=-1, selectedSate=null, empty strings, button enabling/disabling
	/// </summary>
	void RefreshButtons()
	{
		selectedIndex = -1;
		selectedState = null;
		selectedName.text = selectedDate.text = "";

		for ( int i = 0; i < saveSlotButtons.Length; i++ )
		{
			saveSlotButtons[i].ResetButton();
			if ( slotMode == 0 && !toggle.isOn )
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

		if ( slotMode == 0 )//new game
		{
			if ( toggle.isOn )//no save is ON
				return;

			if ( stateItems[index] == null )
			{
				selectedIndex = index;
				selectedState = null;

				selectedName.text = "Empty Slot";
				selectedDate.text = "";

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
					if ( selectedState == stateItems[selectedIndex] )
					{
						selectedName.text = selectedDate.text = "";
						//old save was deleted, continue to next screen
						finalFader.DOFade( 1, .5f ).OnComplete( () =>
						{
							gameObject.SetActive( false );
							selectJourney.ActivateScreen( slotMode );
						} );
						return;
					}
				}, () =>
				{//no
					selectedIndex = -1;
					selectedState = null;
					RefreshButtons();
				} );
			}
		}
		else if ( slotMode == 1 )
		{
			selectedIndex = index;
			selectedState = stateItems[index];

			nextButton.interactable = true;
			warning.SetActive( false );
			selectedName.text = stateItems[index].gameName;
			selectedDate.text = stateItems[index].gameDate;
			//check file version
			Scenario s = Bootstrap.LoadLevel( selectedState.scenarioFilename );
			if ( s != null )
			{
				loadedGameScenario.text = s.scenarioName;
				if ( s.scenarioGUID != selectedState.scenarioGUID )
				{
					warningMsg.text = "WARNING\r\nThe selected item was saved with a different version of the Scenario than you have.";
					warning.SetActive( true );
				}
			}
			else
			{
				loadedGameScenario.text = "";
				selectedIndex = -1;
				selectedState = null;
				nextButton.interactable = false;
				warningMsg.text = "WARNING\r\nThere was a problem loading the Scenario this item was saved in.";
				warning.SetActive( true );
			}
		}
	}

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
		saveSlotButtons[selectedIndex].EnableButton( slotMode == 0 );
		selectedIndex = -1;
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
			 saveSlotButtons[index].EnableButton( slotMode == 0 );
		 }, () =>
		 {//no
		 } );
	}

	public void OnNext()
	{
		nextButton.interactable = false;
		cancelButton.interactable = false;

		if ( !toggle.isOn )//DO save
		{
			Bootstrap.gameName = gameName;
			Bootstrap.saveStateIndex = selectedIndex;
		}
		else
			Bootstrap.saveStateIndex = -1;

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			if ( slotMode == 0 )//new game
				selectJourney.ActivateScreen( slotMode );
			else//restore game
			{
				if ( selectedState != null )
				{
					Bootstrap.scenarioFileName = selectedState.scenarioFilename;
					Bootstrap.heroes = selectedState.heroArray;
					Bootstrap.isNewGame = false;
					SceneManager.LoadScene( "gameboard" );
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

	//private void Update()
	//{

	//}
}
