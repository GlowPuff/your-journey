﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

/// <summary>
/// Keeps track of all Interactions in the game, creating specific interactions based on their type (Threat,Text etc), randomness, and whether they are token interactions
/// </summary>
public class InteractionManager : MonoBehaviour
{
	public List<IInteraction> interactions { get; set; }//not random/token
	public List<IInteraction> randomInteractions { get; set; }
	public List<IInteraction> tokenInteractions { get; set; }
	public List<IInteraction> randomTokenInteractions { get; set; }
	public List<IInteraction> allInteractions { get; set; }
	public bool PanelShowing
	{
		get => uiRoot.childCount > 0;
	}

	public GameObject textPanelPrefab, decisionPanelPrefab, statPanelPrefab, spawnMarkerPrefab, damagePanelPrefab, dialogPanelPrefab;
	public Transform uiRoot;

	[HideInInspector]
	public Engine engine;

	//build list of specific interactions based on each InteractionType in the Scenario
	public void Init( Scenario s )
	{
		engine = FindObjectOfType<Engine>();

		interactions = new List<IInteraction>();

		//first, create a multi-event out of each Trigger that is a multi-trigger
		var multitriggers = engine.scenario.triggersObserver.Where( x => x.isMultiTrigger );
		foreach ( var mt in multitriggers )
		{
			var multiEvent = new MultiEventInteraction();
			multiEvent.GUID = Guid.NewGuid();
			multiEvent.dataName = "converted multievent";
			multiEvent.isEmpty = false;
			multiEvent.triggerName = mt.dataName;//triggered by this trigger
			multiEvent.triggerAfterName = "None";
			multiEvent.isTokenInteraction = false;
			multiEvent.tokenType = TokenType.None;
			multiEvent.eventList = new List<string>();
			multiEvent.triggerList = new List<string>();
			multiEvent.usingTriggers = false;
			multiEvent.isSilent = true;
			multiEvent.interactionType = InteractionType.MultiEvent;

			//add any events listening for this trigger to the eventlist
			//foreach ( var t in s.interactionObserver )
			//	Debug.Log( t.triggerName );
			var listeners = s.interactionObserver.Where( x => x.triggerName == mt.dataName && x.triggerName != "None" );

			if ( listeners.Count() > 0 )
			{
				//Debug.Log( "MultiTrigger looking for: " + mt.dataName + "::found: " + listeners.Count() );
				foreach ( var ev in listeners )
				{
					ev.triggerName = "None";
					multiEvent.eventList.Add( ev.dataName );
				}
				//Debug.Log( "added multievent to interactionObserver" );
				//Debug.Log( "Created MultiEvent placeholder with Events: " + multiEvent.eventList.Count );
				s.interactionObserver.Add( multiEvent );
			}
		}

		//filter into separate lists

		//ALL interactions
		allInteractions = s.interactionObserver.ToList();
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
	/// Cast to specific Interaction based on the type
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
		else if ( interaction.interactionType == InteractionType.MultiEvent )
			return (MultiEventInteraction)interaction;
		else if ( interaction.interactionType == InteractionType.Persistent )
			return (PersistentInteraction)interaction;
		else if ( interaction.interactionType == InteractionType.Conditional )
			return (ConditionalInteraction)interaction;
		else if ( interaction.interactionType == InteractionType.Dialog )
			return (DialogInteraction)interaction;
		else if ( interaction.interactionType == InteractionType.Replace )
			return (ReplaceTokenInteraction)interaction;
		else if ( interaction.interactionType == InteractionType.Reward )
			return (RewardInteraction)interaction;

		throw new Exception( "Couldn't create Interaction from: " + interaction.dataName );
	}

