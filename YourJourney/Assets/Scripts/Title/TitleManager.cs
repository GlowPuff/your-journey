using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
	public SelectJourney selectJourney;
	public SelectSaveSlot selectSaveSlot;
	public Button newButton, loadButton;
	public Image finalFader;
	public AudioSource music;
	public PostProcessVolume volume;
	public SettingsDialog settings;

	CanvasGroup newBcg, loadBcg;

	//new game->select save->select journey->select heroes->show pre-game message->start game

	//load game->select save->start game

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

		newBcg = newButton.GetComponent<CanvasGroup>();
		loadBcg = loadButton.GetComponent<CanvasGroup>();

		selectJourney.AddScenarioPrefabs();

		GlowTimer.SetTimer( 5, () =>
		{
			newButton.transform.DOLocalMoveX( -700, .75f );
			loadButton.transform.DOLocalMoveX( -700, .75f );
			newBcg.interactable = true;
			loadBcg.interactable = true;
		} );
	}

	public void ResetScreen()
	{
		finalFader.DOFade( 0, .5f );

		newBcg.DOFade( 1, .25f );
		loadBcg.DOFade( 1, .25f );
		newBcg.blocksRaycasts = true;
		loadBcg.blocksRaycasts = true;

		newButton.interactable = true;
		loadButton.interactable = true;
	}

	public void NewGame()
	{
		newBcg.DOFade( 0, .25f );
		loadBcg.DOFade( 0, .25f );
		newBcg.blocksRaycasts = false;
		loadBcg.blocksRaycasts = false;

		newButton.interactable = false;
		loadButton.interactable = false;

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			selectSaveSlot.ActivateScreen( 0 );
		} );
	}

	public void LoadGame()
	{
		newBcg.DOFade( 0, .25f );
		loadBcg.DOFade( 0, .25f );
		newBcg.blocksRaycasts = false;
		loadBcg.blocksRaycasts = false;

		newButton.interactable = false;
		loadButton.interactable = false;

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			selectSaveSlot.ActivateScreen( 1 );
		} );
	}

	public void OnSettings()
	{
		settings.Show( "Quit App" );
	}
}
