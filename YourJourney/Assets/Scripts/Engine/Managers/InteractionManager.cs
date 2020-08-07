using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

/// <summary>
/// Keeps track of all Interactions in the game, creating specific interactions based on their type (Threat,Text etc), randomness, and whether they are token interactions
/// </summary>
public class InteractionManager : MonoBehaviour
{
	public List<IInteraction> interactions { get; set; }
	public List<IInteraction> randomInteractions { get; set; }
	public List<IInteraction> tokenInteractions { get; set; }
	public List<IInteraction> randomTokenInteractions { get; set; }
	public bool PanelShowing
	{
		get => uiRoot.childCount > 0;
	}

	public GameObject textPanelPrefab, decisionPanelPrefab, statPanelPrefab, spawnMarkerPrefab;
	public Transform uiRoot;

	[HideInInspector]
	public Engine engine;

	//build list of specific interactions based on each InteractionType in the Scenario
	public void Init( Scenario s )
	{
		engine = FindObjectOfType<Engine>();

		interactions = new List<IInteraction>();

		//filter into separate lists

		//NOT random, NOT token
		var ints = s.interactionObserver.Where( x => !x.dataName.Contains( "GRP" ) && !x.isTokenInteraction );
		//IS random, NOT token
		var randomints = s.interactionObserver.Where( x => x.dataName.Contains( "GRP" ) && !x.isTokenInteraction );
		//IS token, random or not
		var toks = s.interactionObserver.Where( x => x.isTokenInteraction );
		//IS random, IS token
		var randomtoks = s.interactionObserver.Where( x => x.dataName.Contains( "GRP" ) && x.isTokenInteraction );

		interactions = ints.Select( x => CreateInteraction( x ) ).ToList();
		randomInteractions = randomints.Select( x => CreateInteraction( x ) ).ToList();
		tokenInteractions = toks.Select( x => CreateInteraction( x ) ).ToList();
		randomTokenInteractions = randomtoks.Select( x => CreateInteraction( x ) ).ToList();

		//Debug.Log( "interactions: " + interactions.Count() );
		//Debug.Log( "randomInteractions: " + randomInteractions.Count() );
		//Debug.Log( "tokenInteractions: " + tokenInteractions.Count() );
		//Debug.Log( "randomTokenInteractions: " + randomTokenInteractions.Count() );
	}

	/// <summary>
	/// Create cast to specific Interaction TYPE based on the type
	/// </summary>
	IInteraction CreateInteraction( IInteraction interaction )
	{
		if ( interaction.interactionType == InteractionType.Text )
			return (TextInteraction)interaction;
		else if ( interaction.interactionType == InteractionType.Threat )
			return (ThreatInteraction)interaction;
		else if ( interaction.interactionType == InteractionType.StatTest )
			return (StatTestInteraction)interaction;
		else if ( interaction.interactionType == InteractionType.Decision )
			return (DecisionInteraction)interaction;
		else if ( interaction.interactionType == InteractionType.Branch )
			return (BranchInteraction)interaction;
		else if ( interaction.interactionType == InteractionType.Darkness )
			return (DarknessInteraction)interaction;

		throw new Exception( "Couldn't create Interaction from: " + interaction.dataName );
	}

	public TextPanel GetNewTextPanel()
	{
		return Instantiate( textPanelPrefab, uiRoot ).transform.Find( "TextPanel" ).GetComponent<TextPanel>();
	}

	public DecisionPanel GetNewDecisionPanel()
	{
		return Instantiate( decisionPanelPrefab, uiRoot ).transform.Find( "DecisionPanel" ).GetComponent<DecisionPanel>();
	}

	public StatTestPanel GetNewStatPanel()
	{
		return Instantiate( statPanelPrefab, uiRoot ).transform.Find( "StatTestPanel" ).GetComponent<StatTestPanel>();
	}

	/// <summary>
	/// Asks if want to use an action on a specified token interaction name.
	/// This is fired when player clicks a token on a tile
	/// </summary>
	public void QueryTokenInteraction( string name, string btnText, Action<InteractionResult> callback )
	{
		//Debug.Log( "QueryTokenInteraction: " + name );
		//remove any MARKERS
		var objs = FindObjectsOfType<SpawnMarker>();
		foreach ( var ob in objs )
		{
			if ( ob.name.Contains( "SPAWNMARKER" ) )
				Destroy( ob.gameObject );
			else if ( ob.name == "STARTMARKER" )
				ob.gameObject.SetActive( false );
		}

		if ( tokenInteractions.Count > 0 )
		{
			if ( tokenInteractions.Any( x => x.dataName == name ) )
			{
				IInteraction it = tokenInteractions.Where( x => x.dataName == name ).First();
				GetNewTextPanel().ShowQueryInteraction( it, btnText, ( res ) =>
				{
					res.interaction = it;
					res.removeToken = true;

					//DO NOT remove the token if it's a test
					if ( it is StatTestInteraction )
						res.removeToken = false;
					callback?.Invoke( res );
				} );
			}
			else
				GetNewTextPanel().ShowOkContinue( $"Data Error (QueryTokenInteraction)\r\nCould not find Interaction with name '{name}'.", ButtonIcon.Continue );
		}
	}

