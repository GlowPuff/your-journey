using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using System.Collections.Generic;

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
	public Text errorText;

	bool debug = false;

	bool doneLoading = false;

	void Awake()
	{
		System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

		//load settings - music, F/X
		var settings = Bootstrap.LoadSettings();
		Vignette v;
		ColorGrading cg;
		if ( volume.profile.TryGetSettings( out v ) )
			v.active = settings.Item2 == 1;
		if ( volume.profile.TryGetSettings( out cg ) )
			cg.active = settings.Item3 == 1;
		music.enabled = settings.Item1 == 1;

		//load scenario file
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

		fader.gameObject.SetActive( true );
		//build the tiles
		StartCoroutine( BuildScenario() );
		StartCoroutine( BeginGame() );
	}

	IEnumerator BeginGame()
	{
		while ( !doneLoading )
			yield return null;

		if ( Bootstrap.isNewGame )
		{
			endTurnButton.Init( scenario );

			fader.DOFade( 0, 2 ).OnComplete( () =>
			{
				fader.gameObject.SetActive( false );
				StartNewGame();
			} );
		}
		else
		{
			RestoreGame();
			fader.DOFade( 0, 2 ).OnComplete( () =>
			{
				fader.gameObject.SetActive( false );
			} );
		}
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

	public void RestoreGame( bool fromTemp = false )
	{
		Debug.Log( "Restoring..." );
		GameState gameState;
		if ( fromTemp )
			gameState = GameState.LoadStateTemp( scenario );
		else
			gameState = GameState.LoadState( Bootstrap.saveStateIndex );

		if ( gameState == null )
		{
			ShowError( "Could not restore - gamestate is null" );
			return;
		}

		//restore data
		RemoveFogAndMarkers();
		gameState.partyState.SetState();
		endTurnButton.SetState( scenario, gameState.partyState );
		triggerManager.SetState( gameState.triggerState );
		objectiveManager.SetState( gameState.objectiveState );
		chapterManager.SetState( gameState.chapterState );
		FindObjectOfType<CamControl>().SetState( gameState.camState );
		tileManager.SetState( gameState.tileState );
		interactionManager.SetState( gameState.interactionState );
		FindObjectOfType<MonsterManager>().SetState( gameState.monsterState );

		foreach ( FogState fs in gameState.partyState.fogList )
		{
			GameObject fog = Instantiate( tileManager.fogPrefab, transform );
			FogData fg = fog.GetComponent<FogData>();
			fg.chapterName = fs.chapterName;
			fog.transform.position = fs.globalPosition;
		}

		uiControl.interactable = true;
		Debug.Log( "Restored Game" );
	}

	IEnumerator BuildScenario()
	{
		tileManager.BuildScenario();
		yield return null;
		doneLoading = true;
	}

	public void ShowError( string err )
	{
		errorText.text = err;
		Debug.Log( err );
		GlowTimer.SetTimer( 5, () =>
		{
			errorText.text = "";
		} );
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
		else if ( Input.GetKeyDown( KeyCode.S ) )
		{
			if ( FindObjectOfType<ShadowPhaseManager>().doingShadowPhase
				|| FindObjectOfType<InteractionManager>().PanelShowing
				|| FindObjectOfType<ProvokeMessage>().provokeMode )
			{
				ShowError( "Can't QuickSave at this time" );
				return;
			}
			ShowError( "QuickSave State" );
			GameState gs = new GameState();
			gs.SaveStateTemp( this );
		}
		else if ( Input.GetKeyDown( KeyCode.L ) )
		{
			if ( FindObjectOfType<ShadowPhaseManager>().doingShadowPhase
				|| FindObjectOfType<InteractionManager>().PanelShowing
				|| FindObjectOfType<ProvokeMessage>().provokeMode )
			{
				ShowError( "Can't QuickLoad at this time" );
				return;
			}

			ShowError( "QuickLoad State" );
			RestoreGame( true );
		}
		else if ( Input.GetKeyDown( KeyCode.Space ) )
		{
			if ( tileManager.GetAllTileGroups().Length > 0 )
			{
				Vector3 p = tileManager.GetAllTileGroups()[0].groupCenter;
				camControl.MoveTo( p );
			}
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

	public void RemoveFogAndMarkers()
	{
		foreach ( Transform child in transform )
		{
			FogData fg = child.GetComponent<FogData>();
			if ( fg != null )
				Destroy( child.gameObject );
		}

		var objs = FindObjectsOfType<SpawnMarker>();
		foreach ( var ob in objs )
		{
			if ( ob.name.Contains( "SPAWNMARKER" ) )
				Destroy( ob.gameObject );
			if ( ob.name == "STARTMARKER" )
				ob.gameObject.SetActive( false );
		}
	}

	public List<FogState> GetFogState()
	{
		List<FogState> flist = new List<FogState>();
		foreach ( Transform child in transform )
		{
			FogData fg = child.GetComponent<FogData>();
			if ( fg != null )
			{
				FogState fs = new FogState()
				{
					globalPosition = fg.transform.position,
					chapterName = fg.chapterName
				};
				flist.Add( fs );
			}
		}
		return flist;
	}
}
