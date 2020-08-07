using System;
using System.Collections;
using UnityEngine;

public class ShadowPhaseManager : MonoBehaviour
{
	public PhaseNotification phaseNotification;
	public EndTurnButton endTurnButton;

	[HideInInspector]
	public bool doingShadowPhase = false;
	[HideInInspector]
	public bool allowAttacks = false;
	[HideInInspector]
	public Guid allowedMonsterGUID;

	public void EndTurn()
	{
		if ( doingShadowPhase )
			return;

		Debug.Log( "***STARTED SHADOW PHASE" );
		allowAttacks = false;
		doingShadowPhase = true;
		//go thru each monster, ask if it can move+attack random hero
		//attack OR no target
		//if no attack, make it move towards nearest
		//advance threat
		//trigger threat if threshold reached

		//TODO - disable group button
		StartCoroutine( EndTurnSequence() );
	}

	IEnumerator EndTurnSequence()
	{
		Debug.Log( "***STARTED COROUTINE" );
		//SHADOW PHASE announcement
		phaseNotification.Show( "Shadow Phase" );
		yield return new WaitForSeconds( 3 );

		//ENEMY ACTIVATION STEP
		Debug.Log( "***ENEMY ACTIVATION STEP" );
		Monster[] monsters = FindObjectOfType<MonsterManager>().monsterList.ToArray();
		var im = FindObjectOfType<InteractionManager>();
		bool waiting = true;

		//foreach ( var monster in monsters )
		for ( int i = 0; i < monsters.Length; i++ )
		{
			FindObjectOfType<MonsterManager>().UnselectAll();

			if ( monsters[i].isExhausted )
				continue;

			Debug.Log( "***MONSTER ACTIVATING" );
			yield return new WaitForSeconds( 1 );

			string heroName = Bootstrap.GetRandomHero();
			InteractionResult iResult = null;
			//select monster button group
			FindObjectOfType<MonsterManager>().SelectMonster( monsters[i], true );
			//ask if it can move and attack
			waiting = true;
			allowAttacks = true;
			allowedMonsterGUID = monsters[i].GUID;
			var tp = im.GetNewTextPanel();
			tp.ShowYesNo( $"Move {monsters[i].movementValue}: Attack {heroName} or closest Hero.\r\n\r\nCan this enemy group attack?\r\n\r\nIf you have an ability to attack this enemy group, do it now.", res =>
			{
				waiting = false;
				iResult = res;
			} );
			//wait
			while ( waiting )
			{
				if ( monsters[i].ActiveMonsterCount == 0
					|| ( !monsters[i].isElite && monsters[i].isExhausted )
					|| ( monsters[i].isElite && monsters[i].isStunned ) )
					waiting = false;
				yield return null;
			}

			allowAttacks = false;

			//check if monster group is dead/exhausted and abort this monter's attack if needed
			if ( monsters[i].ActiveMonsterCount == 0
				|| ( !monsters[i].isElite && monsters[i].isExhausted )
				|| ( monsters[i].isElite && monsters[i].isStunned ) )
			{
				tp.RemoveBox();
				waiting = true;
				if ( monsters[i].ActiveMonsterCount == 0 )
					im.GetNewTextPanel().ShowOkContinue( $"This enemy group's activation is canceled.\r\n\r\nRemove this enemy group from the board and gain {monsters[i].deadCount} Inspiration.", ButtonIcon.Continue, () => { waiting = false; } );
				else
					im.GetNewTextPanel().ShowOkContinue( "This enemy group has become exhausted.  Activation is canceled.", ButtonIcon.Continue, () => { waiting = false; } );
				while ( waiting )
					yield return null;
			}
			else
			{
				if ( iResult.btn1 )//yes, attack
				{
					Debug.Log( "***YES ATTACK" );
					waiting = true;
					FindObjectOfType<InteractionManager>().GetNewStatPanel().ShowCombatCounter( monsters[i], r => waiting = false );
					//wait
					while ( waiting )
						yield return null;
					//exhaust the enemy
					FindObjectOfType<MonsterManager>().ExhaustMonster( monsters[i], true );
				}
				else //if ( iResult.btn2 )//no target, just move
				{
					Debug.Log( "***NO ATTACK" );
					waiting = true;
					im.GetNewTextPanel().ShowOkContinue( $"Move {monsters[i].dataName} group {monsters[i].maxMovementValue} spaces towards {heroName}.", ButtonIcon.Continue, () => waiting = false );
					//wait
					while ( waiting )
						yield return null;
					//exhaust the enemy
					FindObjectOfType<MonsterManager>().ExhaustMonster( monsters[i], true );
				}
			}
		}

		FindObjectOfType<MonsterManager>().UnselectAll();

		//DARKNESS STEP
		yield return DarknessStep();

		//THREAT STEP
		yield return ThreatStep();

		//FINISH UP
		yield return FinishShadowPhase();

		//yield return new WaitForSeconds( 3 );
		//waiting = true;
		//im.GetNewTextPanel().ShowOkContinue( "Each Hero resets their deck and Scouts 2.", ButtonIcon.Continue, () => waiting = false );
		////wait
		//while ( waiting )
		//	yield return null;

		//FindObjectOfType<MonsterManager>().ReadyAll();
		////ACTION PHASE announcement
		//phaseNotification.Show( "Action Phase" );
		//yield return new WaitForSeconds( 3 );

		//finally end shadow phase
		doingShadowPhase = false;
		Debug.Log( "***ENDED COROUTINE" );

		//SAVE PROGRESS HERE

	}