	public IInteraction GetInteractionByName( string name )
	{
		if ( allInteractions.Any( x => x.dataName == name ) )
			return allInteractions.Where( x => x.dataName == name ).First();
		else
			throw new Exception( "Couldn't find Interaction: " + name );//this condition should never happen
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

	public DamagePanel GetNewDamagePanel()
	{
		return Instantiate( damagePanelPrefab, uiRoot ).transform.Find( "DamagePanel" ).GetComponent<DamagePanel>();
	}

	public DialogPanel GetNewDialogPanel()
	{
		return Instantiate( dialogPanelPrefab, uiRoot ).transform.Find( "DialogPanel" ).GetComponent<DialogPanel>();
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
			if ( ob.name == "STARTMARKER" )
				ob.gameObject.SetActive( false );
		}

		if ( tokenInteractions.Count > 0 )
		{
			if ( tokenInteractions.Any( x => x.dataName == name ) )
			{
				IInteraction it = tokenInteractions.Where( x => x.dataName == name ).First();

				//special case for persistent events
				if ( it.interactionType == InteractionType.Persistent && FindObjectOfType<TriggerManager>().IsTriggered( ( (PersistentInteraction)it ).alternativeTextTrigger ) )
				{
					GetNewTextPanel().ShowOkContinue( ( (PersistentInteraction)it ).alternativeBookData.pages[0], ButtonIcon.Continue );
				}
				else if ( it.interactionType == InteractionType.Dialog )
				{
					//skip interaction, go right to the event which shows its own text box
					callback.Invoke( new InteractionResult() { btn2 = true, interaction = it } );
				}
				else if ( it.interactionType == InteractionType.Text )
				{
					//skip flavor if it's already been activated once
					//hasActivated is only set if event isPersistent
					if ( ( (TextInteraction)it ).hasActivated )
					{
						callback.Invoke( new InteractionResult() { btn2 = true, interaction = it } );
					}
					else
					{
						GetNewTextPanel().ShowQueryInteraction( it, btnText, ( res ) =>
						{
							res.interaction = it;
							callback?.Invoke( res );
						} );
					}
				}
				else
				{
					GetNewTextPanel().ShowQueryInteraction( it, btnText, ( res ) =>
					{
						res.interaction = it;

						callback?.Invoke( res );
					} );
				}
			}
			else
				GetNewTextPanel().ShowOkContinue( $"Data Error (QueryTokenInteraction)\r\nCould not find Interaction with name '{name}'.", ButtonIcon.Continue );
		}
	}

	/// <summary>
	/// Try to fire NON-RANDOM, NON-TOKEN Event based on EVENT NAME
	/// </summary>
	public bool TryFireEventByName( string name )
	{
		//Debug.Log( "TryFireEventByName: " + name );
		if ( interactions.Any( x => x.dataName == name ) )
		{
			Debug.Log( "TryFireEventByName() FOUND Event: " + name );
			ShowInteraction( interactions.Where( x => x.dataName == name ).First() );
			return true;
		}
		//else
		//	Debug.Log( "Couldn't find Event with name: " + name );
		return false;
	}

	/// <summary>
	/// Try to fire NON-RANDOM, NON-TOKEN Event based on TRIGGER NAME
	/// </summary>
	public bool TryFireEventByTrigger( string triggername )
	{
		//Debug.Log( "TryFireEventByTrigger: " + triggername );
		if ( interactions.Any( x => x.triggerName == triggername ) )
		{
			int count = interactions.Count( x => x.triggerName == triggername );
			Debug.Log( "TryFireEventByTrigger() FOUND " + count + " Event(s): " + triggername );
			if ( count == 1 )
			{
				ShowInteraction( interactions.Where( x => x.triggerName == triggername ).First() );
			}
			else
			{
				Debug.Log( "Randomly choosing one of them..." );
				var inters = interactions.Where( x => x.triggerName == triggername ).ToArray();
				ShowInteraction( inters[UnityEngine.Random.Range( 0, count )] );
			}
			return true;
		}
		//else
		//	Debug.Log( "Couldn't find Event listening to Trigger: " + triggername );
		return false;
	}

	public bool TryFireEndScenario( string name )
	{
		if ( engine.scenario.resolutionObserver.Any( x => x.triggerName == name ) )
		{
			var text = engine.scenario.resolutionObserver.Where( x => x.triggerName == name ).First();
			GetNewTextPanel().ShowOkContinue( text.pages[0], ButtonIcon.Continue );
			//handle end game
			engine.triggerManager.TriggerEndGame( text.dataName );
			return true;
		}
		return false;
	}

