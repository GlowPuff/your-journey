using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class GameState
{
	public PartyState partyState;
	public TriggerState triggerState;
	public ObjectiveState objectiveState;
	public MonsterState monsterState;
	public ChapterState chapterState;
	public TileState tileState;
	public InteractionState interactionState;
	public CamState camState;

	public void SaveState( Engine engine, int saveIndex )
	{
		if ( saveIndex == -1 )//not saving, bug out
		{
			Debug.Log( "SaveState::NOT saving" );
			return;
		}

		SaveState( engine, GetFullPath( "SAVE" + saveIndex + ".sav" ) );
	}

	public void SaveStateTemp( Engine engine )
	{
		SaveState( engine, GetFullPath( "TEMP.sav" ) );
	}

	public void SaveState( Engine engine, string fullPath )
	{
		//TODO return a bool for success
		partyState = PartyState.GetState( engine );
		triggerState = engine.triggerManager.GetState();
		objectiveState = engine.objectiveManager.GetState();
		monsterState = GlowEngine.FindObjectOfType<MonsterManager>().GetState();
		chapterState = engine.chapterManager.GetState();
		tileState = engine.tileManager.GetState();
		interactionState = engine.interactionManager.GetState();
		camState = GlowEngine.FindObjectOfType<CamControl>().GetState();

		//string basePath = Path.Combine( Environment.ExpandEnvironmentVariables( "%userprofile%" ), "Documents", "Your Journey", "Saves" );
		string mydocs = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
		string basePath = Path.Combine( mydocs, "Your Journey", "Saves" );

		if ( !Directory.Exists( basePath ) )
		{
			var di = Directory.CreateDirectory( basePath );
			if ( di == null )
			{
				Debug.Log( "Could not create the scenario project folder.\r\nTried to create: " + basePath );
				return;
			}
		}

		string output = JsonConvert.SerializeObject( this, Formatting.Indented, new Vector3Converter() );
		//string outpath = Path.Combine( basePath, "SAVE" + saveIndex + ".sav" );
		Debug.Log( "SaveState::SAVING TO: " + fullPath );

		try
		{
			using ( var stream = File.CreateText( fullPath ) )
			{
				stream.Write( output );
			}
		}
		catch
		{
			Debug.Log( "Could not save the state" );
		}
	}

	public static GameState LoadState( int saveIndex )
	{
		if ( saveIndex < 0 )
		{
			Debug.Log( "LoadState::saveIndex not valid" );
			return null;
		}
		return LoadState( GetFullPath( "SAVE" + saveIndex + ".sav" ) );
	}

	public static GameState LoadStateTemp( Scenario s )
	{
		Debug.Log( "Loading TEMP state" );
		return LoadState( GetFullPath( "TEMP.sav" ), s );
	}
	/// <summary>
	/// expects the FULL PATH+FILENAME
	/// </summary>
	public static GameState LoadState( string filename, Scenario s = null )
	{
		//string basePath = Path.Combine( Environment.ExpandEnvironmentVariables( "%userprofile%" ), "Documents", "Your Journey", "Saves" );
		//string inpath = Path.Combine( basePath, filename );

		try
		{
			string json = "";
			using ( StreamReader sr = new StreamReader( filename ) )
			{
				json = sr.ReadToEnd();
			}

			var fm = JsonConvert.DeserializeObject<GameState>( json );

			//s is not null only ywhen quickloading - make sure quickloading into same version of scenario as was quick saved
			if ( s != null && fm.partyState.scenarioGUID != s.scenarioGUID )
				return null;

			return fm;
		}
		catch ( Exception e )
		{
			Debug.Log( "CRITICAL ERROR: LoadState::" + filename );
			Debug.Log( e.Message );
			return null;
		}
	}

	public static IEnumerable<StateItem> GetSaveItems()
	{
		string mydocs = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
		string basePath = Path.Combine( mydocs, "Your Journey", "Saves" );
		if ( !Directory.Exists( basePath ) )
			Directory.CreateDirectory( basePath );

		List<StateItem> items = new List<StateItem>();
		DirectoryInfo di = new DirectoryInfo( basePath );
		FileInfo[] files = di.GetFiles();

		//exclude the temp save file
		files = ( from f in files
							where f.Name != "TEMP.sav" && f.Extension == ".sav"
							select f ).ToArray();
		//foreach ( FileInfo fi in files )
		for ( int i = 0; i < 6; i++ )
		{
			var fi = ( from f in files
								 where f.Name == "SAVE" + i + ".sav"
								 select f ).FirstOr( null );
			//Debug.Log( fi.FullName );
			//Debug.Log( fi.Name );
			if ( fi == null )
			{
				items.Add( null );
				continue;
			}
			GameState s = LoadState( fi.FullName );
			if ( s != null )
				items.Add( new StateItem()
				{
					gameName = s.partyState.gameName,
					gameDate = s.partyState.gameDate,
					scenarioGUID = s.partyState.scenarioGUID,
					scenarioFilename = s.partyState.scenarioFileName,
					fullSavePath = fi.FullName,
					heroes = s.partyState.heroes.Aggregate( ( acc, cur ) => acc + ", " + cur ),
					heroArray = s.partyState.heroes,
				} );
		}
		Debug.Log( "GetSaveItems::FOUND " + files.Count() );
		return items;
	}

	public static string GetFullPath( string filename )
	{
		string mydocs = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
		string basePath = Path.Combine( mydocs, "Your Journey", "Saves", filename );

		return basePath;
	}
}

