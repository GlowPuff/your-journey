using System.Linq;
using UnityEngine;

/// <summary>
/// Bootstrap stays resident
/// Keep persistent game data here
/// Hero data, scenario, etc
/// </summary>
public class Bootstrap
{
	public static string AppVersion = "0.13";
	public static string FormatVersion = "1.6";

	public static string gameName;//name players assign to their save
	public static int saveStateIndex;
	public static string scenarioFileName { get; set; }
	public static bool isNewGame = true;
	public static string[] heroes;
	public static Difficulty difficulty = Difficulty.Normal;
	public static int[] lastStandCounter;
	public static bool[] isDead;
	public static int loreCount;
	public static int PlayerCount { get => heroes.Length; }
	public static string[] heroCustomNames;
	public static System.Random random = new System.Random();

	/// <summary>
	/// Resets vars and loads scenario using preset scenarioFileName and heroes
	/// </summary>
	public static Scenario LoadLevel()
	{
		ResetVars();
		Scenario scenario = FileManager.Load( FileManager.GetFullPath( scenarioFileName ) );
		if ( scenario != null )
		{
			Debug.Log( "LoadLevel()::Loaded: " + scenarioFileName );

			return scenario;
		}

		return null;
	}

	/// <summary>
	/// sets scenarioFileName and loads scenario
	/// </summary>
	public static Scenario LoadLevel( string filename )
	{
		try
		{
			scenarioFileName = filename;
			Scenario scenario = FileManager.Load( FileManager.GetFullPath( scenarioFileName ) );
			if ( scenario != null )
			{
				Debug.Log( "LoadLevel()::Loaded: " + scenarioFileName );
				return scenario;
			}
			else
				return null;
		}
		catch ( System.Exception e )
		{
			Debug.Log( "LoadLevel()::ERROR: " + scenarioFileName );
			Debug.Log( "LoadLevel()::ERROR: " + e.Message );
			return null;
		}
	}

	/// <summary>
	/// resets isDead, loreCount, lastStandCounter
	/// </summary>
	public static void ResetVars()
	{
		foreach ( string s in heroes )
			Debug.Log( "Hero:" + s );
		isDead = new bool[5];
		isDead.Fill( false );
		lastStandCounter = new int[5];
		lastStandCounter.Fill( 1 );
		loreCount = 0;
	}

	public static Scenario DEBUGLoadLevel()
	{
		heroes = new string[2] { "P1", "P2" };

		scenarioFileName = FileManager.GetProjects().First().fileName;
		Debug.Log( "DEBUGLoadLevel()::Loaded: " + scenarioFileName );
		Scenario scenario = FileManager.Load( FileManager.GetFullPath( scenarioFileName ) );

		ResetVars();

		//force debug vars
		gameName = "DEBUG game";
		difficulty = Difficulty.Normal;
		saveStateIndex = -1;
		//isNewGame = false;

		return scenario;
	}

	public static string GetRandomHero()
	{
		return heroes[Random.Range( 0, heroes.Length )];
	}

	public static void SaveHeroName( int index, string name )
	{
		PlayerPrefs.SetString( "Hero" + index, name );
	}

	public static string GetHeroName( int index )
	{
		return PlayerPrefs.GetString( "Hero" + index, "Hero" + index );
	}

	public static System.Tuple<int, int, int> LoadSettings()
	{
		int music = PlayerPrefs.GetInt( "music", 1 );
		int vignette = PlayerPrefs.GetInt( "vignette", 1 );
		int color = PlayerPrefs.GetInt( "color", 1 );

		return new System.Tuple<int, int, int>( music, vignette, color );
	}

	public static void SaveSettings( System.Tuple<int, int, int> prefs )
	{
		PlayerPrefs.SetInt( "music", prefs.Item1 );
		PlayerPrefs.SetInt( "vignette", prefs.Item2 );
		PlayerPrefs.SetInt( "color", prefs.Item3 );
	}
}