	/// <summary>
	/// Show interaction based on type. "Source" and "action" are NULL ONLY when NON-TOKENS call this method
	/// </summary>
	public void ShowInteraction( IInteraction it, Transform source = null, Action<InteractionResult> action = null )
	{
		if ( it.interactionType == InteractionType.Text )
			HandleText( it, action );
		else if ( it.interactionType == InteractionType.Threat )
		{
			HandleThreat( it, source ? source.position : ( -1000f ).ToVector3(), action );
		}
		else if ( it.interactionType == InteractionType.Decision )
		{
			HandleDecision( it, action );
		}
		else if ( it.interactionType == InteractionType.Branch )
		{
			HandleBranch( it, action );
		}
		else if ( it.interactionType == InteractionType.StatTest )
		{
			HandleStatTest( it, action );
		}
		else if ( it.interactionType == InteractionType.MultiEvent )
		{
			HandleMultiEvent( it, action );
		}
		else if ( it.interactionType == InteractionType.Persistent )
		{
			HandlePersistent( it, action );
		}
		else if ( it.interactionType == InteractionType.Conditional )
		{
			HandleConditional( it, action );
		}
		else if ( it.interactionType == InteractionType.Dialog )
		{
			HandleDialog( it, action );
		}
		else if ( it.interactionType == InteractionType.Replace )
		{
			HandleReplacement( it, action );
		}
		else if ( it.interactionType == InteractionType.Reward )
		{
			HandleReward( it, action );
		}
		else
			GetNewTextPanel().ShowOkContinue( $"Data Error (ShowInteraction)\r\nCould not find Interaction with type '{it.interactionType}'.", ButtonIcon.Continue );
	}

	void HandleText( IInteraction it, Action<InteractionResult> action )
	{
		TextInteraction textInteraction = it as TextInteraction;

		//persistent or not, NOT activated
		if ( !textInteraction.hasActivated )
		{
			GetNewTextPanel().ShowTextInteraction( it, () =>
			{
				engine.triggerManager.FireTrigger( it.triggerAfterName );
				FindObjectOfType<LorePanel>().AddReward( it.loreReward, it.xpReward, it.threatReward );
				if ( textInteraction.isTokenInteraction && textInteraction.isPersistent )
				{
					textInteraction.hasActivated = true;
					action?.Invoke( new InteractionResult() { removeToken = false } );
				}
				else
					action?.Invoke( new InteractionResult() { removeToken = true } );
			} );
		}//is token, persistent AND activated
		else if ( textInteraction.isTokenInteraction
			&& textInteraction.isPersistent
			&& textInteraction.hasActivated )
		{
			if ( !string.IsNullOrEmpty( textInteraction.persistentText ) )
				GetNewTextPanel().ShowOkContinue( textInteraction.persistentText, ButtonIcon.Continue, () =>
				 {
					 action?.Invoke( new InteractionResult() { removeToken = false } );
				 } );
			else
				GetNewTextPanel().ShowOkContinue( textInteraction.eventBookData.pages[0], ButtonIcon.Continue, () =>
				{
					action?.Invoke( new InteractionResult() { removeToken = false } );
				} );
		}
	}

	void HandleThreat( IInteraction it, Vector3 position, Action<InteractionResult> action )
	{
		List<Vector3> positions = new List<Vector3>();

		//generate the encounter using Pool System
		List<Monster> poolList = ( (ThreatInteraction)it ).GenerateEncounter();

		//get VALID (correct difficulty) monsters, both pooled and scripted
		var allMonsters = poolList.Concat( ( (ThreatInteraction)it ).monsterCollection.Where( m => m.IsValid() ) );

		//int groupCount = ( (ThreatInteraction)it ).monsterCollection.Where( m => m.IsValid() ).Count();
		int groupCount = allMonsters.Count();

		Vector3[] opentf = FindObjectOfType<TileManager>().GetAvailableSpawnPositions();

		int[] rnds = GlowEngine.GenerateRandomNumbers( opentf.Length );

		Debug.Log( "Found " + opentf.Length + " positions" );
		//if it's NOT a token interaction, figure out WHERE to spawn the group
		if ( !it.isTokenInteraction )
		{
			//TODO: SORT BY CLOSEST DISTANCE?
			if ( opentf.Length > 0 )//should never be 0, sanity check
			{
				//only use as many positions as exist
				for ( int i = 0; i < Math.Min( groupCount, opentf.Length ); i++ )
					positions.Add( opentf[rnds[i]] );
				Debug.Log( "HandleThreat::Using random location" );
			}
			else
			{
				Debug.Log( "HandleThreat::Could not find a random location" );
				return;
			}
		}
		else//it's a fixed token interaction
		{
			//get positions as close as possible to event that spawns this
			Vector3[] nearest = opentf.OrderBy( x => Vector3.Distance( x, position ) ).ToArray();
			//use as many positions as possible
			for ( int i = 0; i < Math.Min( groupCount, nearest.Length ); i++ )
				positions.Add( nearest[i] );//[rnds[i]] );
		}

		GetNewTextPanel().ShowTextInteraction( it, () =>
		{
			StartCoroutine( MonsterPlacementPrompt( allMonsters.ToArray(), positions.ToArray(), it ) );

			action?.Invoke( new InteractionResult() { removeToken = true } );

			engine.triggerManager.FireTrigger( it.triggerAfterName );
		} );
	}

