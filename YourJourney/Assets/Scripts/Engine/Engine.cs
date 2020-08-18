using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class Engine : MonoBehaviour
{
	[HideInInspector]
	public Scenario scenario;

	public CamControl camControl;
	public TileManager tileManager;
	public EndTurnButton endTurnButton;
	public ObjectiveManager objectiveManager;
	public InteractionManager interactionManager;
	public TriggerManager triggerManager;
	public ChapterManager chapterManager;
	public Image fader;
	public CanvasGroup uiControl;
	public SettingsDialog settingsDialog;
	public AudioSource music;
	public PostProcessVolume volume;

	bool debug = true;

	void Awake()
	{
		var settings = Bootstrap.LoadSettings();
		Vignette v;
		ColorGrading cg;
		if ( volume.profile.TryGetSettings( out v ) )
			v.active = settings.Item2 == 1;
		if ( volume.profile.TryGetSettings( out cg ) )
			cg.active = settings.Item3 == 1;
		music.enabled = settings.Item1 == 1;

		if ( debug )
			scenario = Bootstrap.DEBUGLoadLevel();
		else
			scenario = Bootstrap.LoadLevel();

		endTurnButton.InitialSet( scenario );

		if ( Bootstrap.isNewGame )
		{
			fader.gameObject.SetActive( true );
			fader.DOFade( 0, 2 ).OnComplete( () =>
			{
				fader.gameObject.SetActive( false );
				//StartNewGame();
			} );
		}
		else
			ContinueGame();
	}

	public void StartNewGame()
	{
		//first objective/interaction/trigger are DUMMIES (None), remove them
		scenario.objectiveObserver.RemoveAt( 0 );
		scenario.interactionObserver.RemoveAt( 0 );
		scenario.triggersObserver.RemoveAt( 0 );

		interactionManager.Init( scenario );
		objectiveManager.Init( scenario );
		chapterManager.Init( scenario );

		if ( !debug )
		{
			interactionManager.GetNewTextPanel().ShowOkContinue( scenario.introBookData.pages[0], ButtonIcon.Continue, () =>
				{
					uiControl.interactable = true;
					//endTurnButton.InitialSet( scenario );

					if ( objectiveManager.Exists( scenario.objectiveName ) )
						objectiveManager.TrySetObjective( scenario.objectiveName, () =>
						 {
							 chapterManager.TryTriggerChapter( "Start", true );
						 } );
					else
						chapterManager.TryTriggerChapter( "Start", true );
				} );
		}
		else
		{
			//debug quickstart a chapter:
			objectiveManager.DebugSetObjective( scenario.objectiveName );
			uiControl.interactable = true;
			endTurnButton.InitialSet( scenario );
			chapterManager.TryTriggerChapter( "Start", true );
		}
	}

	public void ContinueGame()
	{
		//restore data
	}

	void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Alpha1 ) )
		{
			FindObjectOfType<MonsterManager>().AddMonsterGroup( new Monster()
			{
				//isLarge = true,
				isElite = false,
				health = 2,
				currentHealth = new int[] { 2, 2, 2 },
				shieldValue = 0,
				count = 2,
				movementValue = 2,
				maxMovementValue = 4,
				GUID = System.Guid.NewGuid(),
				monsterType = MonsterType.OrcHunter,
				dataName = "Orc Hunter"
			} );
		}
		else if ( Input.GetKeyDown( KeyCode.Alpha2 ) )
		{
			//FindObjectOfType<MonsterManager>().RemoveMonster( 0 );
			//tg2 = tileManager.CreateGroupFromChapter( scenario.chapterObserver[0] );
			//tg2.AttachTo( tg1 );
			//camControl.MoveTo( tg2.groupCenter );
			//tg2.AnimateTileUp();
		}
		else if ( Input.GetKeyDown( KeyCode.Alpha3 ) )
		{
		}
		else if ( Input.GetKeyDown( KeyCode.Delete ) )
		{
			//tileManager.RemoveAllTiles();
		}
	}

	public void OnShowSettings()
	{
		settingsDialog.Show( "Quit to Title", OnQuit );
	}

	public void OnQuit()
	{
		fader.gameObject.SetActive( true );
		fader.DOFade( 1, 2 ).OnComplete( () =>
		{
			SceneManager.LoadScene( "title" );
		} );

	}
}