	IEnumerator DarknessStep()
	{
		Debug.Log( "***DARKNESS STEP" );
		yield return null;
	}

	IEnumerator ThreatStep()
	{
		Debug.Log( "***THREAT STEP" );
		//add up threat
		//2*hero count + # unexplored tiles + 1 per threat token
		int hc = 2 * Bootstrap.heroes.Length;
		int ut = FindObjectOfType<TileManager>().UnexploredTileCount();
		int tt = FindObjectOfType<TileManager>().ThreatTokenCount();
		Debug.Log( "***hero threat: " + hc );
		Debug.Log( "***unexplored threat: " + ut );
		Debug.Log( "***threat token threat: " + tt );

		Threat t = endTurnButton.AddThreat( hc + ut + tt );
		//wait for animation
		yield return new WaitForSeconds( 2 );

		if ( t != null )
		{
			Debug.Log( "***FIRING THREAT TRIGGERS" );
			FindObjectOfType<TriggerManager>().FireTrigger( t.triggerName );
			//wait until all triggers finished
			yield return WaitUntilFinished();
		}
		else
		{
			Debug.Log( "***NO THREAT TRIGGERS FOUND" );
			//yield return FinishShadowPhase();
		}
	}

	/// <summary>
	/// shows rally and action phase with appropriate messages
	/// </summary>
	IEnumerator FinishShadowPhase()
	{
		var im = FindObjectOfType<InteractionManager>();
		//RALLY PHASE announcement
		phaseNotification.Show( "Rally Phase" );
		yield return new WaitForSeconds( 3 );

		bool waiting = true;
		im.GetNewTextPanel().ShowOkContinue( "Each Hero resets their deck and Scouts 2.", ButtonIcon.Continue, () =>
		{
			waiting = false;
		} );

		while ( waiting )
			yield return null;

		FindObjectOfType<MonsterManager>().ReadyAll();
		//ACTION PHASE announcement
		phaseNotification.Show( "Action Phase" );
		//wait for action phase animation
		yield return new WaitForSeconds( 3 );
		Debug.Log( "***FINISHED SHADOW PHASE" );
	}

	IEnumerator WaitUntilFinished()
	{
		Debug.Log( "***WAITING..." );
		var im = FindObjectOfType<InteractionManager>();
		//wait for all UI screens to go away - signifies all triggers finished
		//poll IManager uiRoot for children (panels)
		while ( im.PanelShowing )
			yield return null;
		Debug.Log( "***DONE WAITING..." );

		//finally finish shadow phase
		//yield return FinishShadowPhase();
	}
}