	void HandleDecision( IInteraction it, Action<InteractionResult> action )
	{
		GetNewDecisionPanel().Show( (DecisionInteraction)it, ( res ) =>
		{
			if ( res.btn1 )
				engine.triggerManager.FireTrigger( ( (DecisionInteraction)it ).choice1Trigger );
			else if ( res.btn2 )
				engine.triggerManager.FireTrigger( ( (DecisionInteraction)it ).choice2Trigger );
			else if ( res.btn3 )
				engine.triggerManager.FireTrigger( ( (DecisionInteraction)it ).choice3Trigger );
			action?.Invoke( new InteractionResult() { removeToken = true } );
			engine.triggerManager.FireTrigger( it.triggerAfterName );
			FindObjectOfType<LorePanel>().AddReward( it.loreReward, it.xpReward, it.threatReward );
		} );
	}

	void HandleBranch( IInteraction it, Action<InteractionResult> action )
	{
		BranchInteraction bi = it as BranchInteraction;

		void func()
		{
			bi.Resolve( this );
			action?.Invoke( new InteractionResult() { removeToken = true } );
			engine.triggerManager.FireTrigger( it.triggerAfterName );
			FindObjectOfType<LorePanel>().AddReward( it.loreReward, it.xpReward, it.threatReward );
		}

		//only show event text if it's NOT empty
		if ( !string.IsNullOrEmpty( bi.eventBookData.pages[0].Trim() ) )
		{
			GetNewTextPanel().ShowTextInteraction( it, () =>
			{
				func();
			} );
		}
		else//otherwise silently activate
		{
			func();
		}
	}

	void HandleStatTest( IInteraction it, Action<InteractionResult> action = null )
	{
		GetNewStatPanel().Show( (StatTestInteraction)it, ( b ) =>
		{
			engine.triggerManager.FireTrigger( it.triggerAfterName );

			StatTestInteraction sti = (StatTestInteraction)it;

			if ( b.success )//show success textbox
			{
				GetNewTextPanel().ShowOkContinue( sti.passBookData.pages[0], ButtonIcon.Continue, () =>
				{
					action?.Invoke( new InteractionResult() { removeToken = true } );
				} );
				engine.triggerManager.FireTrigger( sti.successTrigger );
				//success reward
				FindObjectOfType<LorePanel>().AddReward( sti.rewardLore, sti.rewardXP, sti.rewardThreat );
			}
			else if ( !b.btn4 && !b.success )//show fail textbox
			{
				GetNewTextPanel().ShowOkContinue( sti.failBookData.pages[0], ButtonIcon.Continue, () =>
				{
					action?.Invoke( new InteractionResult() { removeToken = true } );
				} );
				engine.triggerManager.FireTrigger( sti.failTrigger );
				//fail reward
				FindObjectOfType<LorePanel>().AddPenalty( sti.failThreat );
			}
			else if ( b.btn4 )//show progress or success box
			{
				bool success = ( (StatTestInteraction)it ).ResolveCumulative( b.value );
				if ( success )//success
				{
					GetNewTextPanel().ShowOkContinue( sti.passBookData.pages[0], ButtonIcon.Continue, () =>
					{
						action?.Invoke( new InteractionResult() { removeToken = true } );
					} );
					engine.triggerManager.FireTrigger( sti.successTrigger );
					//success reward
					FindObjectOfType<LorePanel>().AddReward( sti.rewardLore, sti.rewardXP, sti.rewardThreat );
				}
				else//progressive fail
				{
					GetNewTextPanel().ShowOkContinue( sti.progressBookData.pages[0], ButtonIcon.Continue, () =>
					 {
						 action?.Invoke( new InteractionResult() { removeToken = false } );
					 } );
				}
			}
		} );
		//event reward right after event fired, before test shown
		FindObjectOfType<LorePanel>().AddReward( it.loreReward, it.xpReward, it.threatReward );
	}

