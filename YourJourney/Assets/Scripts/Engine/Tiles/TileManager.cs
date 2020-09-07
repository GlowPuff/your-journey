using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileManager : MonoBehaviour//, IEnumerator, IEnumerable
{
	public GameObject[] tilePrefabs;
	public GameObject[] tilePrefabsB;
	public GameObject searchTokenPrefab, darkTokenPrefab, personTokenPrefab, threatTokenPrefab;

	bool disableInput = false;

	List<TileGroup> tileGroupList = new List<TileGroup>();

	public TileGroup this[int idx] => tileGroupList[idx];

	//take an id (101) and return its prefab
	public GameObject GetPrefab( string side, int id )
	{
		return side == "A" ? getATile( id ) : getBTile( id );
	}

	GameObject getATile( int id )
	{
		switch ( id )
		{
			case 100:
				return tilePrefabs[0];
			case 101:
				return tilePrefabs[1];
			case 200:
				return tilePrefabs[2];
			case 201:
				return tilePrefabs[3];
			case 202:
				return tilePrefabs[4];
			case 203:
				return tilePrefabs[5];
			case 204:
				return tilePrefabs[6];
			case 205:
				return tilePrefabs[7];
			case 206:
				return tilePrefabs[8];
			case 207:
				return tilePrefabs[9];
			case 208:
				return tilePrefabs[10];
			case 209:
				return tilePrefabs[11];
			case 300:
				return tilePrefabs[12];
			case 301:
				return tilePrefabs[13];
			case 302:
				return tilePrefabs[14];
			case 303:
				return tilePrefabs[15];
			case 304:
				return tilePrefabs[16];
			case 305:
				return tilePrefabs[17];
			case 306:
				return tilePrefabs[18];
			case 307:
				return tilePrefabs[19];
			case 308:
				return tilePrefabs[20];
			case 400:
				return tilePrefabs[21];
			default:
				return tilePrefabs[0];
		}
	}

	GameObject getBTile( int id )
	{
		switch ( id )
		{
			case 100:
				return tilePrefabsB[0];
			case 101:
				return tilePrefabsB[1];
			case 200:
				return tilePrefabsB[2];
			case 201:
				return tilePrefabsB[3];
			case 202:
				return tilePrefabsB[4];
			case 203:
				return tilePrefabsB[5];
			case 204:
				return tilePrefabsB[6];
			case 205:
				return tilePrefabsB[7];
			case 206:
				return tilePrefabsB[8];
			case 207:
				return tilePrefabsB[9];
			case 208:
				return tilePrefabsB[10];
			case 209:
				return tilePrefabsB[11];
			case 300:
				return tilePrefabsB[12];
			case 301:
				return tilePrefabsB[13];
			case 302:
				return tilePrefabsB[14];
			case 303:
				return tilePrefabsB[15];
			case 304:
				return tilePrefabsB[16];
			case 305:
				return tilePrefabsB[17];
			case 306:
				return tilePrefabsB[18];
			case 307:
				return tilePrefabsB[19];
			case 308:
				return tilePrefabsB[20];
			case 400:
				return tilePrefabsB[21];
			default:
				return tilePrefabsB[0];
		}
	}

	public TileGroup[] GetAllTileGroups()
	{
		return tileGroupList.ToArray();
	}

	/// <summary>
	/// return Transform[] of all visible token positions that are "open", not near a used Token
	/// </summary>
	public Vector3[] GetAvailableTokenPositions()
	{
		//get explored tiles
		var explored = from tg in tileGroupList from tile in tg.tileList where tile.isExplored select tile;
		List<Transform> tkattach = new List<Transform>();
		List<Transform> TKattach = new List<Transform>();
		foreach ( Tile t in explored )
		{
			//get all "token attach" positions
			foreach ( Transform child in t.transform )
				if ( child.name.Contains( "token attach" ) )
					tkattach.Add( child );
			//get all Tokens on the tile
			foreach ( Transform child in t.transform )
				if ( child.name.Contains( "Token(Clone)" ) )
					TKattach.Add( child );
		}

		//if there are tokens on the tile, we need to weed them out
		if ( TKattach.Count > 0 )
		{
			//select token positions that aren't near each other - these will be open for use
			//test shows ~1.1 units typical Token to tk attach distance
			var found = from tktf in tkattach from TKtf in TKattach where Vector3.Distance( tktf.position, TKtf.position ) > 1.5f select tktf;
			//results found - return new vector3[] where y = .3
			if ( found.Count() > 0 )
			{
				var open = found.Select( x => new Vector3( x.position.x, .3f, x.position.z ) );
				return open.ToArray();//return results
			}
			else
				return null;//no open attach positions open
		}
		else//no tokens on board to exclude, just return all attach positions
			return tkattach.Select( x => new Vector3( x.position.x, .3f, x.position.z ) ).ToArray();
	}

	/// <summary>
	/// Creates a group and places all tiles in Chapter specified
	/// </summary>
	public TileGroup CreateGroupFromChapter( Chapter c )
	{
		Debug.Log( "CreateGroupFromChapter: " + c.dataName );
		TileGroup tg = c.isRandomTiles ? TileGroup.CreateRandomGroup( c ) : TileGroup.CreateGroup( c );
		tileGroupList.Add( tg );
		return tg;
	}

	/// <summary>
	/// Toggles mouse/touch input with a delay
	/// </summary>
	public void ToggleInput( bool disabled )
	{
		//the delay avoids things being re-enabled on the same frame
		if ( disabled == true )
			disableInput = true;
		else
			GlowTimer.SetTimer( 1, () => { disableInput = false; } );
	}

	/// <summary>
	/// Creates a group and places all tiles in Chapter specified, then attaches it to specified group
	/// </summary>
	public void CreateGroupAndAttach( Chapter c, TileGroup tg )
	{

	}

	//public bool Collision()
	//{
	//	return tileGroupList[0].CollisionCheck();
	//}

	public void RemoveAllTiles()
	{
		foreach ( var tg in tileGroupList )
			tg.RemoveGroup();
		tileGroupList.Clear();
	}

	private void Update()
	{
		if ( FindObjectOfType<InteractionManager>().PanelShowing )
			return;

		if ( !disableInput && Input.GetMouseButtonDown( 0 ) )
		{
			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
			foreach ( TileGroup tg in tileGroupList )
				foreach ( Tile t in tg.tileList )
					if ( t.InputUpdate( ray ) )
						return;
		}
	}

	public int UnexploredTileCount()
	{
		int c = 0;
		foreach ( TileGroup tg in tileGroupList )
			foreach ( Tile t in tg.tileList )
				if ( !t.isExplored )
					c++;
		return c;
	}

	public int ThreatTokenCount()
	{
		int c = 0;
		foreach ( TileGroup tg in tileGroupList )
			foreach ( Tile t in tg.tileList )
			{
				Transform[] tf = t.GetChildren( "Token" );
				c += tf.Count( x => x.GetComponent<MetaData>().tokenType == TokenType.Threat );
			}
		return c;
	}

	public bool TryTriggerToken( string name )
	{
		//Debug.Log( "TryTriggerToken: " + name );
		//this method acts on ALL tiles on ALL chapters on the board

		//select tile(s) that have a token Triggered By 'name'
		var tiles = from tg in tileGroupList from t in tg.tileList.Where( x => x.HasTriggeredToken( name ) ) select t;

		//no tiles currently on the table have a token Triggered By this name, so enqueue it for later chapters to check when they get explored
		FindObjectOfType<ChapterManager>().EnqueueTokenTrigger( name );

		//there are tiles on the table with matching tokens, weed out explored tiles
		var explored = tiles.Where( x => x.isExplored );
		var unexplored = tiles.Where( x => !x.isExplored );

		//Debug.Log( "Found " + explored.Count() + " matching EXPLORED tiles" );
		//Debug.Log( "Found " + unexplored.Count() + " matching UNEXPLORED tiles" );

		//iterate the tiles and either reveal the token or queue it to show when its tile gets explored
		if ( explored.Count() > 0 )
		{

			TextPanel p = FindObjectOfType<InteractionManager>().GetNewTextPanel();
			p.ShowOkContinue( "Place the indicated Token", ButtonIcon.Continue, () =>
			{
				Vector3 tpos = ( -12345f ).ToVector3();
				//Debug.Log( "explored count=" + explored.Count() );
				foreach ( Tile t in explored )
				{
					//Debug.Log( "TILE ID:" + t.hexTile.idNumber );
					tpos = t.RevealTriggeredToken( name );
					if ( tpos.x != -12345f )
					{
						//Debug.Log( "MOVING" );
						FindObjectOfType<CamControl>().MoveTo( tpos );
					}
				}
			} );
		}

		//mark the rest to trigger later when the tiles get explored
		foreach ( Tile t in unexplored )
			t.EnqueueTokenTrigger( name );

		if ( tiles.Count() > 0 )
			return true;
		else
			return false;
	}
}
