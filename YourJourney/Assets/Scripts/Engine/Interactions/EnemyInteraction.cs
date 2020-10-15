using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class ThreatInteraction : IInteraction
{
	public Guid GUID { get; set; }
	public string dataName { get; set; }
	public bool isEmpty { get; set; }
	public string triggerName { get; set; }
	public string triggerAfterName { get; set; }
	public TextBookData textBookData { get; set; }
	public TextBookData eventBookData { get; set; }
	public bool isTokenInteraction { get; set; }
	public TokenType tokenType { get; set; }
	public PersonType personType { get; set; }
	public int loreReward { get; set; }
	public string triggerDefeatedName { get; set; }
	public bool[] includedEnemies { get; set; }
	[DefaultValue( 0 )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public int basePoolPoints { get; set; }
	[DefaultValue( DifficultyBias.Medium )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public DifficultyBias difficultyBias { get; set; }

	public List<Monster> monsterCollection;

	public InteractionType interactionType { get { return InteractionType.Threat; } set { } }

	int lastEnemyIndex;
	int numSingleGroups;
	int modPoints;

	/// <summary>
	/// Generates enemy groups and adds them to monsterCollection using Pool System
	/// </summary>
	public void GenerateEncounter()
	{
		/*
				Large = +2 health, cost=1
				Bloodthirsty = 1 damage step up+wound bias, cost=2
				Armored = +1 armor, cost=1
		*/
		numSingleGroups = 0;
		lastEnemyIndex = -1;//avoid repeat enemy types if possible
		modPoints = 0;

		int poolCount = CalculateScaledPoints();
		int starting = poolCount;

		//if no enemies checked, returns 1000
		int lowestCost = LowestRequestedEnemyCost();
		if ( lowestCost == 1000 )
		{
			Debug.Log( "There are no Enemies included in the Pool." );
			return;
		}

		if ( poolCount < lowestCost )
		{
			Debug.Log( "There aren't enough Pool Points to generate any Enemies given the current parameters." );
			return;
		}

		//generate all the random monster groups possible
		List<Monster> mList = new List<Monster>();
		while ( poolCount >= lowestCost )//lowest enemy cost
		{
			Tuple<Monster, int> generated = GenerateMonster( poolCount );
			if ( generated.Item1.dataName != "modifier" )
			{
				poolCount = Math.Max( 0, poolCount - generated.Item2 );
				mList.Add( generated.Item1 );
			}
			else
			{
				//use dummy point
				poolCount = Math.Max( 0, poolCount - 1 );
			}
		}

		//recalculate points left over
		poolCount = starting;
		foreach ( Monster sim in mList )
			poolCount -= sim.cost;

		//if enough points left for more enemies, add them
		if ( poolCount > lowestCost )
		{
			foreach ( Monster sim in mList )
			{
				//% chance to add another
				if ( Bootstrap.random.Next( 100 ) < 50 && sim.count < 2 && sim.singlecost <= poolCount )
				{
					poolCount = Math.Max( 0, poolCount - sim.singlecost );
					sim.count++;
					sim.cost += sim.singlecost;
				}
			}
		}

		//add modifiers with any leftover points
		if ( mList.Count > 0 && poolCount > 0 )
		{
			foreach ( Monster sim in mList )
			{
				int mod = Bootstrap.random.Next( 4 );//3=none
				if ( mod != 3 && Monster.ModCost[mod] <= poolCount )
				{
					poolCount -= Monster.ModCost[mod];
					//sim.modList.Add( Monster.modNames[mod] );
					if ( mod == 0 )
						sim.isLarge = true;
					else if ( mod == 1 )
						sim.isBloodThirsty = true;
					else if ( mod == 2 )
						sim.isArmored = true;
					sim.isElite = true;
					modPoints += Monster.ModCost[mod];
					Debug.Log( "mod added: " + Monster.modNames[mod] );
				}
			}
		}

		Debug.Log( "leftover points: " + poolCount );

		//finally add finished groups to collection
		foreach ( Monster ms in mList )
			monsterCollection.Add( ms );
	}

	Tuple<Monster, int> GenerateMonster( int points )
	{
		//monster type/cost
		List<Tuple<MonsterType, int>> mList = new List<Tuple<MonsterType, int>>();
		//create list of enemy candidates
		for ( int i = 0; i < includedEnemies.Length; i++ )
		{
			//skip using enemy if it was already used last iteration
			if ( includedEnemies.Count( x => x ) > 1 && lastEnemyIndex == i )
				continue;

			//includedEnemies lines up with MonsterType enum and MonsterCost array
			if ( includedEnemies[i] && points >= Monster.MonsterCost[i] )
			{
				mList.Add( new Tuple<MonsterType, int>( (MonsterType)i, Monster.MonsterCost[i] ) );
			}
		}

		//how many
		if ( mList.Count > 0 )//sanity check
		{
			//pick 1 at random
			int pick = Bootstrap.random.Next( 0, mList.Count );
			//Debug.Log( pick );

			Monster ms = Monster.MonsterFactory( mList[pick].Item1 );
			int upTo = points / mList[pick].Item2;
			upTo = Math.Min( upTo, 3 );//max of 3 in group
			int count = Bootstrap.random.Next( 1, upTo + 1 );
			//avoid a bunch of 1 enemy groups
			if ( count == 1 && numSingleGroups >= 1 )
			{
				if ( count + 1 <= upTo )//if room to add 1 more...
				{
					//50% chance add another or use the points for modifiers
					if ( Bootstrap.random.Next( 100 ) > 50 || modPoints > 3 )
						count += 1;
					else
					{
						Monster skip = new Monster() { dataName = "modifier", cost = 1 };
						return new Tuple<Monster, int>( skip, 0 );
					}
				}
				else//no more room, 30% to add a modifier point instead
				{
					Monster skip = new Monster() { dataName = "modifier", cost = 0 };
					return new Tuple<Monster, int>( skip, Bootstrap.random.Next( 100 ) > 30 ? 1 : 0 );
				}
			}

			lastEnemyIndex = (int)mList[pick].Item1;
			ms.count = count;
			ms.singlecost = mList[pick].Item2;
			ms.cost = mList[pick].Item2 * count;
			if ( count == 1 )
				numSingleGroups++;
			return new Tuple<Monster, int>( ms, mList[pick].Item2 * count );
		}
		else
		{
			Monster skip = new Monster() { dataName = "modifier", cost = 1 };
			return new Tuple<Monster, int>( skip, 1 );
		}
	}

	private int CalculateScaledPoints()
	{
		float difficultyScale = 0;
		int bias = 0;

		//set the base pool
		int poolCount = basePoolPoints;

		//set the difficulty bias
		if ( difficultyBias == DifficultyBias.Light )
			bias = 3;
		else if ( difficultyBias == DifficultyBias.Medium )
			bias = 5;
		else if ( difficultyBias == DifficultyBias.Heavy )
			bias = 7;

		//set the difficulty scale
		if ( Bootstrap.difficulty == Difficulty.Easy )//easy
			difficultyScale = -.25f;
		else if ( Bootstrap.difficulty == Difficulty.Hard )//hard
			difficultyScale = .5f;

		//modify pool based on hero count above 1 and bias
		poolCount += ( Bootstrap.PlayerCount - 1 ) * bias;

		//modify pool based on difficulty scale
		poolCount += (int)Math.Round( (float)poolCount * difficultyScale );
		Debug.Log( "difficultyScale: " + difficultyScale );
		Debug.Log( "Scaled Pool Points: " + poolCount );
		return poolCount;
	}

	private int LowestRequestedEnemyCost()
	{
		int cost = 1000;

		if ( includedEnemies == null )//backwards compatible
			return 1000;

		for ( int i = 0; i < includedEnemies.Length; i++ )
		{
			if ( includedEnemies[i] )
			{
				if ( Monster.MonsterCost[i] < cost )
				{
					cost = Monster.MonsterCost[i];
				}
			}
		}

		return cost;
	}
}