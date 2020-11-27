using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
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

	//public string bonuses;
	public int health;
	public int shieldValue;
	public int sorceryValue;
	public int damage;
	public bool isLarge;
	public bool isBloodThirsty;
	public bool isArmored;
	public bool isElite;
	public Ability negatedBy { get; set; }
	public MonsterType monsterType { get; set; }
	public int count;
	public int movementValue;
	public int loreReward;
	public bool defaultStats;
	[DefaultValue( "" )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public string specialAbility { get; set; }

	[DefaultValue( true )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public bool isEasy { get; set; }
	[DefaultValue( true )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public bool isNormal { get; set; }
	[DefaultValue( true )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public bool isHard { get; set; }
	[JsonIgnore]
	public ThreatInteraction interaction;//the interaction that spawned this

	public static string[] monsterNames = { "Ruffian", "Goblin Scout", "Orc Hunter", "Orc Marauder", "Warg", "Hill Troll", "Wight" };

	public int[] currentHealth { get; set; } = new int[3];
	public bool isDead;
	public bool isExhausted;
	public bool isStunned;
	public int sunderValue;
	public int deathTally;
	public int deadCount;
	public float cost;
	public float singlecost;

	//lines up with MonsterType enum
	public static int[] MonsterCost = new int[7] { 3, 3, 6, 6, 7, 9, 7 };
	//large, bloodthirsty, armored
	public static int[] ModCost = new int[3] { 1, 2, 1 };
	public static string[] modNames = new string[3] { "Large", "Bloodthirsty", "Armored" };

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

	//returns true if this monster can appear in current difficulty
	public bool IsValid()
	{
		if ( Bootstrap.gameStarter.difficulty == Difficulty.Easy && isEasy )
			return true;
		else if ( Bootstrap.gameStarter.difficulty == Difficulty.Normal && isNormal )
			return true;
		else if ( Bootstrap.gameStarter.difficulty == Difficulty.Hard && isHard )
			return true;

		return false;
	}

	//returns Tuple<fear,damage>
	public Tuple<int, int> CalculateDamage()
	{
		//calculate total and split it between damage and fear
		//modifier adds damage if active enemies in group > 1
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
		if ( specialAbility != null && specialAbility != "Fear Bias" )
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

	public static Monster MonsterFactory( MonsterType mType )
	{
		//light=2, medium=3, heavy=4
		int mHealth = 0, mDamage = 0, mSpeed = 0, armor = 0, sorc = 0;
		string special = "";

		switch ( mType )
		{
			case MonsterType.GoblinScout:
				mHealth = 3;
				mDamage = 2;
				mSpeed = 2;
				armor = 1;
				break;
			case MonsterType.Ruffian:
				mHealth = 5;
				mDamage = 2;
				mSpeed = 2;
				armor = 0;
				break;
			case MonsterType.OrcHunter:
				mHealth = 5;
				mDamage = 3;
				mSpeed = 1;
				armor = 1;
				special = "Ranged";
				break;
			case MonsterType.OrcMarauder:
				mHealth = 5;
				mDamage = 3;
				mSpeed = 1;
				armor = 2;
				break;
			case MonsterType.Warg:
				mHealth = 8;
				mDamage = 3;
				mSpeed = 3;
				armor = 1;
				break;
			case MonsterType.HillTroll:
				mHealth = 14;
				mDamage = 4;
				mSpeed = 1;
				armor = 2;
				break;
			case MonsterType.Wight:
				mHealth = 6;
				mDamage = 4;
				mSpeed = 1;
				sorc = 3;
				special = "Fear Bias";
				break;
		}

		return new Monster()
		{
			dataName = monsterNames[(int)mType],
			monsterType = mType,
			GUID = Guid.NewGuid(),
			health = mHealth,
			damage = mDamage,
			movementValue = mSpeed,
			shieldValue = armor,
			sorceryValue = sorc,
			specialAbility = special,
			triggerName = "None",
			singlecost = MonsterCost[(int)mType],
			isEasy = true,
			isNormal = true,
			isHard = true,
			negatedBy = Ability.Might
		};
	}
}
