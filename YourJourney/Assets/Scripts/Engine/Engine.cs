using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

	public bool debug = false;

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
		{
			scenario = Bootstrap.LoadScenario();
		}

		if ( scenario == null )
		{
			fader.gameObject.SetActive( true );
			fader.DOFade( 0, 2 ).OnComplete( () =>
			{
				fader.gameObject.SetActive( false );
				interactionManager.GetNewTextPanel().ShowOkContinue( "Critical Error\r\n\r\nCould not load Scenario.", ButtonIcon.Continue, () =>
				{
					fader.gameObject.SetActive( true );
					fader.DOFade( 1, 2 ).OnComplete( () =>
					{
						//return to title screen
						SceneManager.LoadScene( "title" );
					} );
				} );
			} );

			return;
		}

		//first objective/interaction/trigger are DUMMIES (None), remove them
		scenario.objectiveObserver.RemoveAt( 0 );
		scenario.interactionObserver.RemoveAt( 0 );
		scenario.triggersObserver.RemoveAt( 0 );

		triggerManager.InitCampaignTriggers();
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
		{
			//Debug.Log( "waiting..." );
			yield return null;
		}

		if ( Bootstrap.gameStarter.isNewGame )
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
			//only show intro text if it's not empty
			if ( !string.IsNullOrEmpty( scenario.introBookData.pages[0] ) )
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

						//fire any campaign triggers
						if ( Bootstrap.campaignState != null )
						{
							foreach ( var t in Bootstrap.campaignState.campaignTriggerState )
								triggerManager.FireTrigger( t );
						}
					} );
			}
			else
			{
				uiControl.interactable = true;

				if ( objectiveManager.Exists( scenario.objectiveName ) )
					objectiveManager.TrySetFirstObjective( scenario.objectiveName, () =>
					{
						chapterManager.TryTriggerChapter( "Start", true );
					} );
				else
					chapterManager.TryTriggerChapter( "Start", true );

				//fire any campaign triggers
				if ( Bootstrap.campaignState != null )
				{
					foreach ( var t in Bootstrap.campaignState.campaignTriggerState )
						triggerManager.FireTrigger( t );
				}
			}
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
			gameState = GameState.LoadState( Bootstrap.gameStarter.saveStateIndex );

		if ( gameState == null )
		{
			ShowError( "Could not restore - gamestate is null" );
			return;
		}

		//restore data
		RemoveFogAndMarkers();
		gameState.campaignState?.SetState();
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

	/// <summary>
	/// updates campaign xp/lore/scenario success/failure and saves state.
	/// only updates xp/lore if value is larger than the one already recorded
	/// </summary>
	public void EndScenario( string resName )//Scenario Ended
	{
		//add scenario lore/xp reward
		FindObjectOfType<LorePanel>().AddReward( scenario.loreReward, scenario.xpReward );

		bool success = scenario.scenarioEndStatus[resName];//default reso
		string msg = success ? "S U C C E S S" : "F A I L U R E";
		var text = interactionManager.GetNewTextPanel();
		text.ShowOkContinue( "The Scenario has ended.\r\n\r\n" + msg, ButtonIcon.Continue, () =>
			{
				fader.gameObject.SetActive( true );
				fader.DOFade( 1, 2 ).OnComplete( () =>
				{
					if ( scenario.projectType == ProjectType.Campaign )
					{
						Debug.Log( "ENDING CAMPAIGN" );
						//update campaign lore/xp rewards
						Bootstrap.campaignState.UpdateCampaign( Bootstrap.loreCount, Bootstrap.xpCount, success );
						Bootstrap.returnToCampaign = true;
						//save campaign state
						GameState gs = new GameState();
						gs.SaveCampaignState( Bootstrap.gameStarter.saveStateIndex, Bootstrap.campaignState );
					}
					else
						Debug.Log( "ENDING SCENARIO" );
					//return to title screen
					SceneManager.LoadScene( "title" );
				} );
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
				|| FindObjectOfType<ProvokeMessage>().provokeMode
				|| fader.gameObject.activeSelf )
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
				|| FindObjectOfType<ProvokeMessage>().provokeMode
				|| fader.gameObject.activeSelf )
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
		else if ( Input.GetKeyDown( KeyCode.X ) )
		{
			//debug - save campaign state as if scenario was finished
			ShowError( "DEBUG save campaign - scenario finished" );

		}
	}

	public void OnShowSettings()
	{
		settingsDialog.Show( "Quit to Title", OnQuit );
	}

	public void OnQuit()
	{
		//SAVE PROGRESS
		GameState gs = new GameState();
		gs.SaveState( FindObjectOfType<Engine>(), Bootstrap.gameStarter.saveStateIndex );

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
