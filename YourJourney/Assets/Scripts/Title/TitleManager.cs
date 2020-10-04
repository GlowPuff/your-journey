using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class TitleManager : MonoBehaviour
{
	public Button newButton, loadButton;
	public RectTransform itemContainer;
	public Animator animator;
	public List<FileItemButton> fileItemButtons = new List<FileItemButton>();
	public GameObject fileItemPrefab, warningPanel;
	public Button beginButton, nextButton, cancelButton;
	public Text nameText, versionText, fileText, appVersion, engineVersion, headingText, diffText;
	public CanvasGroup selectJourneyCG, selectHeroesCG;
	public Button[] heroButtons;
	public Image finalFader;
	public AudioSource music;
	public PostProcessVolume volume;
	public SettingsDialog settings;
	public Text[] heroNameText;

	ProjectItem[] projectItems;
	ProjectItem selectedJourney;
	bool[] selectedHeroes;
	//string[] heroes = new string[6] { "Aragorn", "Beravor", "Bilbo", "Elena", "Gimli", "Legolas" };
	CanvasGroup newBcg, loadBcg;
	string tempName;
	bool isChangingName = false;
	int nameIndex = -1;

	private void Start()
	{
		var settings = Bootstrap.LoadSettings();
		Vignette v;
		ColorGrading cg;
		if ( volume.profile.TryGetSettings( out v ) )
			v.active = settings.Item2 == 1;
		if ( volume.profile.TryGetSettings( out cg ) )
			cg.active = settings.Item3 == 1;
		music.enabled = settings.Item1 == 1;

		projectItems = FileManager.GetProjects().ToArray();
		for ( int i = 0; i < projectItems.Length; i++ )
		{
			var go = Instantiate( fileItemPrefab, itemContainer ).GetComponent<FileItemButton>();
			go.transform.localPosition = new Vector3( 0, ( -110 * i ) );
			go.Init( i, projectItems[i].Title );
			fileItemButtons.Add( go );
		}

		selectedHeroes = new bool[6].Fill( false );

		itemContainer.sizeDelta = new Vector2( 772, fileItemButtons.Count * 110 );

		appVersion.text = "App Version: " + Bootstrap.AppVersion;
		engineVersion.text = "Scenario Format Version: " + Bootstrap.FormatVersion;

		newBcg = newButton.GetComponent<CanvasGroup>();
		loadBcg = loadButton.GetComponent<CanvasGroup>();

		GlowTimer.SetTimer( 5, () =>
		{
			newButton.transform.DOLocalMoveX( -700, .75f );
			loadButton.transform.DOLocalMoveX( -700, .75f );
			newBcg.interactable = true;
			loadBcg.interactable = true;
		} );
	}

	public void NewGame()
	{
		//newButton.DOLocalMoveX( -1135, 1 ).SetEase( Ease.InOutQuad );
		//loadButton.DOLocalMoveX( -1135, 1 ).SetEase( Ease.InOutQuad ).OnComplete( () =>
		//{
		//	animator.Play( "bgFadeIn" );
		//} );
		newBcg.DOFade( 0, .25f );
		loadBcg.DOFade( 0, .25f );
		newBcg.blocksRaycasts = false;
		loadBcg.blocksRaycasts = false;

		newButton.interactable = false;
		loadButton.interactable = false;
		animator.Play( "bgFadeIn" );

		nextButton.interactable = false;
		cancelButton.interactable = true;
		for ( int i = 0; i < fileItemButtons.Count; i++ )
			fileItemButtons[i].ResetColor();
		nameText.text = "";
		fileText.text = "";
		versionText.text = "";
		headingText.text = "Select A Journey";
		selectHeroesCG.gameObject.SetActive( false );
	}

	public void LoadGame()
	{
		//newButton.DOLocalMoveX( -1135, .5f ).SetEase( Ease.InOutQuad );
		//loadButton.DOLocalMoveX( -1135, .5f ).SetEase( Ease.InOutQuad ).OnComplete( () =>
		//{
		//	//animator.Play( "bgFadeIn" );
		//} );
		//;

		//newButton.interactable = false;
		//loadButton.interactable = false;
		//animator.Play( "bgFadeIn" );

		//nextButton.interactable = false;
		//cancelButton.interactable = true;
		//for ( int i = 0; i < fileItemButtons.Count; i++ )
		//	fileItemButtons[i].ResetColor();
		//nameText.text = "";
		//fileText.text = "";
		//versionText.text = "";
		//selectHeroesCG.gameObject.SetActive( false );
	}

	public void Cancel()
	{
		//newButton.DOLocalMoveX( -700, .5f );
		//loadButton.DOLocalMoveX( -700, .5f );
		newBcg.DOFade( 1, .25f );
		loadBcg.DOFade( 1, .25f );
		newBcg.blocksRaycasts = true;
		loadBcg.blocksRaycasts = true;

		newButton.interactable = true;
		loadButton.interactable = true;
		animator.Play( "bgFadeOut" );
		beginButton.interactable = false;
		cancelButton.interactable = false;
	}

	public void OnDifficulty()
	{
		if ( Bootstrap.difficulty == Difficulty.Easy )
			Bootstrap.difficulty = Difficulty.Normal;
		else if ( Bootstrap.difficulty == Difficulty.Normal )
			Bootstrap.difficulty = Difficulty.Hard;
		else if ( Bootstrap.difficulty == Difficulty.Hard )
			Bootstrap.difficulty = Difficulty.Easy;
		diffText.text = Bootstrap.difficulty.ToString();
	}

	public void OnNext()
	{
		//go to hero selection
		warningPanel.SetActive( false );
		selectJourneyCG.interactable = false;

		//reset game vars
		beginButton.interactable = false;
		Bootstrap.difficulty = Difficulty.Normal;
		diffText.text = "Normal";
		selectedHeroes = new bool[6].Fill( false );
		ResetHeros();

		selectJourneyCG.DOFade( 0, .5f ).OnComplete( () =>
		{
			selectHeroesCG.gameObject.SetActive( true );
			selectHeroesCG.DOFade( 1, .5f ).OnComplete( () => selectHeroesCG.interactable = true );
			headingText.text = "Select Heroes";
		} );
	}

	public void OnBack()
	{
		//back to journey select screen
		isChangingName = false;
		selectHeroesCG.interactable = false;
		nextButton.interactable = false;
		nameText.text = "";
		fileText.text = "";
		versionText.text = "";

		for ( int i = 0; i < fileItemButtons.Count; i++ )
			fileItemButtons[i].ResetColor();

		selectHeroesCG.DOFade( 0, .5f ).OnComplete( () =>
		{
			selectHeroesCG.gameObject.SetActive( false );
			selectJourneyCG.DOFade( 1, .5f ).OnComplete( () => selectJourneyCG.interactable = true );
			headingText.text = "Select A Journey";
		} );
	}

	public void OnBegin()
	{
		string[] sh = new string[6].Fill( "" );
		for ( int i = 0; i < 6; i++ )
		{
			if ( selectedHeroes[i] )
				sh[i] = heroNameText[i].text;
		}
		sh = sh.Where( s => !string.IsNullOrEmpty( s ) ).ToArray();
		Bootstrap.SetNewGame( sh, selectedJourney );

		selectHeroesCG.interactable = false;

		DOTween.To( () => music.volume, setter => music.volume = setter, 0f, .5f );
		finalFader.gameObject.SetActive( true );
		finalFader.DOFade( 1, .5f ).OnComplete( () => SceneManager.LoadScene( "gameboard" ) );
	}

	public void OnSettings()
	{
		settings.Show( "Quit App" );
	}

	public void OnHeroSelect( int index )
	{
		ColorBlock cb = heroButtons[index].colors;
		heroButtons[index].colors = new ColorBlock()
		{
			normalColor = new Color( 1, 167f / 255f, 124f / 255f, 1 ),
			pressedColor = cb.pressedColor,
			selectedColor = new Color( 1, 167f / 255f, 124f / 255f, 1 ),
			colorMultiplier = cb.colorMultiplier,
			disabledColor = cb.disabledColor,
			fadeDuration = cb.fadeDuration,
			highlightedColor = cb.highlightedColor
		};
		selectedHeroes[index] = !selectedHeroes[index];

		ResetHeros();

		beginButton.interactable = selectedHeroes.Any( b => b );
	}

	void ResetHeros()
	{
		for ( int i = 0; i < 6; i++ )
		{
			heroNameText[i].text = Bootstrap.GetHeroName( i );
			ColorBlock cb = heroButtons[i].colors;
			if ( !selectedHeroes[i] )
			{
				heroButtons[i].colors = new ColorBlock()
				{
					normalColor = new Color( 1, 167f / 255f, 124f / 255f, 0 ),
					pressedColor = cb.pressedColor,
					selectedColor = new Color( 1, 167f / 255f, 124f / 255f, 0 ),
					colorMultiplier = cb.colorMultiplier,
					disabledColor = cb.disabledColor,
					fadeDuration = cb.fadeDuration,
					highlightedColor = cb.highlightedColor
				};
			}
		}
	}

	public void OnSelectQuest( int index )
	{
		Debug.Log( "QUEST:" + index );
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
	}

	public void OnChangeNameClick( int index )
	{
		isChangingName = true;
		nameIndex = index;
		heroNameText[index].color = Color.green;
		tempName = heroNameText[nameIndex].text;
		heroNameText[nameIndex].text = "";
	}

	private void Update()
	{
		if ( isChangingName )
		{
			if ( Input.GetKeyDown( KeyCode.Escape ) )
			{
				isChangingName = false;
				heroNameText[nameIndex].color = Color.white;
				heroNameText[nameIndex].text = tempName;
				return;
			}

			foreach ( char c in Input.inputString )
			{
				if ( c == '\b' ) // has backspace/delete been pressed?
				{
					if ( heroNameText[nameIndex].text.Length != 0 )
					{
						heroNameText[nameIndex].text = heroNameText[nameIndex].text.Substring( 0, heroNameText[nameIndex].text.Length - 1 );
					}
				}
				else if ( ( c == '\n' ) || ( c == '\r' ) ) // enter/return
				{
					isChangingName = false;
					heroNameText[nameIndex].color = Color.white;
					if ( string.IsNullOrEmpty( heroNameText[nameIndex].text ) )
						heroNameText[nameIndex].text = tempName;
					else
						Bootstrap.SaveHeroName( nameIndex, heroNameText[nameIndex].text );
				}
				else
				{
					heroNameText[nameIndex].text += c;
				}
			}
		}
	}
}
