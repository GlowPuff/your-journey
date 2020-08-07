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

	/// <summary>
	/// IEnumerator, IEnumerable
	/// </summary>
	//public object Current
	//{
	//	get
	//	{
	//		try
	//		{
	//			return tileGroupList[itPosition];
	//		}

	//		catch ( IndexOutOfRangeException )
	//		{
	//			throw new InvalidOperationException();
	//		}
	//	}
	//}
	//int itPosition = -1;

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
	/// Creates a group and places all tiles in Chapter specified
	/// </summary>
	public TileGroup CreateGroupFromChapter( Chapter c, int[] tiles )
	{
		TileGroup tg = c.isRandomTiles ? TileGroup.CreateRandomGroup( c, tiles ) : TileGroup.CreateGroup( c );
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
		var foo = from tg in tileGroupList from t in tg.tileList.Where( x => x.HasTriggeredToken( name ) ) select new { };
		if ( foo.Count() == 0 )
			return false;

		TextPanel p = FindObjectOfType<InteractionManager>().GetNewTextPanel();
		p.ShowOkContinue( "Place the indicated Token", ButtonIcon.Continue, () =>
			{
				Vector3 tpos = ( -12345f ).ToVector3();
				foreach ( TileGroup tg in tileGroupList )
					foreach ( Tile t in tg.tileList )
					{
						tpos = t.RevealTriggeredToken( name );
						if ( tpos.x != -12345f )
						{
							Debug.Log( "MOVING" );
							FindObjectOfType<CamControl>().MoveTo( tpos );
							break;
						}
					}
			} );

		return true;
	}
}
