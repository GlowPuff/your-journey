using System;
using UnityEngine;
using System.Collections.Generic;

public enum ScenarioType { Journey, Battle }
public enum InteractionType { Text, Threat, StatTest, Decision, Branch, Darkness, MultiEvent, Persistent, Conditional, Dialog, Replace, Reward }
public enum MonsterType { Ruffian, GoblinScout, OrcHunter, OrcMarauder, HungryWarg, HillTroll, Wight, 
						Atarin, Gulgotar, Coalfang,
						GiantSpider, PitGoblin, OrcTaskmaster, Shadowman, NamelessThing, CaveTroll, Balrog, SpawnOfUngoliant,
						SupplicantOfMorgoth, Ursa, Ollie,
						FellBeast, WargRider, SiegeEngine, WarOliphaunt, Soldier, UrukWarrior,
						LordAngon, WitchKingOfAngmar, Eadris }
public enum CombatModifier { None, Pierce, Smite, Sunder, Cleave, Lethal, Stun }
public enum TileType { Hex, Battle, Square }
public enum ProjectType { Standalone, Campaign }
public enum Ability { Might, Agility, Wisdom, Spirit, Wit, Wild, Random, None }
public enum TerrainToken { None, Pit, Mist, Barrels, Table, FirePit, Statue }
public enum ButtonIcon { None, Action, OK, Continue, Next }
public enum TokenType { Search, Person, Threat, Darkness, DifficultGround, Fortified, Terrain, None }
public enum PersonType { Human, Elf, Hobbit, Dwarf, None }
public enum TerrainType {
	None, Barrels, Boulder, Bush, FirePit, Mist, Pit, Statue, Stream, Table, Wall, //Core Set
	Elevation, Log, Rubble, Web, //Shadowed Paths
	Barricade, Chest, Fence, Fountain, Pond, Trench //Spreading War
}

public enum Difficulty { Adventure, Normal, Hard }
public enum FinalStand { Damage, Fear }
public enum DifficultyBias { Light, Medium, Heavy }
public enum CampaignStatus { InMenus, PlayingScenario }
public enum ScenarioStatus { NotPlayed, Success, Failure }
public enum TitleScreen { Title, SelectSlot, SelectJourney, SelectHeroes }

public class AbilityUtility
{
	//This is used to return a bolded HTML string with optional size and color, to display an ability icon from a font.
	//The LoTR-JiME-Icons font has been renamed as Harrington in the harringtonBold font. The <b></b> tags switch to this icon font and display the icon.
	// Might, Agility, Wisdom, Spirit, Wit, Wild
	public static readonly string[] testColors = new string[] { "ff0000", "55cc00", "bb00bb", "0088ff", "ffff00", "ffffff" };
	public static readonly string[] testChars = new string[] { "M", "A", "Z", "S", "W", "X" }; //In the LoTR-JiME-Icons font

	public static string Text(Ability ability)
    {
		return "<b>" + testChars[(int)ability] + "</b>";
    }

	public static string Text(Ability ability, int size)
	{
		return "<size=" + size + "><b>" + testChars[(int)ability] + "</b></size>";
	}

	public static string ColoredText(Ability ability)
    {
		return "<color=#" + testColors[(int)ability] + ">" + Text(ability) + "</color>";
    }

	public static string ColoredText(Ability ability, int size)
	{
		return "<color=#" + testColors[(int)ability] + ">" + Text(ability, size) + "</color>";
	}
}


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
	TerrainType terrainType { get; set; }
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
	public List<int> collections { get; set; }
	public string campaignGUID { get; set; }
	public string campaignStory { get; set; }
	public string campaignDescription { get; set; }
	public string coverImage { get; set; }
}

public class CampaignItem
{
	public string scenarioName { get; set; }
	/// <summary>
	/// file NAME only, NOT the full path
	/// </summary>
	public string fileName { get; set; }
	//TODO collections in CampaignScreen?
	//public List<int> collections { get; set; }
}

public class StateItem
{
	public string gameName, scenarioFilename, gameDate, heroes, fullSavePath, fileVersion, coverImage;
	public string[] heroArray;
	public int[] heroIndexArray;
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
	public int[] selectedHeroesIndex;
	public int saveStateIndex;
	public string gameName;
	public string coverImage;
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
	public int[] heroesIndex;
	/// <summary>
	/// setting this to false makes the scenario load state from the previously set saveStateIndex, default=true
	/// </summary>
	public bool isNewGame = true;
	//REQUIRED for NEW scenarios, otherwise restored from state
	public Difficulty difficulty = Difficulty.Normal;
	public int loreStartValue;
	public int xpStartValue;
	public string coverImage;
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