using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FightManager : MonoBehaviour
{
	Monster theMonster;
	InteractionManager im;
	MonsterManager mm;

	//elite enemies are able to perform counterattacks even	when they are exhausted
	//stunning exhausts a group - if the group is elite it also cannot counterattack this attack

	public void Provoke( Monster monster )
	{
		//provoking does NOT exhaust the group
		im = FindObjectOfType<InteractionManager>();
		mm = FindObjectOfType<MonsterManager>();
		theMonster = monster;

		im.GetNewDamagePanel().ShowCombatCounter( monster, () =>
		{
			mm.UnselectAll();
		} );
	}

	/// <summary>
	/// Shadow Phase monster activation
	/// </summary>
	//public IEnumerator MonsterStep( Monster monster )
	//{
	//	var im = FindObjectOfType<InteractionManager>();
	//	var mm = FindObjectOfType<MonsterManager>();
	//	var tp = im.GetNewTextPanel();
	//	string heroName = Bootstrap.GetRandomHero();
	//	InteractionResult iResult = null;
	//	//select monster button group
	//	mm.SelectMonster( monster, true );
	//	//ask if it can move and attack
	//	bool waiting = true;
	//	bool allowAttacks = true;
	//	Guid allowedMonsterGUID = monster.GUID;

	//	tp.ShowYesNo( $"Move {monster.movementValue}: Attack {heroName} or closest Hero.\r\n\r\nCan this enemy group attack?\r\n\r\nIf you have an ability to attack this enemy group, do it now.", res =>
	//	{
	//		waiting = false;
	//		iResult = res;
	//	} );
	//	//wait
	//	while ( waiting )
	//	{
	//		if ( monster.ActiveMonsterCount == 0
	//			|| ( !monster.isElite && monster.isExhausted )
	//			|| ( monster.isElite && monster.isStunned ) )
	//			waiting = false;
	//		yield return null;
	//	}

	//	allowAttacks = false;

	//	//check if monster group is dead/exhausted and abort this monter's attack if needed
	//	if ( monster.ActiveMonsterCount == 0
	//		|| ( !monster.isElite && monster.isExhausted )
	//		|| ( monster.isElite && monster.isStunned ) )
	//	{
	//		tp.RemoveBox();
	//		waiting = true;
	//		if ( monster.ActiveMonsterCount == 0 )
	//			im.GetNewTextPanel().ShowOkContinue( $"This enemy group's activation is canceled.\r\n\r\nRemove this enemy group from the board and gain {monster.deadCount} Inspiration.", ButtonIcon.Continue, () => { waiting = false; } );
	//		else
	//			im.GetNewTextPanel().ShowOkContinue( "This enemy group has become exhausted.  Activation is canceled.", ButtonIcon.Continue, () => { waiting = false; } );
	//		while ( waiting )
	//			yield return null;
	//	}
	//	else
	//	{
	//		if ( iResult.btn1 )//yes, attack
	//		{
	//			Debug.Log( "***YES ATTACK" );
	//			waiting = true;
	//			FindObjectOfType<InteractionManager>().GetNewStatPanel().ShowCombatCounter( monster, r => waiting = false );
	//			//wait
	//			while ( waiting )
	//				yield return null;
	//			//exhaust the enemy
	//			FindObjectOfType<MonsterManager>().ExhaustMonster( monster, true );
	//		}
	//		else //if ( iResult.btn2 )//no target, just move
	//		{
	//			Debug.Log( "***NO ATTACK" );
	//			waiting = true;
	//			im.GetNewTextPanel().ShowOkContinue( $"Move {monster.dataName} group {monster.movementValue * 2} spaces towards {heroName}.", ButtonIcon.Continue, () => waiting = false );
	//			//wait
	//			while ( waiting )
	//				yield return null;
	//			//exhaust the enemy
	//			FindObjectOfType<MonsterManager>().ExhaustMonster( monster, true );
	//		}
	//	}




	//	yield return null;
	//}
}
