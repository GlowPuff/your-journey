using System;
using System.Dynamic;
using UnityEngine;

/// <summary>
/// Models one GROUP of enemies (up to 3 enemies in a group)
/// </summary>
public class Monster
{
	public Guid GUID;
	public string dataName;
	public bool isEmpty;
	public string triggerName;

	public string bonuses;
	public int health;
	public int shieldValue;
	public int sorceryValue;
	public int damage;
	public int fear;
	public bool isLarge;
	public bool isBloodThirsty;
	public bool isArmored;
	public bool isElite;
	public Ability negatedBy { get; set; }
	public MonsterType monsterType { get; set; }
	public int count;
	public int movementValue;
	//public int maxMovementValue;
	public int loreReward;
	public bool defaultStats;
	public string specialAbility { get; set; }

	public IInteraction interaction;//the interaction that spawned this

	public static string[] monsterNames = { "Ruffian", "Goblin Scout", "Orc Hunter", "Orc Marauder", "Warg", "Hill Troll", "Wight" };

	public int[] currentHealth { get; set; } = new int[3];
	public bool isDead;
	public bool isExhausted;
	public bool isStunned;
	public int sunderValue;
	public int deathTally;
	public int deadCount;

	/// <summary>
	/// returns # of monsters that are alive
	/// </summary>
	public int ActiveMonsterCount
	{
		get
		{
			int c = 0;
			for ( int i = 0; i < count; i++ )
			{
				if ( currentHealth[i] > 0 )
					c++;
				//	c += currentHealth[i];
			}
			return c;
		}
	}

	public Monster()
	{

	}

	public Monster( string name )
	{
		dataName = name;
		damage = 1;
		fear = 1;
		health = 5;
		movementValue = 2;
		//maxMovementValue = 4;
		triggerName = "None";
		negatedBy = Ability.None;
		count = 1;
		isExhausted = isStunned = false;
	}

	//returns Tuple<fear,damage>
	public Tuple<int, int> CalculateDamage()
	{
		//calculate total and split it between damage and fear
		//modifier adds damage if enemies in group > 1
		//if it's a heavy hitter, limit the modifier to +1
		int modifier = ActiveMonsterCount == 1 ? 0 : ( damage == 4 ? 1 : ActiveMonsterCount - 1 );
		int total = damage + modifier + UnityEngine.Random.Range( -1, 2 );
		Debug.Log( "ActiveMonsterCount: " + ActiveMonsterCount );
		Debug.Log( "modified damage: " + modifier );
		Debug.Log( "total damage: " + total );
		int d = UnityEngine.Random.Range( 0, total + 1 );
		int f = total - d;
		if ( d == 0 && f == 0 )
			d = 1;
		if ( specialAbility != "Fear Bias" )
		{
			int temp = d;
			if ( f > d )
			{
				d = f;
				f = temp;
			}
		}
		else
		{
			int temp = f;
			if ( d > f )
			{
				f = d;
				d = temp;
			}
		}

		return new Tuple<int, int>( f, d );
	}
}
