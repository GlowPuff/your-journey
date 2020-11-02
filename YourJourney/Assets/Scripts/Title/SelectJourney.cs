using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SelectJourney : MonoBehaviour
{
	public SelectHeroes selectHeroes;
	public SelectSaveSlot selectSaveSlot;
	public List<FileItemButton> fileItemButtons = new List<FileItemButton>();
	public Image finalFader;
	public Text nameText, versionText, fileText, appVersion, engineVersion;
	ProjectItem[] projectItems;
	ProjectItem selectedJourney;
	public GameObject fileItemPrefab, warningPanel;
	public RectTransform itemContainer;
	public Button nextButton, cancelButton;

	int slotMode;

	public void ActivateScreen( int smode )
	{
		gameObject.SetActive( true );
		warningPanel.SetActive( false );
		cancelButton.interactable = true;
		slotMode = smode;

		for ( int i = 0; i < fileItemButtons.Count; i++ )
			fileItemButtons[i].ResetColor();

		appVersion.text = "App Version: " + Bootstrap.AppVersion;
		engineVersion.text = "Scenario Format Version: " + Bootstrap.FormatVersion;
		nameText.text = "";
		fileText.text = "";
		versionText.text = "";

		finalFader.DOFade( 0, .5f );
	}

	public void AddScenarioPrefabs()
	{
		projectItems = FileManager.GetProjects().ToArray();
		for ( int i = 0; i < projectItems.Length; i++ )
		{
			var go = Instantiate( fileItemPrefab, itemContainer ).GetComponent<FileItemButton>();
			go.transform.localPosition = new Vector3( 0, ( -110 * i ) );
			go.Init( i, projectItems[i].Title );
			fileItemButtons.Add( go );
		}
		itemContainer.sizeDelta = new Vector2( 772, fileItemButtons.Count * 110 );
	}

	public void OnSelectQuest( int index )
	{
		warningPanel.SetActive( false );

		for ( int i = 0; i < fileItemButtons.Count; i++ )
		{
			if ( i != index )
				fileItemButtons[i].ResetColor();
		}
		//fill in file info
		nameText.text = projectItems[index].Title;
		fileText.text = projectItems[index].fileName;
		versionText.text = "File Version: " + projectItems[index].fileVersion;

		//check version
		if ( projectItems[index].fileVersion != Bootstrap.FormatVersion )
			warningPanel.SetActive( true );

		nextButton.interactable = true;
		selectedJourney = projectItems[index];
		Debug.Log( selectedJourney.fileName );
	}

	public void OnNext()
	{
		Bootstrap.scenarioFileName = selectedJourney.fileName;
		nextButton.interactable = false;
		cancelButton.interactable = false;

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			selectHeroes.ActivateScreen( selectedJourney, slotMode );
		} );
	}

	public void OnCancel()
	{
		nextButton.interactable = false;
		cancelButton.interactable = false;
		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			selectSaveSlot.ActivateScreen( slotMode );
		} );
	}
}
