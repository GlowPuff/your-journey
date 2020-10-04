using System.Linq;
using UnityEngine;

/// <summary>
/// Bootstrap stays resident
/// Keep persistent game data here
/// Hero data, scenario, etc
/// </summary>
public class Bootstrap
{
	public static string AppVersion = "0.9";
	public static string FormatVersion = "1.4";

	public static bool isNewGame = true;
	public static string[] heroes;
	public static Difficulty difficulty = Difficulty.Normal;
	public static int[] lastStandCounter;
	public static bool[] isDead;
	public static int loreCount;
	public static int PlayerCount { get => heroes.Length; }
	public static string[] heroCustomNames;

	static ProjectItem projectItem { get; set; }

	/// <summary>
	/// loads scenario using file info previously set with "SetNewGame" at the title screen
	/// </summary>
	public static Scenario LoadLevel()
	{
		Scenario scenario = FileManager.Load( FileManager.GetFullPath( projectItem.fileName ) );
		Debug.Log( "LoadLevel()::Loaded: " + projectItem.fileName );
		foreach ( string s in heroes )
			Debug.Log( "Hero:" + s );
		isDead = new bool[5];
		isDead.Fill( false );
		lastStandCounter = new int[5];
		lastStandCounter.Fill( 1 );

		return scenario;
	}

	public static Scenario DEBUGLoadLevel()
	{
		heroes = new string[2] { "Aragorn", "Gimli" };

		projectItem = FileManager.GetProjects().First();
		Debug.Log( "DEBUGLoadLevel()::Loaded: " + projectItem.fileName );
		Scenario scenario = FileManager.Load( FileManager.GetFullPath( projectItem.fileName ) );
		difficulty = Difficulty.Hard;
		isDead = new bool[5];
		isDead.Fill( false );
		lastStandCounter = new int[5];
		lastStandCounter.Fill( 1 );

		return scenario;
	}

	public static string GetRandomHero()
	{
		return heroes[Random.Range( 0, heroes.Length )];
	}

	public static void SetNewGame( string[] heroes, ProjectItem projectItem )
	{
		Bootstrap.heroes = heroes;
		Bootstrap.projectItem = projectItem;
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