	void HandleMultiEvent( IInteraction it, Action<InteractionResult> action )
	{
		if ( ( (MultiEventInteraction)it ).isSilent )
		{
			if ( ( (MultiEventInteraction)it ).usingTriggers )
			{
				foreach ( string t in ( (MultiEventInteraction)it ).triggerList )
					engine.triggerManager.FireTrigger( t );
			}
			else
			{
				foreach ( string t in ( (MultiEventInteraction)it ).eventList )
					engine.triggerManager.FireTrigger( t );
			}
			action?.Invoke( new InteractionResult() { removeToken = true } );
			engine.triggerManager.FireTrigger( it.triggerAfterName );
			FindObjectOfType<LorePanel>().AddReward( it.loreReward, it.xpReward, it.threatReward );
		}
		else
		{
			GetNewTextPanel().ShowTextInteraction( it, () =>
			{
				if ( ( (MultiEventInteraction)it ).usingTriggers )
				{
					foreach ( string t in ( (MultiEventInteraction)it ).triggerList )
						engine.triggerManager.FireTrigger( t );
				}
				else
				{
					foreach ( string t in ( (MultiEventInteraction)it ).eventList )
						engine.triggerManager.FireTrigger( t );
				}
				action.Invoke( new InteractionResult() { removeToken = true } );
				engine.triggerManager.FireTrigger( it.triggerAfterName );
				FindObjectOfType<LorePanel>().AddReward( it.loreReward, it.xpReward, it.threatReward );
			} );
		}
	}

	void HandlePersistent( IInteraction it, Action<InteractionResult> action )
	{
		action?.Invoke( new InteractionResult() { removeToken = false } );
		//persistent events are only delegates for activating a "real" event
		//persistent events don't have Event Text or fire "triggerAfterName"
	}

	void HandleConditional( IInteraction it, Action<InteractionResult> action )
	{
		action?.Invoke( new InteractionResult() { removeToken = false } );
		//conditional events don't have a "triggerAfterName"
		//conditional events aren't "triggeredBy" - they only listen
		//conditional events cannot be token interactions - they work silently
	}

	void HandleDialog( IInteraction it, Action<InteractionResult> action )
	{
		GetNewDialogPanel().Show( it as DialogInteraction, ( res ) =>
		{
			DialogInteraction di = it as DialogInteraction;
			bool remove = di.isDone && !di.isPersistent;

			if ( res.btn1 )
			{
				GetNewTextPanel().ShowOkContinue( di.c1Text, ButtonIcon.Continue, () =>
				{
					engine.triggerManager.FireTrigger( di.c1Trigger );
					action?.Invoke( new InteractionResult() { removeToken = remove } );
				} );
			}
			else if ( res.btn2 )
			{
				GetNewTextPanel().ShowOkContinue( di.c2Text, ButtonIcon.Continue, () =>
				{
					engine.triggerManager.FireTrigger( di.c2Trigger );
					action?.Invoke( new InteractionResult() { removeToken = remove } );
				} );
			}
			else if ( res.btn3 )
			{
				GetNewTextPanel().ShowOkContinue( di.c3Text, ButtonIcon.Continue, () =>
				{
					engine.triggerManager.FireTrigger( di.c3Trigger );
					action?.Invoke( new InteractionResult() { removeToken = remove } );
				} );
			}

			if ( !res.canceled && !di.hasActivated )
			{
				//only add lore and fire trigger the first time it's activated
				di.hasActivated = true;
				engine.triggerManager.FireTrigger( it.triggerAfterName );
				FindObjectOfType<LorePanel>().AddReward( it.loreReward, it.xpReward, it.threatReward );
			}
		} );
	}

