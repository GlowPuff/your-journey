using System;
using System.Collections.Generic;
using System.IO;
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
	public CamState camState;

	public void SaveState( Engine engine )
	{
		partyState = PartyState.GetState( engine );
		triggerState = engine.triggerManager.GetState();
		objectiveState = engine.objectiveManager.GetState();
		monsterState = GlowEngine.FindObjectOfType<MonsterManager>().GetState();
		chapterState = engine.chapterManager.GetState();
		tileState = engine.tileManager.GetState();
		camState = GlowEngine.FindObjectOfType<CamControl>().GetState();

		string basePath = Path.Combine( Environment.ExpandEnvironmentVariables( "%userprofile%" ), "Documents", "Your Journey", "Saves" );

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
		string outpath = Path.Combine( basePath, partyState.fileName );
		Debug.Log( "SAVING TO: " + outpath );

		try
		{
			using ( var stream = File.CreateText( outpath ) )
			{
				stream.Write( output );
			}
		}
		catch ( Exception e )
		{
			Debug.Log( "Could not save the state" );
			Debug.Log( e.Message );
		}
	}

	public GameState LoadState( string filename )
	{
		string basePath = Path.Combine( Environment.ExpandEnvironmentVariables( "%userprofile%" ), "Documents", "Your Journey", "Saves" );
		string inpath = Path.Combine( basePath, filename );

		string json = "";
		using ( StreamReader sr = new StreamReader( inpath ) )
		{
			json = sr.ReadToEnd();
		}

		var fm = JsonConvert.DeserializeObject<GameState>( json );

		return fm;
	}
}

/*
 * Things to save:
 * x	party info - last stands, lore, items?
 * tile data - tile blocks(chapters), tile groups, tiles
 * x current objective
 * x current threat threshold
 * x active monsters and their state
 * x	triggers - firedTriggersList
 * 
 */

public class PartyState
{
	public string fileName { get; set; }
	public string scenarioFileName { get; set; }
	public int scenarioFileVersion { get; set; }
	public Difficulty difficulty { get; set; }
	public string[] heroes { get; set; }
	public int[] lastStandCounter { get; set; }
	public bool[] isDead { get; set; }
	public int loreCount { get; set; }
	public int threatThreshold { get; set; }

	public static PartyState GetState( Engine engine )
	{
		return new PartyState()
		{
			fileName = Bootstrap.fileName,
			scenarioFileName = Bootstrap.scenarioFileName,
			difficulty = Bootstrap.difficulty,
			heroes = Bootstrap.heroes,
			lastStandCounter = Bootstrap.lastStandCounter,
			isDead = Bootstrap.isDead,
			loreCount = Bootstrap.loreCount,
			threatThreshold = (int)engine.endTurnButton.currentThreat
		};
	}
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
	public int collectionCount;
}

public class ChapterState
{
	public List<string> tokenTriggerQueue = new List<string>();
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
	//[JsonConverter( typeof( Vector3Converter ) )]
	public Vector3 globalPosition;
	public bool isExplored;
	public Guid guid;
	public List<SingleTileState> tileStates;
}

public class SingleTileState
{
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
	public Vector3 globalPosition;
	public Guid parentTileGUID;
	public MetaData metaData;
}

public class CamState
{
	public Vector3 position;
	public float YRotation;//Euler y angle
}

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