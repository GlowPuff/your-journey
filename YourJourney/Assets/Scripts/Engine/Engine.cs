using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

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
		System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

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

		//first objective/interaction/trigger are DUMMIES (None), remove them
		scenario.objectiveObserver.RemoveAt( 0 );
		scenario.interactionObserver.RemoveAt( 0 );
		scenario.triggersObserver.RemoveAt( 0 );

		interactionManager.Init( scenario );
		objectiveManager.Init( scenario );
		chapterManager.Init( scenario );
		endTurnButton.Init( scenario );

		StartCoroutine( BuildScenario() );

		if ( Bootstrap.isNewGame )
		{
			fader.gameObject.SetActive( true );
			fader.DOFade( 0, 2 ).OnComplete( () =>
			{
				fader.gameObject.SetActive( false );
				StartNewGame();
			} );
		}
		else
			ContinueGame();
	}

	public void StartNewGame()
	{
		if ( !debug )
		{
			interactionManager.GetNewTextPanel().ShowOkContinue( scenario.introBookData.pages[0], ButtonIcon.Continue, () =>
				{
					uiControl.interactable = true;

					if ( objectiveManager.Exists( scenario.objectiveName ) )
						objectiveManager.TrySetFirstObjective( scenario.objectiveName, () =>
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
			chapterManager.TryTriggerChapter( "Start", true );
		}
	}

	public void ContinueGame()
	{
		//restore data
	}

	IEnumerator BuildScenario()
	{
		tileManager.BuildScenario();
		yield return null;
	}

	void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Alpha1 ) )
		{
			//FindObjectOfType<MonsterManager>().AddMonsterGroup( new Monster()
			//{
			//	//isLarge = true,
			//	isElite = false,
			//	health = 2,
			//	currentHealth = new int[] { 2, 2, 2 },
			//	shieldValue = 0,
			//	count = 2,
			//	movementValue = 2,
			//	specialAbility = "",
			//	GUID = System.Guid.NewGuid(),
			//	monsterType = MonsterType.OrcHunter,
			//	dataName = "Orc Hunter",
			//	damage = 2
			//} );
		}
		else if ( Input.GetKeyDown( KeyCode.Alpha2 ) )
		{
			FindObjectOfType<LorePanel>().AddLore( 4 );
			//FindObjectOfType<MonsterManager>().RemoveMonster( 0 );
			//tg2 = tileManager.CreateGroupFromChapter( scenario.chapterObserver[0] );
			//tg2.AttachTo( tg1 );
			//camControl.MoveTo( tg2.groupCenter );
			//tg2.AnimateTileUp();
		}
		else if ( Input.GetKeyDown( KeyCode.S ) )
		{
			GameState gs = new GameState();
			gs.SaveState( this );
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

	public void RemoveFog( string chName )
	{
		foreach ( Transform child in transform )
		{
			FogData fg = child.GetComponent<FogData>();
			if ( fg != null && fg.chapterName == chName )
				GameObject.Destroy( child.gameObject );
		}
	}
}
