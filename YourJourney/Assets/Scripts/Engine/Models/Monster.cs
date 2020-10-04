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

	//returns true if this monster can appear in current difficulty
	public bool IsValid()
	{
		if ( Bootstrap.difficulty == Difficulty.Easy && isEasy )
			return true;
		else if ( Bootstrap.difficulty == Difficulty.Normal && isNormal )
			return true;
		else if ( Bootstrap.difficulty == Difficulty.Hard && isHard )
			return true;

		return false;
	}

	public void AdjustPlayerCountDifficulty()
	{
		if ( Bootstrap.PlayerCount == 1 )
		{
			Debug.Log( "AdjustPlayerCountDifficulty::1" );
			//reduce enemy count
			count = Math.Max( 1, count - 1 );
			//attributes, -1 to shield/sorc with a minimum of 1 if value exists
			shieldValue = shieldValue > 0 ? Math.Max( 1, shieldValue - 1 ) : 0;
			sorceryValue = sorceryValue > 0 ? Math.Max( 1, sorceryValue - 1 ) : 0;
		}
		else if ( Bootstrap.PlayerCount > 2 )
		{
			Debug.Log( "AdjustPlayerCountDifficulty::>2" );

			int attAmount = Bootstrap.PlayerCount - 2;
			//health
			health++;
			//increase enemy count if it's NOT uniquely named
			if ( monsterNames.Any( x => dataName == x ) )
				count = Math.Min( 3, count + 1 );
			//attributes, +attAmount to shield/sorc
			shieldValue = shieldValue > 0 ? Math.Min( shieldValue + attAmount, health ) : 0;
			sorceryValue = sorceryValue > 0 ? Math.Min( sorceryValue + attAmount, health ) : 0;
		}
	}

	public void AdjustDifficulty()
	{
		if ( Bootstrap.difficulty == Difficulty.Easy )
		{
			Debug.Log( "AdjustDifficulty::Easy" );
			//health
			health = Math.Max( 1, health - 1 );
			//attributes
			shieldValue = sorceryValue = 0;
		}
		else if ( Bootstrap.difficulty == Difficulty.Hard )
		{
			Debug.Log( "AdjustDifficulty::Hard" );
			//health
			health++;
			//attributes
			if ( shieldValue == 0 && sorceryValue == 0 )
				shieldValue = 1;
			else
			{
				//make sure shield/sorc aren't more than health
				shieldValue = shieldValue > 0 ? shieldValue + 1 : 0;
				shieldValue = Math.Min( shieldValue, health );
				sorceryValue = sorceryValue > 0 ? sorceryValue + 1 : 0;
				sorceryValue = Math.Min( sorceryValue, health );
			}
			//elite bonus
			if ( isElite )
			{
				List<int> existing = new List<int>();
				if ( isLarge )
					existing.Add( 0 );
				if ( isBloodThirsty )
					existing.Add( 1 );
				if ( isArmored )
					existing.Add( 2 );

				int[] ebonuses = new int[3] { 0, 1, 2 };
				//filter out existing bonuses
				ebonuses = ebonuses.Where( x => !existing.Contains( x ) ).Select( x => x ).ToArray();
				if ( ebonuses.Length > 0 )
				{
					//assign a new, random elite bonus
					int rnd = UnityEngine.Random.Range( 0, ebonuses.Length );
					if ( ebonuses[rnd] == 0 )
						isLarge = true;
					else if ( ebonuses[rnd] == 1 )
						isBloodThirsty = true;
					else if ( ebonuses[rnd] == 2 )
						isArmored = true;
				}
			}
		}
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
}
