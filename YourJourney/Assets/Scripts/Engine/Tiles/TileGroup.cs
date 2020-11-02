using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class TileGroup
{
	public TileManager tileManager;
	public Vector3 groupCenter;
	//0th Tile is the ROOT
	public List<Tile> tileList;
	public bool isExplored { get; set; }
	public Vector3 startPosition { get; set; }

	//each Tile is a child of containerObject
	public Transform containerObject;
	//List<Vector3> childOffsets;//normalized vectors pointint to child
	Chapter chapter { get; set; }
	//int[] randomTileIndices;//randomly index into chapter tileObserver
	[HideInInspector]
	public System.Guid GUID { get; set; }//same as chapter GUID

	private static float RandomAngle()
	{
		int[] a = { 0, 60, 120, 180, 240, -60, -120, -180, -240 };
		return a[Random.Range( 0, a.Length )];
	}

	TileGroup( System.Guid guid )
	{
		GUID = guid;//System.Guid.NewGuid();
	}

	public Chapter GetChapter()
	{
		return chapter;
	}

	public static TileGroup CreateFixedGroup( Chapter c )
	{
		TileGroup tg = new TileGroup( c.GUID );
		tg.startPosition = ( -1000f ).ToVector3();
		tg.isExplored = false;

		tg.BuildFixedFromChapter( c );
		return tg;
	}

	public static TileGroup CreateRandomGroup( Chapter c )
	{
		TileGroup tg = new TileGroup( c.GUID );
		tg.startPosition = ( -1000f ).ToVector3();
		tg.isExplored = false;

		tg.BuildRandomFromChapter( c );
		return tg;
	}

	//Build random group from editor Chapter
	void BuildRandomFromChapter( Chapter c )
	{
		//Debug.Log( "BuildRandomFromChapter" );
		List<Transform> usedPositions = new List<Transform>();
		tileManager = Object.FindObjectOfType<TileManager>();
		chapter = c;
		tileList = new List<Tile>();
		//randomTileIndices = GlowEngine.GenerateRandomNumbers( chapter.randomTilePool.Count );

		//Debug.Log( "(RANDOM)FOUND " + chapter.randomTilePool.Count + " TILES" );
		//Debug.Log( "RANDOM ROOT INDEX: " + randomTileIndices[0] );

		//create the parent container
		containerObject = new GameObject().transform;
		containerObject.name = "TILEGROUP: ";

		Tile previous = null;
		for ( int i = 0; i < c.tileObserver.Count; i++ )
		{
			//HexTile hexroot = new HexTile( chapter.randomTilePool[randomTileIndices[i]], new Vector(), RandomAngle() );
			//HexTile hexroot = new HexTile( randomTiles[i], new Vector(), RandomAngle() );

			HexTile hexroot = (HexTile)c.tileObserver[i];
			hexroot.vposition = new Vector3();
			hexroot.angle = RandomAngle();

			//provide OnExplore trigger for random tile?
			//HexTile hexroot = new HexTile();
			//create parent object for prefab tile
			GameObject go = new GameObject();
			go.name = hexroot.idNumber.ToString();
			//instantiate the tile prefab
			string side = hexroot.tileSide == "Random" ? ( Random.Range( 1, 101 ) < 50 ? "A" : "B" ) : hexroot.tileSide;
			Tile tile = Object.Instantiate( tileManager.GetPrefab( side, hexroot.idNumber ), go.transform ).GetComponent<Tile>();
			//Tile tile = tileManager.GetPrefab( side, hexroot.idNumber );
			//set its data
			//tile.Init();
			tile.gameObject.SetActive( false );
			//tile.transform.parent = go.transform;
			//tile.transform.localPosition = Vector3.zero;
			tile.hexTile = hexroot;
			tile.tileGroup = this;
			tile.chapter = c;
			//rotate go object
			tile.transform.parent.localRotation = Quaternion.Euler( 0, hexroot.angle, 0 );
			//set go's parent
			tile.transform.parent.transform.parent = containerObject;
			containerObject.name += " " + hexroot.idNumber.ToString();
			if ( previous != null )
			{
				tile.AttachTo( previous, this );
			}
			tileList.Add( tile );
			previous = tile;

			//add fixed tokens
			if ( tile.hexTile.tokenList.Count > 0 )
				usedPositions.AddRange( AddFixedToken( tile ) );

			if ( hexroot.isStartTile )
			{
				startPosition = tile.GetChildren( "token attach" )[0].position.Y( .26f );
				tile.isExplored = true;
			}
		}

		//add random tokens
		if ( c.usesRandomGroups )
			AddRandomTokens( usedPositions.ToArray() );

		//***start tile cannot be random, below is no longer needed
		//find starting position if applicable
		//if ( c.dataName == "Start" )
		//{
		//	bool found = false;
		//	foreach ( var randomTile in tileList )
		//	{
		//		var positions = randomTile.GetChildren( "token attach" );
		//		var tokens = randomTile.GetChildren( " Token(Clone)" );

		//		var open = from p in positions
		//							 from t in tokens
		//							 let foo = new { p, t }
		//							 where Vector3.Distance( foo.p.position.Y( 0 ), foo.t.position.Y( 0 ) ) > 1f
		//							 select foo;

		//		if ( open.Count() > 0 )
		//		{
		//			Debug.Log( "placed starting position" );
		//			startPosition = open.First().p.position.Y( .26f );
		//			found = true;
		//			randomTile.hexTile.isStartTile = true;
		//			Colorize( true );
		//			break;
		//		}
		//	}
		//	if ( !found )
		//	{
		//		Debug.Log( "Starting position not found, using default" );
		//		int tid = GlowEngine.GenerateRandomNumbers( tileList.Count )[0];
		//		startPosition = tileList[tid].GetExploretokenPosition();
		//		tileList[tid].hexTile.isStartTile = true;
		//		Colorize( true );
		//	}
		//}

		GenerateGroupCenter();
	}

	//Build fixed group from editor Chapter
	void BuildFixedFromChapter( Chapter c )
	{
		//Debug.Log( "BuildFixedFromChapter" );
		List<Transform> usedPositions = new List<Transform>();
		tileManager = Object.FindObjectOfType<TileManager>();
		chapter = c;
		tileList = new List<Tile>();

		//Debug.Log( "(FIXED)FOUND " + chapter.tileObserver.Count + " TILES" );

		//create the parent container
		containerObject = new GameObject().transform;
		containerObject.name = "TILEGROUP: ";

		for ( int i = 0; i < c.tileObserver.Count; i++ )
		{
			//Debug.Log( chapter.tileObserver[i].idNumber );

			HexTile h = chapter.tileObserver[i] as HexTile;
			containerObject.name += " " + h.idNumber.ToString();
			GameObject goc = new GameObject();
			goc.name = h.idNumber.ToString();

			Tile tile = Object.Instantiate( tileManager.GetPrefab( h.tileSide, h.idNumber ), goc.transform ).GetComponent<Tile>();
			//Tile tile = tileManager.GetPrefab( h.tileSide, h.idNumber );
			//set its data
			//tile.Init();
			tile.gameObject.SetActive( false );
			//tile.transform.parent = goc.transform;
			//tile.transform.localPosition = Vector3.zero;
			tile.chapter = c;
			tile.hexTile = h;
			tile.tileGroup = this;
			if ( i > 0 )
			{
				//3D distance between tiles in X = 0.75
				//3D distance between tiles in Y = 0.4330127

				//EDITOR distance between hextile centers = 55.425626
				//3D distance between hextile centers = .8660254
				float d = Vector3.Distance( tile.hexTile.vposition, tileList[0].hexTile.vposition );
				//Debug.Log( "DIST:" + d );//x48, y27.71281
				float scalar = .8660254f * d;
				scalar = scalar / 55.425626f;
				//Debug.Log( "SCALAR:" + scalar );//x48, y27.71281

				//get normalized EDITOR vector to first tile in this group
				Vector3 offset = tile.hexTile.vposition - tileList[0].hexTile.vposition;
				//convert normalized EDITOR vector to 3D using distance tween hexes
				Vector3 n = Vector3.Normalize( offset ) * scalar;
				//Debug.Log( "offset::" + goc.name + "::" + n );

				//reflect to account for difference in coordinate systems quadrant (2D to 3D)
				n = Vector3.Reflect( n, new Vector3( 0, 0, 1 ) );

				//fix tile positions that don't have editor root hex at 0,1
				Vector3 tilefix = Vector3.zero;
				//convert the string to vector2
				string[] s = tile.hexTile.hexRoot.Split( ',' );
				Vector2 p = new Vector2( float.Parse( s[0] ), float.Parse( s[1] ) );
				if ( p.y != 1 )
					tilefix = new Vector3( 0, 0, -.4330127f * ( p.y - 1f ) );
				if ( p.x != 0 )
					tilefix = new Vector3( p.x * .75f, 0, 0 );

				//set tile position using goc's position + reflected offset
				tile.SetPosition( tileList[0].transform.parent.transform.position + n + tilefix, h.angle );
				//Debug.Log( "ROOTPOS:" + tile.rootPosition.transform.position );
				//Debug.Log( "ROOT::" + tileList[0].transform.parent.transform.position );
			}
			else
			{
				//fix tile positions that don't have editor root hex at 0,1
				Vector3 tilefix = Vector3.zero;
				//convert the string to vector2
				string[] s = tile.hexTile.hexRoot.Split( ',' );
				Vector2 p = new Vector2( float.Parse( s[0] ), float.Parse( s[1] ) );
				if ( p.y != 1 )
					tilefix = new Vector3( 0, 0, -.4330127f * ( p.y - 1f ) );
				if ( p.x != 0 )
					tilefix = new Vector3( p.x * .75f, 0, 0 );
				tile.SetPosition( Vector3.zero, h.angle );
				tile.transform.position += tilefix;
			}

			tileList.Add( tile );
			//set parent of goc 
			tile.transform.parent.transform.parent = containerObject;
			//add a token, if there is one
			//if ( !c.usesRandomGroups )
			if ( tile.hexTile.tokenList.Count > 0 )
				usedPositions.AddRange( AddFixedToken( tile ) );

			//find starting position if applicable
			if ( h.isStartTile )
			{
				tile.isExplored = true;
				startPosition = tile.GetChildren( "token attach" )[0].position.Y( .26f );
			}
		}

		//add random tokens
		if ( c.usesRandomGroups && usedPositions.Count > 0 )
			AddRandomTokens( usedPositions.ToArray() );

		GenerateGroupCenter();
	}

	void AddRandomTokens( Transform[] usedPositions )
	{
		if ( chapter.randomInteractionGroup == "None" )
			return;
		//usedPositions = wonky user placed token position
		InteractionManager im = GlowEngine.FindObjectOfType<InteractionManager>();
		//get array of interactions that are in the interaction group
		IInteraction[] interactionGroupArray = im.randomTokenInteractions
			.Where( x => x.dataName.EndsWith( chapter.randomInteractionGroup ) ).ToArray();
		//Debug.Log( "EVENTS IN GROUP [" + chapter.randomInteractionGroup + "]: " + interactionGroupArray.Length );

		//get all the possible token spawn locations that are NOT near FIXED tokens already placed
		List<Transform> attachtfs = new List<Transform>();
		List<Transform> finalOpenTFS = new List<Transform>();
		foreach ( Tile t in tileList )
		{
			attachtfs.Clear();
			attachtfs.AddRange( t.GetChildren( "token attach" ) );
			var usedInThisTile = from tu in usedPositions
													 where tu.GetComponent<MetaData>().tileID/*tile.hexTile.idNumber*/ == t.hexTile.idNumber
													 select tu;

			var opentfs = new List<Transform>();
			foreach ( Transform tf in attachtfs )
			{
				float minD = 1000;
				foreach ( Transform utf in usedInThisTile )
				{
					float d = Vector3.Distance( utf.position, tf.position );
					if ( d < minD )
						minD = d;
				}
				if ( minD > 1.1f )
					opentfs.Add( tf );
			}

			finalOpenTFS.AddRange( opentfs );
		}

		//recreate opentfs as hash with UNIQUE items, no dupes
		var openhash = new HashSet<Transform>( finalOpenTFS );
		finalOpenTFS = openhash.Select( x => x ).ToList();
		//Debug.Log( "REQUESTED EVENTS: " + chapter.randomInteractionGroupCount );
		//Debug.Log( "USED POSITIONS: " + usedPositions.Length );
		//Debug.Log( "FOUND POSSIBLE POSITIONS: " + attachtfs.Count );
		//Debug.Log( "FOUND OPEN POSITIONS: " + finalOpenTFS.Count() );

		//sanity check, max number of events based on how many requested and how many actually found in group how many actual open positions
		int max = Mathf.Min( interactionGroupArray.Length, Mathf.Min( chapter.randomInteractionGroupCount, finalOpenTFS.Count() ) );
		//Debug.Log( $"GRABBING {max} EVENTS" );

		//generate random indexes to interactions within the group
		int[] rnds = GlowEngine.GenerateRandomNumbers( max );
		//randomly get randomInteractionGroupCount number of interactions
		IInteraction[] igs = new IInteraction[max];
		for ( int i = 0; i < max; i++ )
		{
			igs[i] = interactionGroupArray[rnds[i]];
			//Debug.Log( $"CHOSE EVENT: {igs[i].dataName} WITH TYPE {igs[i].tokenType}" );
		}

		//create the tokens on random tiles for the interactions we just got
		int[] rands = GlowEngine.GenerateRandomNumbers( max/*opentfs.Count()*/ );
		for ( int i = 0; i < max/*igs.Length*/; i++ )
		{
			//get tile this transform position belongs to
			Tile tile = finalOpenTFS[rands[i]].parent.GetComponent<Tile>();
			//Debug.Log( "TILE #:" + tile.hexTile.idNumber );
			//if the token points to a persistent event, swap the token type with the event it's delegating to

			//create new token prefab for this interaction
			GameObject go = null;
			if ( igs[i].tokenType == TokenType.Search )
			{
				go = Object.Instantiate( tileManager.searchTokenPrefab, tile.transform );
			}
			else if ( igs[i].tokenType == TokenType.Person )
			{
				if ( igs[i].personType == PersonType.Human )
					go = Object.Instantiate( tileManager.humanTokenPrefab, tile.transform );
				else if ( igs[i].personType == PersonType.Elf )
					go = Object.Instantiate( tileManager.elfTokenPrefab, tile.transform );
				else if ( igs[i].personType == PersonType.Hobbit )
					go = Object.Instantiate( tileManager.hobbitTokenPrefab, tile.transform );
				else if ( igs[i].personType == PersonType.Dwarf )
					go = Object.Instantiate( tileManager.dwarfTokenPrefab, tile.transform );
			}
			else if ( igs[i].tokenType == TokenType.Threat )
			{
				go = Object.Instantiate( tileManager.threatTokenPrefab, tile.transform );
			}
			else if ( igs[i].tokenType == TokenType.Darkness )
			{
				go = Object.Instantiate( tileManager.darkTokenPrefab, tile.transform );
			}
			else
			{
				Debug.Log( $"ERROR: TOKEN TYPE SET TO NONE FOR {igs[i].dataName}" );
			}

			go.transform.position = new Vector3( finalOpenTFS[rands[i]].position.x, go.transform.position.y, finalOpenTFS[rands[i]].position.z );
			go.GetComponent<MetaData>().tokenType = HandlePersistentTokenSwap( igs[i].dataName );//igs[i].tokenType;
			go.GetComponent<MetaData>().personType = igs[i].personType;
			go.GetComponent<MetaData>().triggeredByName = "None";
			go.GetComponent<MetaData>().triggerName = "None";
			go.GetComponent<MetaData>().interactionName = igs[i].dataName;
			go.GetComponent<MetaData>().GUID = System.Guid.NewGuid();
			go.GetComponent<MetaData>().isRandom = true;
			//go.GetComponent<MetaData>().isCreatedFromReplaced = false;
			//go.GetComponent<MetaData>().hasBeenReplaced = false;

			tile.tokenStates.Add( new TokenState()
			{
				isActive = false,
				parentTileGUID = tile.hexTile.GUID,
				localPosition = go.transform.localPosition,
				metaData = new MetaDataJSON( go.GetComponent<MetaData>() ),
			} );
		}
	}

	Transform[] AddFixedToken( Tile tile )
	{
		List<Transform> usedPositions = new List<Transform>();
		//List<Vector3> openPositions = new List<Vector3>();
		//openPositions.AddRange( tile.GetChildren( "token attach" ).Select( x => x.position ) );

		foreach ( Token t in tile.hexTile.tokenList )
		{
			//if the token points to a persistent event, swap the token type with the event it's delegating to
			t.tokenType = HandlePersistentTokenSwap( t.triggerName );

			//Debug.Log( t.dataName );
			if ( t.tokenType == TokenType.Exploration || t.tokenType == TokenType.None )//sanity bail out
				continue;

			GameObject go = null;
			if ( t.tokenType == TokenType.Search )
			{
				go = Object.Instantiate( tileManager.searchTokenPrefab, tile.transform );
			}
			else if ( t.tokenType == TokenType.Person )
			{
				if ( t.personType == PersonType.Human )
					go = Object.Instantiate( tileManager.humanTokenPrefab, tile.transform );
				else if ( t.personType == PersonType.Elf )
					go = Object.Instantiate( tileManager.elfTokenPrefab, tile.transform );
				else if ( t.personType == PersonType.Hobbit )
					go = Object.Instantiate( tileManager.hobbitTokenPrefab, tile.transform );
				else if ( t.personType == PersonType.Dwarf )
					go = Object.Instantiate( tileManager.dwarfTokenPrefab, tile.transform );
			}
			else if ( t.tokenType == TokenType.Threat )
			{
				go = Object.Instantiate( tileManager.threatTokenPrefab, tile.transform );
			}
			else if ( t.tokenType == TokenType.Darkness )
			{
				go = Object.Instantiate( tileManager.darkTokenPrefab, tile.transform );
			}

			go.GetComponent<MetaData>().tokenType = t.tokenType;
			go.GetComponent<MetaData>().personType = t.personType;
			go.GetComponent<MetaData>().triggeredByName = t.triggeredByName;
			go.GetComponent<MetaData>().interactionName = t.triggerName;
			go.GetComponent<MetaData>().GUID = t.GUID;
			//offset to token in EDITOR coords
			go.GetComponent<MetaData>().offset = t.vposition - new Vector3( 256, 0, 256 );
			go.GetComponent<MetaData>().isRandom = false;
			go.GetComponent<MetaData>().tileID = tile.hexTile.idNumber;
			//go.GetComponent<MetaData>().isCreatedFromReplaced = false;
			//go.GetComponent<MetaData>().hasBeenReplaced = false;
			//go.GetComponent<MetaData>().isActive = false;

			//calculate position of the Token
			Vector3 offset = go.GetComponent<MetaData>().offset;
			var center = tile.tilemesh.GetComponent<MeshRenderer>().bounds.center;
			var size = tile.tilemesh.GetComponent<MeshRenderer>().bounds.size;
			float scalar = Mathf.Max( size.x, size.z ) / 650f;
			offset *= scalar;
			offset = Vector3.Reflect( offset, new Vector3( 0, 0, 1 ) );
			var tokenPos = new Vector3( center.x + offset.x, 2, center.z + offset.z );
			go.transform.position = tokenPos.Y( 0 );

			usedPositions.Add( go.transform );

			tile.tokenStates.Add( new TokenState()
			{
				isActive = false,
				parentTileGUID = tile.hexTile.GUID,
				localPosition = go.transform.localPosition,
				metaData = new MetaDataJSON( go.GetComponent<MetaData>() ),
			} );
		}

		return usedPositions.ToArray();
	}

	/// <summary>
	/// Used by Replacement Event to replace existing token with new one and update owner tile's MetaDataJSON
	/// </summary>
	public MetaData ReplaceToken( IInteraction sourceEvent, MetaData oldMD, Tile tile )
	{
		GameObject go = null;

		if ( sourceEvent.tokenType == TokenType.Search )
		{
			go = Object.Instantiate( tileManager.searchTokenPrefab, tile.transform );
		}
		else if ( sourceEvent.tokenType == TokenType.Person )
		{
			if ( sourceEvent.personType == PersonType.Human )
				go = Object.Instantiate( tileManager.humanTokenPrefab, tile.transform );
			else if ( sourceEvent.personType == PersonType.Elf )
				go = Object.Instantiate( tileManager.elfTokenPrefab, tile.transform );
			else if ( sourceEvent.personType == PersonType.Hobbit )
				go = Object.Instantiate( tileManager.hobbitTokenPrefab, tile.transform );
			else if ( sourceEvent.personType == PersonType.Dwarf )
				go = Object.Instantiate( tileManager.dwarfTokenPrefab, tile.transform );
		}
		else if ( sourceEvent.tokenType == TokenType.Threat )
		{
			go = Object.Instantiate( tileManager.threatTokenPrefab, tile.transform );
		}
		else if ( sourceEvent.tokenType == TokenType.Darkness )
		{
			go = Object.Instantiate( tileManager.darkTokenPrefab, tile.transform );
		}

		//update old metadataJSON so token is not active
		TokenState oldtstate = tile.tokenStates.Where( x => x.metaData.GUID == oldMD.GUID ).FirstOr( null );
		//swap in relevant metadata from old target
		MetaData newMD = go.GetComponent<MetaData>();

		newMD.tokenType = sourceEvent.tokenType;
		newMD.personType = sourceEvent.personType;
		newMD.triggeredByName = oldMD.triggeredByName;
		newMD.interactionName = sourceEvent.dataName;
		newMD.GUID = sourceEvent.GUID;//oldMD.GUID;
		newMD.offset = oldMD.offset;
		newMD.isRandom = false;
		//newMD.isCreatedFromReplaced = true;
		//newMD.hasBeenReplaced = false;
		newMD.tileID = tile.hexTile.idNumber;
		newMD.transform.position = oldMD.transform.position;
		newMD.gameObject.SetActive( oldMD.gameObject.activeSelf );

		//add new token state for new token
		var ts = new TokenState()
		{
			isActive = oldtstate.isActive,
			parentTileGUID = tile.hexTile.GUID,
			localPosition = go.transform.localPosition,
			metaData = new MetaDataJSON( newMD ),
		};
		tile.tokenStates.Add( ts );

		oldtstate.isActive = false;//make old token inactive

		oldMD.gameObject.SetActive( false );//inactivate old token object
																				//oldMD.hasBeenReplaced = true;//mark it's been replaced

		return newMD;
	}

	/// <summary>
	/// swap token type to delegate event if it's a persistent event
	/// </summary>
	TokenType HandlePersistentTokenSwap( string eventName )
	{
		IInteraction persEvent = GlowEngine.FindObjectOfType<InteractionManager>().GetInteractionByName( eventName );

		if ( persEvent is PersistentInteraction )
		{
			string delname = ( (PersistentInteraction)persEvent ).eventToActivate;
			IInteraction delEvent = GlowEngine.FindObjectOfType<InteractionManager>().GetInteractionByName( delname );
			return delEvent.tokenType;
		}

		return persEvent.tokenType;
	}

	void GenerateGroupCenter()
	{
		groupCenter = GlowEngine.AverageV3( tileList.Select( t => t.transform.position ).ToArray() );
	}

	/// <summary>
	/// animates tile up, reveals Tokens
	/// </summary>
	public void AnimateTileUp( Chapter chapter )
	{
		//Debug.Log( "AnimateTileUp::" + firstChapter );
		//animate upwards
		foreach ( Tile t in tileList )
		{
			Vector3 local = t.transform.position + new Vector3( 0, -.5f, 0 );
			t.transform.position = local;
		}
		float i = 0;
		Tweener tweener = null;
		foreach ( Tile t in tileList )
		{
			tweener = t.transform.DOMoveY( 0, 1.75f ).SetEase( Ease.OutCubic ).SetDelay( i );
			i += .5f;
		}

		//isPreExplored is false for all tiles
		//only Start can be toggled true
		if ( chapter.dataName == "Start" && chapter.isPreExplored )
			tweener?.OnComplete( () => { RevealInteractiveTokens(); } );
		else if ( chapter.dataName == "Start" && !chapter.isPreExplored )
			tweener?.OnComplete( () =>
			{
				RevealExploreToken();
				RevealInteractiveTokens( true );
			} );
		else
			tweener?.OnComplete( () => { RevealExploreToken(); } );
	}

	/// <summary>
	/// Randomly attaches one group to another
	/// </summary>
	public bool AttachTo( TileGroup tgToAttachTo )
	{
		//get all open connectors in THIS tilegroup
		Vector3[] openConnectors = GlowEngine.RandomizeArray( GetOpenConnectors() );
		//get all open anchors on group we're connecting TO
		Vector3[] tgOpenConnectors = GlowEngine.RandomizeArray( tgToAttachTo.GetOpenAnchors() );
		//dummy
		GameObject dummy = new GameObject();
		Vector3[] orTiles = new Vector3[tileList.Count];

		//record original CONTAINER position
		Vector3 or = containerObject.position;
		//record original TILE positions
		for ( int i = 0; i < tileList.Count; i++ )
			orTiles[i] = tileList[i].transform.position;

		bool safe = false;
		foreach ( Vector3 c in openConnectors )
		{
			safe = false;
			//parent each TILE to dummy
			foreach ( Tile tile in tileList )
				tile.transform.parent.transform.parent = dummy.transform;
			//move containerObject to each connector in THIS group
			containerObject.position = c;
			//parent TILES back to containerObject
			foreach ( Tile tile in tileList )
				tile.transform.parent.transform.parent = containerObject.transform;

			//move containerObject to each anchor trying to connect to
			foreach ( Vector3 a in tgOpenConnectors )
			{
				containerObject.position = a;
				//rotate 360 for different orientations?

				//check collision
				if ( !CheckCollisionsWithinGroup( GetAllOpenConnectorsOnBoard() ) )
				{
					safe = true;
					break;
				}
			}

			if ( safe )
			{
				break;
			}
			else
			{
				//Debug.Log( "RESETTING" );
				//reset tilegroup to original position
				containerObject.position = or;
				for ( int i = 0; i < tileList.Count; i++ )
					tileList[i].transform.position = orTiles[i];
			}
		}

		Object.Destroy( dummy );
		if ( !safe )
		{
			Debug.Log( "AttachTo*********NOT FOUND" );
			return false;
		}

		GenerateGroupCenter();
		return true;
	}

	/// <summary>
	/// check collisions between THIS group's CONNECTORS and input test points (CONNECTORS)
	/// </summary>
	/// <returns>true if collision found</returns>
	public bool CheckCollisionsWithinGroup( Transform[] testPoints )
	{
		//List<Vector3> allConnectorsSet = new List<Vector3>();
		//List<Vector3> testVectors = new List<Vector3>();

		//create list of ALL connectors in ALL tiles in the group
		var allConnectorsSet = from tile in tileList from tf in tile.GetChildren( "connector" ) select tf.position;

		//create list of all test point connectors
		var testVectors = from tf in testPoints select tf.position;

		//create list of ALL connectors in ALL tiles in the group
		//foreach ( Tile tile in tileList )
		//{
		//foreach ( Transform t in tile.GetChildren( "connector" ) )
		//	allConnectorsSet.Add( t.position );

		//create list of all test point connectors
		//foreach ( Transform t in testPoints )
		//	testVectors.Add( t.position );

		bool collisionFound = false;

		//failure means that position is taken by a tile = COLLISION
		foreach ( Vector3 tp in testVectors )
		{
			foreach ( Vector3 a in allConnectorsSet )
			{
				float d = Vector3.Distance( a, tp );
				if ( d <= .5 )
					collisionFound = true;
			}
		}

		return collisionFound;
	}

	/// <summary>
	/// Returns ALL connectors in ALL tiles on board across ALL tilegroups (except THIS one)
	/// </summary>
	public Transform[] GetAllOpenConnectorsOnBoard()
	{
		//get all connectors EXCEPT the ones in THIS tilegroup since we'll be testing THIS group's connectors against all OTHERS
		//otherwise it'll test against ITSELF
		var allConnectors = from tg in tileManager.GetAllTileGroups()
												where tg.GUID != GUID
												from tile in tg.tileList
												from tf in tile.GetChildren( "connector" )
												select tf;
		return allConnectors.ToArray();
	}

	/// <summary>
	/// returns all connector positions in the tilegroup
	/// </summary>
	public Vector3[] GetOpenConnectors()
	{
		var foo = from tile in tileList from c in tile.GetChildren( "connector" ) select c.position;
		return foo.ToArray();
	}

	//public Transform[] GetOpenAnchorsTransforms()
	//{
	//	List<Transform> allAnchorsSet = new List<Transform>();
	//	List<Transform> allConnectorsSet = new List<Transform>();
	//	List<Transform> safeAnchors = new List<Transform>();

	//	foreach ( Tile tile in tileList )
	//	{
	//		allAnchorsSet.AddRange( tile.GetChildren( "anchor" ) );
	//		allConnectorsSet.AddRange( tile.GetChildren( "connector" ) );
	//	}

	//	foreach ( Transform a in allAnchorsSet )
	//	{
	//		bool hit = false;
	//		foreach ( Transform c in allConnectorsSet )
	//		{
	//			float d = Vector3.Distance( c.position, a.position );
	//			if ( d <= .5f )
	//				hit = true;
	//		}
	//		if ( !hit && !safeAnchors.Contains( a ) )
	//			safeAnchors.Add( a );
	//	}

	//	return allAnchorsSet.ToArray();//safeAnchors.ToArray();
	//}
	/// <summary>
	/// returns all anchor positions (rounded up) in the group that are open to attach to
	/// </summary>
	public Vector3[] GetOpenAnchors()
	{
		var allAnchors = from tile in tileList from tf in tile.GetChildren( "anchor" ) select tf.position;
		return allAnchors.ToArray();
	}

	public void RemoveGroup()
	{
		Object.Destroy( containerObject.gameObject );
	}

	/// <summary>
	/// drops in the Exploration token ONLY, skips player start tile
	/// </summary>
	public void RevealExploreToken()
	{
		foreach ( Tile t in tileList )
			if ( !t.hexTile.isStartTile )
				t.RevealExplorationToken();
	}

	/// <summary>
	/// only if FIRST tilegroup
	/// </summary>
	public void RevealInteractiveTokens( bool startTileOnly = false )
	{
		//Debug.Log( "RevealInteractiveTokens" );
		foreach ( Tile t in tileList )
			if ( startTileOnly && t.hexTile.isStartTile )
				t.RevealInteractiveTokens();
			else if ( !startTileOnly )
				t.RevealInteractiveTokens();
	}

	/// <summary>
	/// colorize whole group (START chapter ONLY), fire chapter exploreTrigger
	/// </summary>
	public void Colorize( bool onlyStart = false )
	{
		Debug.Log( "EXPLORING GROUP isExplored?::" + isExplored );

		if ( isExplored )
			return;

		//isExplored = true;

		//if it's not the first chapter, set the "on explore" trigger
		//if ( chapter.dataName != "Start" )
		GlowEngine.FindObjectOfType<TriggerManager>().FireTrigger( chapter.exploreTrigger );

		foreach ( Tile t in tileList )
		{
			if ( onlyStart && t.hexTile.isStartTile )
				t.Colorize();
			else if ( !onlyStart )
				t.Colorize();
		}
	}

	public void ActivateTiles()
	{
		foreach ( Tile tile in tileList )
			tile.gameObject.SetActive( true );
	}

	/// <summary>
	/// returns all TokenStates[] that given token is in
	/// </summary>
	public TokenState[] GetTokenListByGUID( System.Guid guid )
	{
		return ( from tile in tileList
						 from tstate in tile.tokenStates
						 where tstate.metaData.GUID == guid
						 select tstate ).ToArray();
	}

	public void SetState( TileGroupState tileGroupState )
	{
		containerObject.position = tileGroupState.globalPosition;
		isExplored = tileGroupState.isExplored;

		foreach ( Tile tile in tileList )
		{
			SingleTileState sts = ( from t in tileGroupState.tileStates
															where t.tileGUID == tile.hexTile.GUID
															select t ).FirstOr( null );
			if ( sts != null )
				tile.SetState( sts );
		}

		GenerateGroupCenter();
	}
}
