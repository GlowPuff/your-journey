using System.Linq;
using UnityEngine;

/// <summary>
/// Bootstrap stays resident
/// Keep persistent game data here
/// Hero data, scenario, etc
/// </summary>
public class Bootstrap
{
	public static string AppVersion = "0.4";
	public static string FormatVersion = "1.1";

	public static bool isNewGame = true;
	public static string[] heroes;

	static ProjectItem projectItem;

	public static Scenario LoadLevel()
	{
		Scenario scenario = FileManager.Load( FileManager.GetFullPath( projectItem.fileName ) );
		Debug.Log( "Loaded: " + scenario.fileName );
		foreach ( string s in heroes )
			Debug.Log( "Hero:" + s );
		return scenario;
	}

	public static Scenario DEBUGLoadLevel()
	{
		heroes = new string[2] { "Aragorn", "Gimli" };

		ProjectItem item = FileManager.GetProjects().First();
		Scenario scenario = FileManager.Load( FileManager.GetFullPath( item.fileName ) );
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