	void HandleReplacement( IInteraction it, Action<InteractionResult> action )
	{
		var repevt = it as ReplaceTokenInteraction;
		repevt.hasActivated = true;

		if ( !repevt.noText )
		{
			GetNewTextPanel().ShowTextInteraction( it, () =>
			{
				DoReplacement( repevt );
				action?.Invoke( new InteractionResult() { removeToken = true } );
			} );
		}
		else
			DoReplacement( repevt );
	}

	void HandleReward( IInteraction it, Action<InteractionResult> action )
	{
		RewardInteraction ri = it as RewardInteraction;

		void func()
		{
			action?.Invoke( new InteractionResult() { removeToken = true } );
			engine.triggerManager.FireTrigger( it.triggerAfterName );
			FindObjectOfType<LorePanel>().AddReward( ri.rewardLore, ri.rewardXP, ri.rewardThreat );
		}

		//only show event text if it's NOT empty
		if ( !string.IsNullOrEmpty( ri.eventBookData.pages[0].Trim() ) )
		{
			GetNewTextPanel().ShowTextInteraction( it, () =>
			{
				func();
			} );
		}
		else//otherwise silently activate
		{
			func();
		}
	}

	void DoReplacement( ReplaceTokenInteraction repEvent, bool fromLoad = false, bool isActive = false )
	{
		IInteraction replace = tokenInteractions.Where( x => x.dataName == repEvent.eventToReplace ).FirstOr( null );

		IInteraction repwith = tokenInteractions.Where( x => x.dataName == repEvent.replaceWithEvent ).FirstOr( null );

		MetaData oldmd = null;
		bool found = false;

		if ( replace != null && repwith != null )
		{
			TileGroup[] tgs = engine.tileManager.GetAllTileGroups();
			for ( int i = 0; i < tgs.Length; i++ )
			{
				foreach ( Tile tile in tgs[i].tileList )
				{
					Transform[] tf = tile.GetChildren( "Token(Clone)" );
					for ( int ii = 0; ii < tf.Length; ii++ )
					{
						oldmd = tf[ii].GetComponent<MetaData>();
						//get the token(metadata) pointing to event getting replaced
						if ( oldmd.interactionName == replace.dataName )
						{
							if ( !fromLoad && tile.isExplored && oldmd.gameObject.activeSelf )
							{
								engine.camControl.MoveTo( oldmd.transform.position );
								GetNewTextPanel().ShowOkContinue( "Replace the Token with a " + repwith.tokenType + " Token.", ButtonIcon.Continue, () =>
										{
											//instantiate new prefab, fill in its data
											tgs[i].ReplaceToken( repwith, oldmd, tile );
										} );
							}
							else if ( !fromLoad && !tile.isExplored && !oldmd.gameObject.activeSelf )
							{
								tgs[i].ReplaceToken( repwith, oldmd, tile );
							}
							else//loading state, just swap token
							{
								var newmd = tgs[i].ReplaceToken( repwith, oldmd, tile );
								newmd.gameObject.SetActive( isActive );
							}
							found = true;
							break;
						}
					}
					if ( found )
						break;
				}
				if ( found )
					break;
			}
		}

		engine.triggerManager.FireTrigger( repEvent.triggerAfterName );
		FindObjectOfType<LorePanel>().AddReward( repEvent.loreReward, repEvent.xpReward, repEvent.threatReward );
	}

