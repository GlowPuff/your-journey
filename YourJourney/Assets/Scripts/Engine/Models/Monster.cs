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
	public int id;
	public string dataName;
	public bool isEmpty;
	public string triggerName;

	//public string bonuses;
	public int health;
	public int shieldValue;
	public int sorceryValue;
	public int moveA;
	public int moveB;
	public string[] moveSpecial;
	public bool isRanged;
	public int groupLimit;
	public int figureLimit;
	public int[] cost;
	public string[] tag;
	public int damage;
	public bool isFearsome;
	public string[] special;
	public bool isLarge;
	public bool isBloodThirsty;
	public bool isArmored;
	public bool isElite;
	public bool hasBanner = false;
	public Ability negatedBy { get; set; }
	public MonsterType monsterType { get; set; }
	public int count;
	public int movementValue;
	public int loreReward;
	public bool defaultStats;

	[DefaultValue( true )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public bool isEasy { get; set; } //adventure mode
	[DefaultValue( true )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public bool isNormal { get; set; }
	[DefaultValue( true )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public bool isHard { get; set; }
	[JsonIgnore]
	public ThreatInteraction interaction;//the interaction that spawned this

	public static string[] monsterNames = { "Ruffian", "Goblin Scout", "Orc Hunter", "Orc Marauder", "Hungry Warg", "Hill Troll", "Wight",
											"Atarin", "Gulgotar", "Coalfang",
											"Giant Spider", "Pit Goblin", "Orc Taskmaster", "Shadowman", "Nameless Thing", "Cave Troll", "Balrog", "Spawn of Ungoliant",
											"Supplicant of Morgoth", "Ursa", "Ollie",
											"Fell Beast", "Warg Rider", "Siege Engine", "War Oliphaunt", "Soldier", "Uruk Warrior",
											"Lord Angon", "Witch-king of Angmar", "Eadris"
	};

	public int[] currentHealth { get; set; } = new int[3];
	public int[] currentSunder { get; set; } = new int[3];
	public bool isDead;
	public bool isExhausted;
	public bool isStunned;
	//public int sunderValue;
	public int deathTally;
	public int deadCount;
	public float fCost;
	public float singlecost;

	//lines up with MonsterType enum
	public static int[] MonsterCost = new int[] { 7, 4, 10, 9, 14, 25, 17, //Core Set
		1000, 1000, 1000, //Villains of Eriador
		5, 4, 14, 17, 27, 20, 1000, 36, //Shadowed Paths
		1000, 1000, 1000, //Dwellers in Darkness
		24, 14, 22, 30, 8, 11, //Spreading War
		1000, 1000, 1000 //Scourges of the Wastes
	};

	public static int[] MonsterCount = new int[] { 6, 6, 3, 6, 3, 1, 3, //Core Set
		1, 1, 1, //Villains of Eriador
		6, 6, 3, 3, 3, 2, 1, 1, //Shadowed Paths
		1, 1, 1, //Dwellers in Darkness
		3, 3, 2, 1, 6, 6, //Spreading War
		1, 1, 1, //Scourges of the Wastes
	};
	//large, bloodthirsty, armored
	public static int[] ModCost = new int[] { 3, 6, 4, 4, 7, 3, 6, 6, 10, 8, 9, 6, 10 };
	public static string[] modNames = new string[13] { "Large", "Bloodthirsty", "Armored", "Huge", "Shrouded", "Terrifying", "Spike Armor", "Well-Equipped", "Veteran", "Wary", "Guarded", "Hardened", "Alert" };

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
		if ( Bootstrap.gameStarter.difficulty == Difficulty.Adventure && isEasy )
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
		if ( isFearsome )
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
		int mId, mHealth = 0, mArmor = 0, mSorcery = 0, mMoveA = 0, mMoveB = 0, mGroupLimit = 0, mFigureLimit = 0, mDamage = 0, mSpeed = 0;
		int[] mCost = new int[] { 1000, 0, 0 };
		bool mRanged = false, mFearsome = false;
		string mEnumName, mDataName = "";
		string[] mMoveSpecial, mTag, mSpecial = new string[0];

		switch (mType)
		{
			//Core Set
			case MonsterType.Ruffian:
				mId = 0;
				mEnumName = "Ruffian";
				mDataName = "Ruffian";
				mHealth = 5;
				mArmor = 0;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 7, 13, 19 };
				mTag = new string[] { "Humanoid", "Weak" };
				mSpeed = 2;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.GoblinScout:
				mId = 1;
				mEnumName = "GoblinScout";
				mDataName = "Goblin Scout";
				mHealth = 3;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 4, 7, 11 };
				mTag = new string[] { "Goblin", "Weak", "Small" };
				mSpeed = 2;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.OrcHunter:
				mId = 2;
				mEnumName = "OrcHunter";
				mDataName = "Orc Hunter";
				mHealth = 5;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = true;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 10, 19, 28 };
				mTag = new string[] { "Orc", "Ranged" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.OrcMarauder:
				mId = 3;
				mEnumName = "OrcMarauder";
				mDataName = "Orc Marauder";
				mHealth = 5;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 9, 17, 25 };
				mTag = new string[] { "Orc", "Slow" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.HungryWarg:
				mId = 4;
				mEnumName = "HungryWarg";
				mDataName = "Hungry Warg";
				mHealth = 8;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 14, 27, 40 };
				mTag = new string[] { "Beast", "Powerful", "Fast" };
				mSpeed = 3;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.HillTroll:
				mId = 5;
				mEnumName = "HillTroll";
				mDataName = "Hill Troll";
				mHealth = 14;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 25, 0, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large" };
				mSpeed = 2;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Wight:
				mId = 6;
				mEnumName = "Wight";
				mDataName = "Wight";
				mHealth = 6;
				mArmor = 0;
				mSorcery = 3;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 17, 32, 47 };
				mTag = new string[] { "Humanoid", "Powerful" };
				mSpeed = 1;
				mDamage = 4;
				mFearsome = true;
				mSpecial = new string[] { };
				break;

			//Villains of Eriador
			case MonsterType.Atarin:
				mId = 7;
				mEnumName = "Atarin";
				mDataName = "Atarin";
				mHealth = 8;
				mArmor = 1;
				mSorcery = 1;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 1000, 0, 0 };
				mTag = new string[] { "Humanoid", "Powerful", "Small" };
				mSpeed = 2;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Gulgotar:
				mId = 8;
				mEnumName = "Gulgotar";
				mDataName = "Gulgotar";
				mHealth = 8;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 3;
				mMoveSpecial = new string[] { "Encumbered" };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 1000, 0, 0 };
				mTag = new string[] { "Orc", "Powerful" };
				mSpeed = 1;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Coalfang:
				mId = 9;
				mEnumName = "Coalfang";
				mDataName = "Coalfang";
				mHealth = 8;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { "Predatory" };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 1000, 0, 0 };
				mTag = new string[] { "Beast", "Fast" };
				mSpeed = 3;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;

			//Shadowed Paths
			case MonsterType.GiantSpider:
				mId = 10;
				mEnumName = "GiantSpider";
				mDataName = "Giant Spider";
				mHealth = 4;
				mArmor = 0;
				mSorcery = 0;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 5, 8, 12 };
				mTag = new string[] { "Beast", "Weak", "Small", "Fast" };
				mSpeed = 3;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.PitGoblin:
				mId = 11;
				mEnumName = "PitGoblin";
				mDataName = "Pit Goblin";
				mHealth = 3;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = true;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 4, 7, 11 };
				mTag = new string[] { "Goblin", "Weak", "Small", "Ranged" };
				mSpeed = 2;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.OrcTaskmaster:
				mId = 12;
				mEnumName = "OrcTaskmaster";
				mDataName = "Orc Taskmaster";
				mHealth = 7;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 14, 27, 40 };
				mTag = new string[] { "Orc", "Slow" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Shadowman:
				mId = 13;
				mEnumName = "Shadowman";
				mDataName = "Shadowman";
				mHealth = 5;
				mArmor = 1;
				mSorcery = 2;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { "Incorporeal" };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 17, 32, 47 };
				mTag = new string[] { "Humanoid", "Powerful", "Slow" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.NamelessThing:
				mId = 14;
				mEnumName = "NamelessThing";
				mDataName = "Nameless Thing";
				mHealth = 20;
				mArmor = 0;
				mSorcery = 0;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { "Tunneler" };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 3;
				mCost = new int[] { 27, 0, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large", "Fast" };
				mSpeed = 3;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.CaveTroll:
				mId = 15;
				mEnumName = "CaveTroll";
				mDataName = "Cave Troll";
				mHealth = 10;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 2;
				mFigureLimit = 2;
				mCost = new int[] { 20, 40, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large", "Slow" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Balrog:
				mId = 16;
				mEnumName = "Balrog";
				mDataName = "Balrog";
				mHealth = 18;
				mArmor = 2;
				mSorcery = 2;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 1000, 0, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large", "Slow" };
				mSpeed = 1;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { "Cleave" };
				break;
			case MonsterType.SpawnOfUngoliant:
				mId = 17;
				mEnumName = "SpawnOfUngoliant";
				mDataName = "Spawn of Ungoliant";
				mHealth = 18;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 1000, 0, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large" };
				mSpeed = 2;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;

			//Dwellers in Darkness
			case MonsterType.SupplicantOfMorgoth:
				mId = 18;
				mEnumName = "SupplicantOfMorgoth";
				mDataName = "Supplicant of Morgoth";
				mHealth = 9;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 1000, 0, 0 };
				mTag = new string[] { "Orc", "Powerful", "Slow" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = true;
				mSpecial = new string[] { };
				break;
			case MonsterType.Ursa:
				mId = 19;
				mEnumName = "Ursa";
				mDataName = "Ursa";
				mHealth = 16;
				mArmor = 2;
				mSorcery = 4;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 0, 0, 0 };
				mTag = new string[] { "Humanoid", "Powerful", "Fast" };
				mSpeed = 3;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Ollie:
				mId = 20;
				mEnumName = "Ollie";
				mDataName = "Ollie";
				mHealth = 11;
				mArmor = 0;
				mSorcery = 1;
				mMoveA = 1;
				mMoveB = 2;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 1000, 0, 0 };
				mTag = new string[] { "Beast", "Large", "Slow" };
				mSpeed = 1;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;

			//Spreading War
			case MonsterType.FellBeast:
				mId = 21;
				mEnumName = "FellBeast";
				mDataName = "Fell Beast";
				mHealth = 8;
				mArmor = 0;
				mSorcery = 2;
				mMoveA = 2;
				mMoveB = 2;
				mMoveSpecial = new string[] { "Flying" };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 3;
				mCost = new int[] { 24, 0, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large", "Fast" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.WargRider:
				mId = 22;
				mEnumName = "WargRider";
				mDataName = "Warg Rider";
				mHealth = 10;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { };
				mRanged = true;
				mGroupLimit = 3;
				mFigureLimit = 3;
				mCost = new int[] { 14, 27, 40 };
				mTag = new string[] { "Goblin", "Beast", "Ranged" };
				mSpeed = 3;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.SiegeEngine:
				mId = 23;
				mEnumName = "SiegeEngine";
				mDataName = "Siege Engine";
				mHealth = 7;
				mArmor = 4;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 1;
				mMoveSpecial = new string[] { "Reinforcements" };
				mRanged = true;
				mGroupLimit = 1;
				mFigureLimit = 2;
				mCost = new int[] { 22, 0, 0 };
				mTag = new string[] { "Powerful", "Large", "Ranged", "Slow" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.WarOliphaunt:
				mId = 24;
				mEnumName = "WarOliphaunt";
				mDataName = "War Oliphaunt";
				mHealth = 10;
				mArmor = 4;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 3;
				mMoveSpecial = new string[] { "Stampede" };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 30, 0, 0 };
				mTag = new string[] { "Beast", "Powerful", "Large" };
				mSpeed = 2;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Soldier:
				mId = 25;
				mEnumName = "Soldier";
				mDataName = "Soldier";
				mHealth = 6;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 8, 14, 20 };
				mTag = new string[] { "Humanoid", "Weak" };
				mSpeed = 2;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.UrukWarrior:
				mId = 26;
				mEnumName = "UrukWarrior";
				mDataName = "Uruk Warrior";
				mHealth = 7;
				mArmor = 2;
				mSorcery = 0;
				mMoveA = 1;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 11, 19, 27 };
				mTag = new string[] { "Orc", "Fast" };
				mSpeed = 1;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;

			//Scourges of the Wastes
			case MonsterType.LordAngon:
				mId = 27;
				mEnumName = "LordAngon";
				mDataName = "Lord Angon";
				mHealth = 12;
				mArmor = 2;
				mSorcery = 2;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 1000, 0, 0 };
				mTag = new string[] { "Humanoid", "Poweful" };
				mSpeed = 2;
				mDamage = 3;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.WitchKingOfAngmar:
				mId = 28;
				mEnumName = "WitchKingOfAngmar";
				mDataName = "Witch-king of Angmar";
				mHealth = 20;
				mArmor = 4;
				mSorcery = 4;
				mMoveA = 3;
				mMoveB = 6;
				mMoveSpecial = new string[] { "Flying" };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 1000, 0, 0 };
				mTag = new string[] { "Humanoid", "Beast", "Powerful", "Fast", "Flying" };
				mSpeed = 3;
				mDamage = 4;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
			case MonsterType.Eadris:
				mId = 29;
				mEnumName = "Eadris";
				mDataName = "Eadris";
				mHealth = 10;
				mArmor = 1;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 1;
				mFigureLimit = 1;
				mCost = new int[] { 1000, 0, 0 };
				mTag = new string[] { "Humanoid", "Small" };
				mSpeed = 2;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;

			default:
				mId = 0;
				mHealth = 5;
				mArmor = 0;
				mSorcery = 0;
				mMoveA = 2;
				mMoveB = 4;
				mMoveSpecial = new string[] { };
				mRanged = false;
				mGroupLimit = 3;
				mFigureLimit = 6;
				mCost = new int[] { 7, 13, 19 };
				mTag = new string[] { "Humanoid", "Weak" };
				mSpeed = 2;
				mDamage = 2;
				mFearsome = false;
				mSpecial = new string[] { };
				break;
		}

		return new Monster()
		{
			//dataName = monsterNames[(int)mType],
			id = mId,
			dataName = mDataName,
			monsterType = mType,
			GUID = Guid.NewGuid(),
			health = mHealth,
			shieldValue = mArmor,
			sorceryValue = mSorcery,
			moveA = mMoveA,
			moveB = mMoveB,
			moveSpecial = mSpecial,
			isRanged = mRanged,
			groupLimit = mGroupLimit,
			figureLimit = mFigureLimit,
			cost = mCost,
			movementValue = mSpeed,
			damage = mDamage,
			special = mSpecial,
			isFearsome = mFearsome,
			triggerName = "None",
			singlecost = MonsterCost[(int)mType],
			isEasy = true,
			isNormal = true,
			isHard = true,
			negatedBy = Ability.Might
		};
	}
}
