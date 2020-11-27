using System;
using UnityEngine;

public enum ScenarioType { Journey, Battle }
public enum InteractionType { Text, Threat, StatTest, Decision, Branch, Darkness, MultiEvent, Persistent, Conditional, Dialog, Replace }
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
public enum DifficultyBias { Light, Medium, Heavy }
public enum CampaignStatus { InMenus, PlayingScenario }
public enum ScenarioStatus { NotPlayed, Success, Failure }
public enum TitleScreen { Title, SelectSlot, SelectJourney, SelectHeroes }

public class InteractionResult
{
	public bool btn1, btn2, btn3, btn4, removeToken = true, success, canceled;
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
	int xpReward { get; set; }
	int threatReward { get; set; }
	PersonType personType { get; set; }
	bool isPersistent { get; set; }
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
	public string campaignGUID { get; set; }
	public string campaignStory { get; set; }
	public string campaignDescription { get; set; }
}

public class CampaignItem
{
	public string scenarioName { get; set; }
	/// <summary>
	/// file NAME only, NOT the full path
	/// </summary>
	public string fileName { get; set; }
}

public class StateItem
{
	public string gameName, scenarioFilename, gameDate, heroes, fullSavePath, fileVersion;
	public string[] heroArray;
	public Guid stateGUID;
	public ProjectType projectType;
	public CampaignState campaignState;
}

public class TitleMetaData
{
	public int slotMode;
	public ProjectItem projectItem;//set in SelectJourney
	public CampaignState campaignState;
	public string[] selectedHeroes;
	public int saveStateIndex;
	public string gameName;
	public Difficulty difficulty;
	public TitleScreen previousScreen;
	public bool skippedToCampaignScreen = false;
}

/// <summary>
/// Required data for starting any scenario
/// </summary>
public class GameStarter
{
	public string gameName;//name players assign to their save
	public int saveStateIndex = -1;
	/// <summary>
	/// file NAME only, NOT the full path
	/// </summary>
	public string scenarioFileName;
	public string[] heroes;
	/// <summary>
	/// setting this to false makes the scenario load state from the previously set saveStateIndex, default=true
	/// </summary>
	public bool isNewGame = true;
	//REQUIRED for NEW scenarios, otherwise restored from state
	public Difficulty difficulty = Difficulty.Normal;
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