	IEnumerator MonsterPlacementPrompt( Monster[] monsters, Vector3[] positions, IInteraction it )
	{
		Debug.Log( "**START MonsterPlacementPrompt" );

		int posidx = 0;
		for ( int i = 0; i < monsters.Length; i++ )
		{
			//only monsters in this difficulty
			if ( !monsters[i].IsValid() )
				continue;

			bool waiting = true;
			if ( i < positions.Length )
				posidx = i;

			//place marker and move camera
			Instantiate( spawnMarkerPrefab, positions[posidx], Quaternion.identity );
			FindObjectOfType<CamControl>().MoveTo( positions[posidx] );
			Monster m = monsters[i];

			//add monster group to bar, one at a time
			FindObjectOfType<MonsterManager>().AddMonsterGroup( m, it as ThreatInteraction );

			TextPanel p = FindObjectOfType<InteractionManager>().GetNewTextPanel();
			p.ShowOkContinue( $"Place {m.count} {m.dataName}(s) in the indicated position.", ButtonIcon.Continue, () => waiting = false );
			while ( waiting )
				yield return null;
		}

		Debug.Log( "**END MonsterPlacementPrompt" );
	}

	public InteractionState GetState()
	{
		InteractionState state = new InteractionState();
		//stat events
		var stats = from evt in allInteractions
								where evt.interactionType == InteractionType.StatTest
								select evt;
		foreach ( var stat in stats )
		{
			StatEventState sis = new StatEventState();
			sis.eventGUID = stat.GUID;
			sis.accumulatedValue = ( (StatTestInteraction)stat ).accumulatedValue;
			state.statEventStates.Add( sis );
		}

		//dialog events
		stats = from evt in allInteractions
						where evt.interactionType == InteractionType.Dialog
						select evt;
		foreach ( var stat in stats )
		{
			DialogEventState des = new DialogEventState();
			des.eventGUID = stat.GUID;
			des.hasActivated = ( (DialogInteraction)stat ).hasActivated;
			des.c1Used = ( (DialogInteraction)stat ).c1Used;
			des.c2Used = ( (DialogInteraction)stat ).c2Used;
			des.c3Used = ( (DialogInteraction)stat ).c3Used;
			des.isDone = ( (DialogInteraction)stat ).isDone;
			state.dialogEventStates.Add( des );
		}

		//replace events
		stats = from evt in allInteractions
						where evt.interactionType == InteractionType.Replace
						select evt;
		foreach ( var stat in stats )
		{
			ReplaceEventState res = new ReplaceEventState();
			res.eventGUID = stat.GUID;
			res.hasActivated = ( (ReplaceTokenInteraction)stat ).hasActivated;
			state.replaceEventStates.Add( res );
		}

		//text events
		stats = from evt in allInteractions
						where evt.interactionType == InteractionType.Text
						select evt;
		foreach ( var stat in stats )
		{
			TextEventState tes = new TextEventState();
			tes.eventGUID = stat.GUID;
			tes.hasActivated = ( (TextInteraction)stat ).hasActivated;
			state.textEventStates.Add( tes );
		}

		return state;
	}

	public void SetState( InteractionState istate )
	{
		foreach ( var interaction in allInteractions )
		{
			//stat events
			var state = ( from evt in istate.statEventStates
										where evt.eventGUID == interaction.GUID
										select evt ).FirstOr( null );
			if ( state != null )
			{
				( (StatTestInteraction)interaction ).accumulatedValue = state.accumulatedValue;
			}
			//dialog events
			var dlgstate = ( from evt in istate.dialogEventStates
											 where evt.eventGUID == interaction.GUID
											 select evt ).FirstOr( null );
			if ( dlgstate != null )
			{
				( (DialogInteraction)interaction ).hasActivated = dlgstate.hasActivated;
				( (DialogInteraction)interaction ).c1Used = dlgstate.c1Used;
				( (DialogInteraction)interaction ).c2Used = dlgstate.c2Used;
				( (DialogInteraction)interaction ).c3Used = dlgstate.c3Used;
				( (DialogInteraction)interaction ).isDone = dlgstate.isDone;
			}
			//replace events
			var repstate = ( from evt in istate.replaceEventStates
											 where evt.eventGUID == interaction.GUID
											 select evt ).FirstOr( null );
			if ( repstate != null )
			{
				ReplaceTokenInteraction rti = (ReplaceTokenInteraction)interaction;
				rti.hasActivated = repstate.hasActivated;
			}
			//text events
			var txtstate = ( from evt in istate.textEventStates
											 where evt.eventGUID == interaction.GUID
											 select evt ).FirstOr( null );
			if ( txtstate != null )
			{
				( (TextInteraction)interaction ).hasActivated = txtstate.hasActivated;
			}
		}
	}
}