	/// <summary>
	/// Try to fire NON-RANDOM, NON-TOKEN interaction based on name
	/// </summary>
	public bool TryFireInteraction( string name )
	{
		Debug.Log( "TryFireInteraction: " + name );
		if ( interactions.Any( x => x.dataName == name ) )
		{
			Debug.Log( "Found Interaction: " + name );
			ShowInteraction( interactions.Where( x => x.dataName == name ).First() );
			return true;
		}
		//else
		//	Debug.Log( "COULDN'T FIRE INTERACTION: " + name );
		return false;
	}

	public bool TryFireEndScenario( string name )
	{
		if ( engine.scenario.resolutionObserver.Any( x => x.triggerName == name ) )
		{
			var text = engine.scenario.resolutionObserver.Where( x => x.triggerName == name ).First();
			GetNewTextPanel().ShowOkContinue( text.pages[0], ButtonIcon.Continue );
			return true;
		}
		return false;

	}

	/// <summary>
	/// Show interaction based on type
	/// </summary>
	public void ShowInteraction( IInteraction it, Transform source = null, Action<InteractionResult> action = null )
	{
		if ( it.interactionType == InteractionType.Text )
			HandleText( it );
		else if ( it.interactionType == InteractionType.Threat )
		{
			HandleThreat( it, source ? source.position : ( -1000f ).ToVector3() );
		}
		else if ( it.interactionType == InteractionType.Decision )
		{
			HandleDecision( it );
		}
		else if ( it.interactionType == InteractionType.Branch )
		{
			HandleBranch( it );
		}
		else if ( it.interactionType == InteractionType.StatTest )
		{
			HandleStatTest( it, action );
		}
		else
			GetNewTextPanel().ShowOkContinue( $"Data Error (ShowInteraction)\r\nCould not find Interaction with type '{it.interactionType}'.", ButtonIcon.Continue );
	}

	void HandleText( IInteraction it )
	{
		GetNewTextPanel().ShowTextInteraction( it, () =>
		{
			engine.triggerManager.FireTrigger( it.triggerAfterName );
		} );
	}

	void HandleThreat( IInteraction it, Vector3 position )
	{
		GetNewTextPanel().ShowTextInteraction( it, () =>
		{
			engine.triggerManager.FireTrigger( it.triggerAfterName );
			GetNewTextPanel().ShowOkContinue( "Place the enemy figures in the indicated position.", ButtonIcon.Continue );
		} );
		FindObjectOfType<MonsterManager>().AddNewMonsterGroup( ( (ThreatInteraction)it ).monsterCollection.ToArray(), it );
		if ( position.x != -1000 )
		{
			var go = Instantiate( spawnMarkerPrefab, position, Quaternion.identity );
		}
	}

	void HandleDecision( IInteraction it )
	{
		GetNewDecisionPanel().Show( (DecisionInteraction)it, ( res ) =>
		{
			if ( res.btn1 )
				engine.triggerManager.FireTrigger( ( (DecisionInteraction)it ).choice1Trigger );
			else if ( res.btn2 )
				engine.triggerManager.FireTrigger( ( (DecisionInteraction)it ).choice2Trigger );
			else if ( res.btn3 )
				engine.triggerManager.FireTrigger( ( (DecisionInteraction)it ).choice3Trigger );
			engine.triggerManager.FireTrigger( it.triggerAfterName );
		} );
	}

	void HandleBranch( IInteraction it )
	{
		( (BranchInteraction)it ).Resolve( this );
		engine.triggerManager.FireTrigger( it.triggerAfterName );
	}

	void HandleStatTest( IInteraction it, Action<InteractionResult> action = null )
	{
		GetNewStatPanel().Show( (StatTestInteraction)it, ( b ) =>
		{
			StatTestInteraction sti = (StatTestInteraction)it;

			if ( b.success )//show success textbox
			{
				GetNewTextPanel().ShowOkContinue( sti.passBookData.pages[0], ButtonIcon.Continue, () => { engine.triggerManager.FireTrigger( it.triggerAfterName ); } );
				engine.triggerManager.FireTrigger( sti.successTrigger );
				action?.Invoke( new InteractionResult() { removeToken = true } );
			}
			else if ( !b.btn4 && !b.success )//show fail textbox
			{
				GetNewTextPanel().ShowOkContinue( sti.failBookData.pages[0], ButtonIcon.Continue, () => { engine.triggerManager.FireTrigger( it.triggerAfterName ); } );
				engine.triggerManager.FireTrigger( sti.failTrigger );
				action?.Invoke( new InteractionResult() { removeToken = true } );
			}
			else if ( b.btn4 )//show progress or success box
			{
				bool success = ( (StatTestInteraction)it ).ResolveCumulative( b.value, engine );
				if ( success )
				{
					GetNewTextPanel().ShowOkContinue( sti.passBookData.pages[0], ButtonIcon.Continue, () => { engine.triggerManager.FireTrigger( it.triggerAfterName ); } );
					engine.triggerManager.FireTrigger( sti.successTrigger );
					action?.Invoke( new InteractionResult() { removeToken = true } );
				}
				else
				{
					GetNewTextPanel().ShowOkContinue( sti.progressBookData.pages[0], ButtonIcon.Continue, () => { engine.triggerManager.FireTrigger( it.triggerAfterName ); } );
					action?.Invoke( new InteractionResult() { removeToken = false } );
				}
			}
		} );
	}
}
