using System;
using UnityEngine;

public enum ScenarioType { Journey, Battle }
public enum InteractionType { Text, Threat, StatTest, Decision, Branch, Darkness, MultiEvent, Persistent, Conditional }
public enum MonsterType { Ruffian, GoblinScout, OrcHunter, OrcMarauder, Warg, HillTroll, Wight }
public enum CombatModifier { None, Pierce, Smite, Sunder, Cleave, Lethal, Stun }
public enum TileType { Hex, Battle }
public enum ProjectType { Standalone, Campaign }
public enum Ability { Might, Agility, Wisdom, Spirit, Wit, None }
public enum TerrainToken { None, Pit, Mist, Barrels, Table, FirePit, Statue }
public enum ButtonIcon { None, Action, OK, Continue, Next }
public enum TokenType { Search, Person, Threat, Darkness, Exploration, None }
public enum PersonType { Human, Elf, Hobbit, Dwarf }
public enum Difficulty { Easy, Normal, Hard }
public enum FinalStand { Damage, Fear }

public class InteractionResult
{
	public bool btn1, btn2, btn3, btn4, removeToken, success, canceled;
	public int value;
	public IInteraction interaction;

	public InteractionResult() { }
}

public class CombatModify
{
	public bool Pierce, Smite, Sunder, Cleave, Lethal, Stun;
}

public interface ITile
{
	TileType tileType { get; set; }
}

public interface IInteraction
{
	string dataName { get; set; }
	Guid GUID { get; set; }
	InteractionType interactionType { get; set; }
	bool isTokenInteraction { get; set; }
	string triggerName { get; set; }
	string triggerAfterName { get; set; }
	TextBookData textBookData { get; set; }
	TextBookData eventBookData { get; set; }
	TokenType tokenType { get; set; }
	int loreReward { get; set; }
	PersonType personType { get; set; }
}

public interface ICommonData
{
	Guid GUID { get; set; }
	string dataName { get; set; }
	bool isEmpty { get; set; }
	string triggerName { get; set; }
}

public class ProjectItem
{
	public string Title { get; set; }
	public string Date { get; set; }
	public string Description { get; set; }
	public ProjectType projectType { get; set; }
	public string fileName { get; set; }
	public string fileVersion { get; set; }
}

public struct Vector
{
	public float x, y;

	public Vector( float x, float y )
	{
		this.x = x;
		this.y = y;
	}
}

public static class Extensions
{
	public static Vector3 ToVector3( this Vector v )
	{
		return new Vector3( v.x, 0, v
			.y );
	}
}