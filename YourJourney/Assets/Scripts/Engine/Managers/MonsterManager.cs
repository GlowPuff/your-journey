using UnityEngine;
using System.Collections.Generic;

public class MonsterManager : MonoBehaviour
{
	public CombatPanel combatPanel;
	public GameObject monsterButtonPrefab;

	//Monster selectedMonster;

	[HideInInspector]
	public List<Monster> monsterList = new List<Monster>();

	/// <summary>
	/// Add from saved game
	/// </summary>
	public void AddMonsterGroup( Monster m, IInteraction interaction = null )
	{
		m.interaction = interaction;
		GameObject go = Instantiate( monsterButtonPrefab, transform );
		go.transform.localPosition = new Vector3( 175 * monsterList.Count, 0, 0 );
		go.GetComponent<MonsterButton>().monster = m;
		go.GetComponent<MonsterButton>().Show( m.isElite );

		monsterList.Add( m );
	}

	/// <summary>
	/// Add from interacting with a threat
	/// </summary>
	public void AddNewMonsterGroup( Monster[] array, IInteraction interaction )
	{
		foreach ( Monster m in array )
		{
			m.interaction = interaction;
			m.currentHealth = new int[3] { m.health, m.health, m.health };
			GameObject go = Instantiate( monsterButtonPrefab, transform );
			go.transform.localPosition = new Vector3( 175 * monsterList.Count, 0, 0 );
			go.GetComponent<MonsterButton>().monster = m;
			go.GetComponent<MonsterButton>().Show( m.isElite );

			monsterList.Add( m );
		}
	}

	//public void RemoveMonster( int i )
	//{
	//	if ( i >= monsterList.Count )
	//		return;
	//	RemoveMonster( monsterList[i] );
	//}

	/// <summary>
	/// removes monster group AND fires its Trigger After Event
	/// </summary>
	public void RemoveMonster( Monster m )
	{
		monsterList.Remove( m );

		//remove the prefab button from the screen and rearrange buttons
		int c = 0;
		foreach ( Transform child in transform )
		{
			if ( child.name.Contains( "MonsterButton" ) )
			{
				if ( child.GetComponent<MonsterButton>().monster == m )
					child.GetComponent<MonsterButton>().Remove();
				else
					child.GetComponent<MonsterButton>().Regroup( c++ * 175 );
			}
		}

		//fire After Event trigger for monster group
		if ( m.interaction != null )
			FindObjectOfType<TriggerManager>().FireTrigger( m.interaction.triggerAfterName );
	}

	public bool ShowCombatPanel( Monster m )
	{
		foreach ( Transform child in transform )
		{
			if ( child.name.Contains( "MonsterButton" ) )
			{
				child.GetComponent<MonsterButton>().ToggleSelect( false );
			}
		}
		//selectedMonster = m;
		return combatPanel.Show( m );
	}

	public void UnselectAll()
	{
		foreach ( Transform child in transform )
		{
			if ( child.name.Contains( "MonsterButton" ) )
			{
				child.GetComponent<MonsterButton>().ToggleSelect( false );
			}
		}
	}

	public void SelectMonster( Monster m, bool select )
	{
		foreach ( Transform child in transform )
		{
			if ( child.name.Contains( "MonsterButton" ) )
			{
				if ( child.GetComponent<MonsterButton>().monster.GUID == m.GUID )
					child.GetComponent<MonsterButton>().ToggleSelect( select );
			}
		}
	}

	public void ExhaustMonster( Monster m, bool exhaust )
	{
		foreach ( Transform child in transform )
		{
			if ( child.name.Contains( "MonsterButton" ) )
			{
				if ( child.GetComponent<MonsterButton>().monster.GUID == m.GUID )
				{
					child.GetComponent<MonsterButton>().ToggleExhausted( exhaust );
					return;
				}
			}
		}
		Debug.Log( "ExhaustMonster::no monster found with this GUID" );
	}

	public void ReadyAll()
	{
		foreach ( Transform child in transform )
		{
			if ( child.name.Contains( "MonsterButton" ) )
				child.GetComponent<MonsterButton>().ToggleExhausted( false );
		}
	}
}