///STATE OBJECTS

public class PartyState
{
	public string gameName { get; set; }
	public string gameDate { get; set; }
	public int saveStateIndex { get; set; }
	public string scenarioFileName { get; set; }
	public Guid scenarioGUID { get; set; }
	public Difficulty difficulty { get; set; }
	public string[] heroes { get; set; }
	public int[] lastStandCounter { get; set; }
	public bool[] isDead { get; set; }
	public int loreCount { get; set; }
	public int threatThreshold { get; set; }
	public Queue<Threat> threatStack { get; set; }
	public List<FogState> fogList { get; set; } = new List<FogState>();

	public static PartyState GetState( Engine engine )
	{
		return new PartyState()
		{
			gameName = Bootstrap.gameName,
			gameDate = DateTime.Today.ToShortDateString(),
			saveStateIndex = Bootstrap.saveStateIndex,
			scenarioFileName = Bootstrap.scenarioFileName,
			scenarioGUID = engine.scenario.scenarioGUID,
			difficulty = Bootstrap.difficulty,
			loreCount = Bootstrap.loreCount,
			threatThreshold = (int)engine.endTurnButton.currentThreat,
			threatStack = engine.endTurnButton.threatStack,
			heroes = Bootstrap.heroes,
			lastStandCounter = Bootstrap.lastStandCounter,
			isDead = Bootstrap.isDead,
			fogList = engine.GetFogState()
		};
	}

	public void SetState()
	{
		Bootstrap.gameName = gameName;
		Bootstrap.saveStateIndex = saveStateIndex;
		Bootstrap.scenarioFileName = scenarioFileName;
		Bootstrap.difficulty = difficulty;
		Bootstrap.heroes = heroes;
		Bootstrap.lastStandCounter = lastStandCounter;
		Bootstrap.isDead = isDead;
		Bootstrap.loreCount = loreCount;
	}
}

public class InteractionState
{
	public List<StatEventState> statEventStates = new List<StatEventState>();
	public List<DialogEventState> dialogEventStates = new List<DialogEventState>();
	public List<ReplaceEventState> replaceEventStates = new List<ReplaceEventState>();
	public List<TextEventState> textEventStates = new List<TextEventState>();
	public List<Guid> activeTokenGUIDs = new List<Guid>();
}

public class TextEventState
{
	public Guid eventGUID;
	public bool hasActivated;
}

public class StatEventState
{
	public Guid eventGUID;
	public int accumulatedValue;
}

public class DialogEventState
{
	public Guid eventGUID;
	public bool hasActivated;
	public bool c1Used, c2Used, c3Used;
	public bool isDone;
}

public class ReplaceEventState
{
	public Guid eventGUID;
	public bool hasActivated;
	public bool replaceWithHasActivated;
}

public class FogState
{
	public Vector3 globalPosition;
	public string chapterName;
}

public class TriggerState
{
	public Dictionary<string, bool> firedTriggersList = new Dictionary<string, bool>();
}

public class ObjectiveState
{
	public Guid currentObjective;
}

public class MonsterState
{
	public List<SingleMonsterState> monsterList = new List<SingleMonsterState>();
}

public class SingleMonsterState
{
	public Monster monster;
	public Guid eventGUID;
}

public class ChapterState
{
	public List<string> tokenTriggerQueue = new List<string>();
	public Guid previousGroupGUID;
}

public class TileState//tilemanager
{
	/// <summary>
	/// dynamic chapters that HAVE BEEN ACTIVATED
	/// </summary>
	public List<Guid> activatedDynamicChapters = new List<Guid>();
	public List<TileGroupState> tileGroupStates = new List<TileGroupState>();
}

public class TileGroupState
{
	public Vector3 globalPosition;
	public bool isExplored;
	public Guid guid;
	public List<SingleTileState> tileStates;
}

public class SingleTileState
{
	public bool isActive;
	public Guid tileGUID;
	public bool isExplored;
	public Vector3 globalPosition;
	public Vector3 globalParentPosition;
	public float globalParentYRotation;//Euler Y angle
	public List<string> tokenTriggerList = new List<string>();
	public List<TokenState> tokenStates = new List<TokenState>();
}

public class TokenState
{
	public bool isActive;
	public Vector3 localPosition;
	public Guid parentTileGUID;
	public MetaDataJSON metaData;
}

public class CamState
{
	public Vector3 position;
	public float YRotation;//Euler y angle
}

//[JsonConverter( typeof( Vector3Converter ) )]
//public struct V3
//{
//	public float x, y, z;

//	public V3( Vector3 v3 )
//	{
//		x = v3.x;
//		y = v3.y;
//		z = v3.y;
//	}
//}