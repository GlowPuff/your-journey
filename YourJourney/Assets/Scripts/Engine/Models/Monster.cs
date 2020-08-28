using System;
using System.Linq;

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
	public int maxMovementValue;
	public int loreReward;
	public bool defaultStats;
	public string specialAbility { get; set; }

	public IInteraction interaction;//the interaction that spawned this

	public static string[] monsterNames = { "Ruffian", "Goblin Scout", "Orc Hunter", "Orc Marauder", "Warg", "Hill Troll", "Wight" };

	public int[] currentHealth = new int[3];
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
				c += currentHealth[i];
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
		maxMovementValue = 4;
		triggerName = "None";
		negatedBy = Ability.None;
		count = 1;
		isExhausted = isStunned = false;
	}
}
