using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class TitleManager : MonoBehaviour
{
	public Transform newButton, loadButton;
	public RectTransform itemContainer;
	public Animator animator;
	public List<FileItemButton> fileItemButtons = new List<FileItemButton>();
	public GameObject fileItemPrefab;
	public Button beginButton, nextButton, cancelButton;
	public Text nameText, versionText, fileText, appVersion, engineVersion, headingText;
	public CanvasGroup selectJourneyCG, selectHeroesCG;
	public Button[] heroButtons;
	public Image finalFader;
	public AudioSource music;
	public PostProcessVolume volume;
	public SettingsDialog settings;

	ProjectItem[] projectItems;
	ProjectItem selectedJourney;
	bool[] selectedHeroes;
	string[] heroes = new string[6] { "Aragorn", "Berevor", "Bilbo", "Elena", "Gimli", "Legolas" };

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
		engineVersion.text = "Engine Version: " + Bootstrap.EngineVersion;
	}

	public void NewGame()
	{
		newButton.DOLocalMoveX( -1135, 1 ).SetEase( Ease.InOutQuad );
		loadButton.DOLocalMoveX( -1135, 1 ).SetEase( Ease.InOutQuad ).OnComplete( () =>
		{
			animator.Play( "bgFadeIn" );
		} );
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
		newButton.DOLocalMoveX( -700, .5f );
		loadButton.DOLocalMoveX( -700, .5f );
		animator.Play( "bgFadeOut" );
		beginButton.interactable = false;
		cancelButton.interactable = false;
	}

	public void OnNext()
	{
		selectJourneyCG.interactable = false;

		selectedHeroes = new bool[6].Fill( false );
		ResetHeroColors();

		selectJourneyCG.DOFade( 0, .5f ).OnComplete( () =>
		{
			selectHeroesCG.gameObject.SetActive( true );
			selectHeroesCG.DOFade( 1, .5f ).OnComplete( () => selectHeroesCG.interactable = true );
			headingText.text = "Select Heroes";
		} );
	}

	public void OnBack()
	{
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
				sh[i] = heroes[i];
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

		ResetHeroColors();

		beginButton.interactable = selectedHeroes.Any( b => b );
	}

	void ResetHeroColors()
	{
		for ( int i = 0; i < 6; i++ )
		{
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
		for ( int i = 0; i < fileItemButtons.Count; i++ )
		{
			if ( i != index )
				fileItemButtons[i].ResetColor();
		}
		//fill in file info
		nameText.text = projectItems[index].Title;
		fileText.text = projectItems[index].fileName;
		versionText.text = "File Version: " + projectItems[index].fileVersion;

		nextButton.interactable = true;
		selectedJourney = projectItems[index];
	}
}
