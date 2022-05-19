using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class Tile : MonoBehaviour
{
	public Renderer meshRenderer;
	public Vector3 CurrentAnchor { get { return currentAnchor; } }
	[HideInInspector]
	public Vector3 centerPosition { get { return tilemesh.GetComponent<Renderer>().bounds.center; } }
	[HideInInspector]
	public int currentConnectorID;
	[HideInInspector]
	public HexTile hexTile;
	[HideInInspector]
	public bool isExplored { get; set; } = false;
	//rootPosition is the position (connector) where all xforms take place from. Only used for building a loaded scenario (fixed, not random)
	public Transform rootPosition;//world coords
	public TileGroup tileGroup { get; set; }
	public Chapter chapter;

	//offset from connector to center of mesh
	[HideInInspector]
	public GameObject tilemesh;
	Vector3[] connectorOffset;
	Vector3 currentAnchor;
	int anchorCount, connectorCount;
	Transform exploreToken;
	InteractionManager interactionManager;
	float sepiaValue = 1;
	//queue of tokens to fire because tile wasn't explored yet
	[HideInInspector]
	public List<string> tokenTriggerList = new List<string>();
	[HideInInspector]
	public List<TokenState> tokenStates = new List<TokenState>();

	TriggerManager triggerManager;
	CamControl camControl;

	public void Awake()
	{
		interactionManager = FindObjectOfType<InteractionManager>();

		tilemesh = GetChildren( "tile" )[0].gameObject;
		meshRenderer = tilemesh.GetComponent<Renderer>();
		anchorCount = GetCount( "anchor" );
		connectorCount = GetCount( "connector" );
		connectorOffset = new Vector3[connectorCount];

		int c = 0;
		for ( int i = 0; i < transform.childCount; i++ )
		{
			Transform child = transform.GetChild( i );

			if ( child.name.Contains( "connector" ) )
			{
				//calculate LOCAL offsets to each connector
				//connectorOffset[c++] = child.localPosition - Vector3.zero;
				connectorOffset[c++] = child.position - transform.position;
			}
		}

		exploreToken = transform.Find( "Exploration Token" ).transform;
		exploreToken.localPosition = new Vector3( exploreToken.localPosition.x, 2, exploreToken.localPosition.z );
		exploreToken.gameObject.SetActive( false );

		meshRenderer.material.SetFloat( "_sepiaValue", 1 );

		triggerManager = FindObjectOfType<TriggerManager>();
		camControl = FindObjectOfType<CamControl>();
		//StartCoroutine( Wait1Frame() );
	}

	public void EnqueueTokenTrigger( string name )
	{
		Debug.Log( "Tile EnqueueTokenTrigger: " + name );
		if ( !tokenTriggerList.Contains( name ) )
			tokenTriggerList.Add( name );
	}

	//public Vector3 GetExploretokenPosition()
	//{
	//	Transform tf = transform.Find( "Exploration Token" );
	//	return new Vector3( tf.position.x, .26f, tf.position.z );
	//}

	//IEnumerator Wait1Frame()
	//{
	//	yield return null;
	//}

	public int GetCount( string name )
	{
		int c = 0;
		for ( int i = 0; i < transform.childCount; i++ )
			if ( transform.GetChild( i ).name.Contains( name ) )
				c++;
		//Debug.Log( "GetCount::" + name + "::" + c );
		return c;
	}

	/// <summary>
	/// gets all children whose name CONTAINS given string
	/// </summary>
	public Transform[] GetChildren( string name )
	{
		Transform[] t = new Transform[GetCount( name )];
		int c = 0;
		foreach ( Transform child in transform )
		{
			if ( child.name.Contains( name ) )
				t[c++] = child;
		}

		return t;
	}

	public bool IsDarknessTokenActive()
	{
		Transform[] dark = GetChildren( "Darkness Token" );

		foreach ( Transform child in dark )
		{
			if ( child.gameObject.activeInHierarchy )
				return true;
		}
		return false;
	}

	//public bool CheckCollision2()
	//{
	//	bool foundHit = false;
	//	Transform[] connectors = GetChildren( "connector" );

	//	for ( int i = 0; i < connectors.Length; i++ )
	//	{
	//		Vector3 connector = connectors[i].position + new Vector3( 0, 1, 0 );
	//		var hits = Physics.RaycastAll( connector, Vector3.down, 5 );
	//		foreach ( var hit in hits )
	//		{
	//			if ( hit.collider.name != "hit Plane"
	//				&& hit.collider.transform.parent.name != name )
	//			{
	//				//Debug.Log( "connector " + connectors[i].name + " HIT WITH::" + hit.collider?.gameObject.name );
	//				foundHit = true;
	//			}
	//		}
	//	}
	//	if ( foundHit )
	//		Debug.Log( name + " DETECTED COLLISION" );
	//	else
	//		Debug.Log( name + " DETECTED NO COLLISION" );

	//	return foundHit;
	//}

	/// <summary>
	/// returns TRUE if COLLISION
	/// </summary>
	public bool CheckCollision()
	{
		//new Vector3( -1.5f, 0, -0.4330127f )
		//int mask1 = 1 << 2;
		//int mask2 = 1 << 9;
		int mask = 1 << 10;
		//layerMask = ~layerMask;//inverse, NOT
		//~( mask1 | mask2 )

		tilemesh.layer = 2;

		bool foundHit = false;
		RaycastHit hit;

		Transform[] connectors = GetChildren( "connector" );

		for ( int i = 0; i < connectors.Length; i++ )
		{
			Vector3 connector = connectors[i].position + new Vector3( 0, 1, 0 );
			//Debug.Log( "CONNECTOR WORLD::" + connectors[i].name + "::" + connector );
			if ( Physics.Raycast( connector, Vector3.down, out hit, 3, mask ) )
			{
				foundHit = true;
				//Debug.Log( "connector " + connectors[i].name + " HIT WITH::" + hit.collider?.gameObject.name );
				//Debug.Log( "COLLIDED WITH::" + hit.collider?.gameObject.transform.position );
				break;
			}
		}

		if ( foundHit )
			Debug.Log( name + " DETECTED COLLISION" );
		else
			Debug.Log( name + " DETECTED NO COLLISION" );

		tilemesh.layer = 10;

		return foundHit;
	}

	/// <summary>
	/// Tile MOVES when setting Connector
	/// </summary>
	public void SetConnector( int idx )
	{
		//Debug.Log( "SetConnector::" + gameObject.name );
		//Debug.Log( "SetConnector()::idx=" + idx );
		//Debug.Log( "SetConnector()::connectorCount=" + connectorCount );
		if ( idx >= connectorCount )
		{
			Debug.Log( "SetConnector()::idx >= SetConnector" );
			return;
		}
		currentConnectorID = idx;
		transform.localPosition = Vector3.zero;
		transform.localPosition -= connectorOffset[idx];
	}

	//public void SetRandomConnector()
	//{
	//	int c = Random.Range( 0, connectorCount );
	//	currentConnectorID = c + 1;//1-based
	//	transform.localPosition = Vector3.zero;
	//	transform.localPosition -= connectorOffset[c];

	//	CheckCollision();
	//}

	public void SetAnchor( int idx )
	{
		//Debug.Log( "SetAnchor::" + gameObject.name );
		//Debug.Log( "SetAnchor()::idx=" + idx );
		//Debug.Log( "SetAnchor()::anchorCount=" + anchorCount );
		if ( idx >= anchorCount )
		{
			Debug.Log( "SetAnchor()::idx >= anchorCount" );
			return;
		}
		Transform[] anchors = GetChildren( "anchor" );
		currentAnchor = anchors[idx].position;
		//Debug.Log( "currentAnchor:" + anchors[idx].name );
		//Debug.Log( "currentAnchor=" + currentAnchor );
	}

	//public void SetRandomAnchor()
	//{
	//	Transform[] anchors = GetChildren( "anchor" );
	//	currentAnchor = anchors[Random.Range( 0, anchorCount )].position;
	//}

	//WORLD coords
	public void AttachTo( Vector3 anchor )
	{
		transform.parent.transform.position = anchor;
		transform.parent.transform.Translate( Vector3.zero );
	}

	/// <summary>
	/// Randomly attaches 2 tiles (random anchor/connector) within a given group, tile=previous tile already on board
	/// </summary>
	public void AttachTo( Tile tile, TileGroup tg )
	{
		//anchors = white outer transforms
		//connectors = red inner transforms
		Transform[] anchorPoints = tile.GetChildren( "anchor" );
		int[] ra = GlowEngine.GenerateRandomNumbers( anchorPoints.Length );
		int[] rc = GlowEngine.GenerateRandomNumbers( connectorCount );
		bool success = false;

		for ( int c = 0; c < connectorCount; c++ )
		{
			for ( int a = 0; a < anchorPoints.Length; a++ )//white anchors on board
			{
				tile.SetAnchor( ra[a] );
				SetConnector( rc[c] );
				AttachTo( tile.currentAnchor );
				Transform[] ap = GetChildren( "connector" );
				success = !tg.CheckCollisionsWithinGroup( ap );
				if ( success )
					break;
			}
			if ( success )
				break;
		}

		if ( !success )
		{
			Debug.Log( "FAILED TO FIND OPEN TILE LOCATION" );
			throw new System.Exception( "FAILED TO FIND OPEN TILE LOCATION" );
		}
	}

	public void SetPosition( Vector3 worldpos, float angle )
	{
		//rootPosition is the position where all xforms take place from
		//get vector from root hextile to center of model
		Vector3 v = transform.position - rootPosition.position;
		//move the parent into place
		transform.parent.transform.position = worldpos;
		//offset model from parent
		transform.localPosition = v;
		//rotate parent transform
		transform.parent.localRotation = Quaternion.Euler( 0, angle, 0 );
	}

	public void RevealExplorationToken()
	{
		exploreToken.gameObject.SetActive( true );
		exploreToken.localScale = Vector3.one;
		exploreToken.position = exploreToken.position.Y( 2f );
		exploreToken.DOLocalMoveY( .3f, 1 ).SetEase( Ease.OutBounce );
	}

	public void RevealInteractiveTokens()
	{
		RevealToken( TokenType.Search );
		RevealToken( TokenType.Person );
		RevealToken( TokenType.Threat );
		RevealToken( TokenType.Darkness );
	}

	/// <summary>
	/// Sets isExplored=true and does token removal animation
	/// </summary>
	public void RemoveExplorationToken()
	{
		isExplored = true;
		tileGroup.isExplored = true;
		//hexTile.isExplored = true;
		Sequence sequence = DOTween.Sequence();
		sequence.Append( exploreToken.DOLocalMoveY( 1, 1 ).SetEase( Ease.InOutQuad ) );
		sequence.Join( exploreToken.DOScale( 0, 1 ) );
		sequence.Play().OnComplete( () => { exploreToken.gameObject.SetActive( false ); } );
	}

	public void RemoveInteractivetoken( Transform tf, MetaData metaData )
	{
		//mark token state as inactive
		TokenState ts;
		if ( !metaData.isRandom )
			ts = tokenStates.Where( x => x.metaData.GUID == metaData.GUID ).FirstOr( null );
		else
			ts = tokenStates.Where( x => x.metaData.interactionName == metaData.interactionName ).FirstOr( null );
		if ( ts != null )
			ts.isActive = false;

		Sequence sequence = DOTween.Sequence();
		sequence.Append( tf.DOLocalMoveY( 1, 1 ).SetEase( Ease.InOutQuad ) );
		sequence.Join( tf.DOScale( 0, 1 ) );
		sequence.Play().OnComplete( () => { tf.gameObject.SetActive( false ); } );
	}

	/// <summary>
	/// Explore tile - colorize only
	/// </summary>
	public void Colorize()
	{
		isExplored = true;
		//hexTile.isExplored = true;

		DOTween.To( () => sepiaValue, x =>
		{
			sepiaValue = x;
			meshRenderer.material.SetFloat( "_sepiaValue", sepiaValue );
		}, 0, 2f );
	}

	/// <summary>
	/// reveal/drop token of specified type onto the tile ONLY if it's not a triggered Token (TriggeredBy)
	/// </summary>
	void RevealToken( TokenType ttype )
	{
		//var size = tilemesh.GetComponent<MeshRenderer>().bounds.size;
		var center = tilemesh.GetComponent<MeshRenderer>().bounds.center;
		Transform[] tf = GetChildren( ttype.ToString() );

		for ( int i = 0; i < tf.Length; i++ )
		{
			TokenState tState = null;
			MetaData metaData = tf[i].GetComponent<MetaData>();
			//only want FIXED tokens
			if ( !metaData.isRandom )//&& !metaData.hasBeenReplaced )
			{
				string tBy = metaData.triggeredByName;
				//skip if it's triggeredBy
				if ( tBy != "None" )
				{
					//if it's not in the list, keep it hidden because it hasn't activated yet, move to next token in loop
					if ( !tokenTriggerList.Contains( tBy ) )//( tBy != "None" )
						continue;
				}

				//offset to token in EDITOR coords
				//Vector3 offset = tf[i].GetComponent<MetaData>().offset;
				////Debug.Log( "EDITOR offset:" + offset );
				//float scalar = Mathf.Max( size.x, size.z ) / 650f;
				//offset *= scalar;
				//offset = Vector3.Reflect( offset, new Vector3( 0, 0, 1 ) );

				//tf[i].position = new Vector3( center.x + offset.x, 2, center.z + offset.z );
				tf[i].gameObject.SetActive( true );
				tf[i].position = tf[i].position.Y( 2 );
				tf[i].RotateAround( center, Vector3.up, hexTile.angle );
				tf[i].DOLocalMoveY( .3f, 1 ).SetEase( Ease.OutBounce );

				//update token state to active
				tState = tokenStates.Where( x => x.metaData.GUID == metaData.GUID ).FirstOr( null );
			}
			else //if ( !metaData.hasBeenReplaced )
			{
				//random tokens are already placed during tile creation using preset transforms built into the mesh "token attach"
				tf[i].gameObject.SetActive( true );
				tf[i].position = tf[i].position.Y( 2 );
				tf[i].DOLocalMoveY( .3f, 1 ).SetEase( Ease.OutBounce );

				//update token state to active
				tState = tokenStates.Where( x => x.metaData.interactionName == metaData.interactionName ).FirstOr( null );
			}

			if ( tState != null )
			{
				tState.isActive = true;
				tState.localPosition = tf[i].localPosition.Y( .3f );
			}
		}
	}

	public bool HasTriggeredToken( string name )
	{
		bool found = false;

		Transform[] tf = GetChildren( "Token" );
		for ( int i = 0; i < tf.Length; i++ )
		{
			string tBy = tf[i].GetComponent<MetaData>().triggeredByName;
			if ( tBy != name )
				continue;
			else
				found = true;
		}

		return found;
	}

	/// <summary>
	/// Reveals ALL triggered tokens on this tile from TryTriggerToken()
	/// </summary>
	public Vector3[] RevealTriggeredTokens( string tname )
	{
		var size = tilemesh.GetComponent<MeshRenderer>().bounds.size;
		var center = tilemesh.GetComponent<MeshRenderer>().bounds.center;
		Transform[] tf = GetChildren( "Token(Clone)" );
		//Vector3 tpos = ( -12345f ).ToVector3();
		List<Vector3> tpos = new List<Vector3>();

		for ( int i = 0; i < tf.Length; i++ )
		{
			MetaData tfmetaData = tf[i].GetComponent<MetaData>();
			string tBy = tfmetaData.triggeredByName;
			if ( tBy != tname )//|| tfmetaData.hasBeenReplaced )
				continue;

			//offset to token in EDITOR coords
			Vector3 offset = tfmetaData.offset;
			float scalar = Mathf.Max( size.x, size.z ) / 650f;
			offset *= scalar;
			offset = Vector3.Reflect( offset, new Vector3( 0, 0, 1 ) );

			tf[i].gameObject.SetActive( true );
			tf[i].position = new Vector3( center.x + offset.x, 2, center.z + offset.z );
			tf[i].RotateAround( center, Vector3.up, hexTile.angle );
			tf[i].DOLocalMoveY( .3f, 1 ).SetEase( Ease.OutBounce );
			tpos.Add( tf[i].position );

			//mark active in token state
			//MetaData metaData = tf[i].GetComponent<MetaData>();
			TokenState tState = null;
			if ( !tfmetaData.isRandom )
			{
				tState = tokenStates.Where( x => x.metaData.GUID == tf[i].GetComponent<MetaData>().GUID ).FirstOr( null );
			}
			else
			{
				tState = tokenStates.Where( x => x.metaData.interactionName == tf[i].GetComponent<MetaData>().interactionName ).FirstOr( null );
			}
			if ( tState != null )
			{
				tState.isActive = true;
				tState.localPosition = tf[i].localPosition.Y( .3f );
			}
		}

		return tpos.ToArray();
	}

	/// <summary>
	/// Handle clicking on tokens
	/// </summary>
	public bool InputUpdate( Ray ray )
	{
		if ( FindObjectOfType<ShadowPhaseManager>().doingShadowPhase )
			return false;

		if ( Physics.Raycast( ray, out RaycastHit hit ) )
		{
			Transform objectHit = hit.transform;
			if ( objectHit.name == "Exploration Token" )
			{
				var objs = FindObjectsOfType<SpawnMarker>();
				foreach ( var ob in objs )
				{
					if ( ob.name.Contains( "SPAWNMARKER" ) )
						Destroy( ob.gameObject );
					if ( ob.name == "STARTMARKER" )
						ob.gameObject.SetActive( false );
				}

				Tile tile = objectHit.parent.GetComponent<Tile>();

				interactionManager.GetNewTextPanel().ShowQueryExploration( ( res ) =>
				{
					if ( res.btn1 )
					{
						ShowExplorationText( tile, () =>
						 {
							 tile.RemoveExplorationToken();
							 tile.Colorize();
							 tile.RevealInteractiveTokens();
							 //fire trigger on chapter exploration
							 triggerManager.FireTrigger( tile.chapter.exploreTrigger );
							 //fire trigger on tile exploration
							 triggerManager.FireTrigger( tile.hexTile.triggerName );
							 //objectHit.parent.GetComponent<Tile>().tileGroup.ExploreTile();
						 } );
					}
				} );

				camControl.MoveTo( tile.centerPosition, .2f );
				return true;
			}
			else if ( objectHit.name.Contains( "Token" ) )
			{
				camControl.MoveTo( objectHit.parent.GetComponent<Tile>().centerPosition, .2f );
				QueryTokenInteraction( objectHit );
				return true;
			}
		}
		return false;
	}

	void ShowExplorationText( Tile tile, System.Action action )
	{
		string flavor = tile.hexTile.flavorBookData.pages.Count > 0 ? tile.hexTile.flavorBookData.pages[0] : "";
		string instructions = "Discard the exploration token.";
		if ( Bootstrap.gameStarter.difficulty != Difficulty.Hard )
			instructions += " Gain 1 inspiration.";
		if ( !string.IsNullOrEmpty( flavor ) )
			flavor = flavor + "\r\n\r\n" + instructions;
		else
			flavor = instructions;

		interactionManager.GetNewTextPanel().ShowOkContinue( flavor, ButtonIcon.Continue, action );
	}

	/// <summary>
	/// Show interaction flavor text, see if player wants to use an Action
	/// </summary>
	void QueryTokenInteraction( Transform objectHit )
	{
		MetaData metaData = objectHit.GetComponent<MetaData>();
		string objectEventName = metaData.interactionName;
		string objectEventToken = metaData.tokenType.ToString();
		Tile tile = objectHit.parent.GetComponent<Tile>();

		IInteraction inter = interactionManager.GetInteractionByName( metaData.interactionName );

		//handle edge case PersistentInteraction
		if ( inter is PersistentInteraction )
		{
			//ONLY swap in delegate event if the pers event hasn't had its alt text triggered
			if ( !FindObjectOfType<TriggerManager>().IsTriggered( ( (PersistentInteraction)inter ).alternativeTextTrigger ) )
			{
				objectEventName = ( (PersistentInteraction)inter ).eventToActivate;
				IInteraction delegateInteraction = interactionManager.GetInteractionByName( objectEventName );
				//delegate action to this event
				objectEventToken = delegateInteraction.tokenType.ToString();
				//make it persistent
				delegateInteraction.isPersistent = true;
			}
		}

		interactionManager.QueryTokenInteraction( objectEventName, objectEventToken, ( res ) =>
	{
		if ( res.btn2 )
		{
			Debug.Log( "INTERACT::" + res.interaction.dataName );
			if ( res.interaction.interactionType != InteractionType.Persistent )
			{
				interactionManager.ShowInteraction( res.interaction, objectHit, ( iresult ) =>
				{
					if ( !res.interaction.isPersistent && iresult.removeToken )
						tile.RemoveInteractivetoken( objectHit, metaData );
				} );
			}
			else
				Debug.Log( "Persistent Event, doing nothing" );
		}
		else
			Debug.Log( "NO BTN2" );
	} );
	}

	void CreateToken( TokenState tokenState )
	{
		GameObject go = null;
		TileManager tileManager = FindObjectOfType<TileManager>();

		if ( tokenState.metaData.tokenType == TokenType.Search )
		{
			go = Object.Instantiate( tileManager.searchTokenPrefab, gameObject.transform );
		}
		else if ( tokenState.metaData.tokenType == TokenType.Person )
		{
			if ( tokenState.metaData.personType == PersonType.Human )
				go = Object.Instantiate( tileManager.humanTokenPrefab, gameObject.transform );
			else if ( tokenState.metaData.personType == PersonType.Elf )
				go = Object.Instantiate( tileManager.elfTokenPrefab, gameObject.transform );
			else if ( tokenState.metaData.personType == PersonType.Hobbit )
				go = Object.Instantiate( tileManager.hobbitTokenPrefab, gameObject.transform );
			else if ( tokenState.metaData.personType == PersonType.Dwarf )
				go = Object.Instantiate( tileManager.dwarfTokenPrefab, gameObject.transform );
		}
		else if ( tokenState.metaData.tokenType == TokenType.Threat )
		{
			go = Object.Instantiate( tileManager.threatTokenPrefab, gameObject.transform );
		}
		else if ( tokenState.metaData.tokenType == TokenType.Darkness )
		{
			go = Object.Instantiate( tileManager.darkTokenPrefab, gameObject.transform );
		}

		MetaData newMD = go.GetComponent<MetaData>();

		newMD.triggerName = tokenState.metaData.triggerName;
		newMD.interactionName = tokenState.metaData.interactionName;
		newMD.triggeredByName = tokenState.metaData.triggeredByName;
		newMD.tokenType = tokenState.metaData.tokenType;
		newMD.personType = tokenState.metaData.personType;
		newMD.offset = tokenState.metaData.offset;
		newMD.GUID = tokenState.metaData.GUID;
		newMD.isRandom = tokenState.metaData.isRandom;
		newMD.tileID = hexTile.idNumber;

		newMD.transform.localScale = Vector3.one;
		newMD.transform.localPosition = tokenState.localPosition;
		newMD.gameObject.SetActive( tokenState.isActive );
	}

	public void SetState( SingleTileState singleTileState )
	{
		isExplored = singleTileState.isExplored;
		if ( isExplored )
		{
			if ( singleTileState.isActive )
				exploreToken.gameObject.SetActive( false );
			DOTween.To( () => 0, x =>
			{
				sepiaValue = x;
				meshRenderer.material.SetFloat( "_sepiaValue", sepiaValue );
			}, 0, 2f );
		}
		else
		{
			if ( singleTileState.isActive )
				RevealExplorationToken();
			else
			{
				exploreToken.gameObject.SetActive( false );
				exploreToken.position = exploreToken.position.Y( 2f );
			}

			DOTween.To( () => 1, x =>
			{
				sepiaValue = x;
				meshRenderer.material.SetFloat( "_sepiaValue", sepiaValue );
			}, 1, 2f );
		}

		///RESTORE TILE STATE
		transform.parent.position = singleTileState.globalParentPosition;
		transform.parent.rotation = Quaternion.Euler( 0, singleTileState.globalParentYRotation, 0 );
		transform.position = singleTileState.globalPosition;
		tokenTriggerList = singleTileState.tokenTriggerList;
		tokenStates = singleTileState.tokenStates;
		gameObject.SetActive( singleTileState.isActive );

		///RESTORE TOKENS
		//clear tokenstates list and remove all tokens on tile
		tokenStates = singleTileState.tokenStates;
		//get all token on this tile
		Transform[] tf = GetChildren( "Token(Clone)" );
		var remove = tf.Select( x => x.gameObject );
		foreach ( var rmd in remove )
			Destroy( rmd.gameObject );
		//recreate tokens
		foreach ( TokenState ts in singleTileState.tokenStates )
		{
			CreateToken( ts );
		}

		//remove any tokens that are created from replaced event
		//only important for quick loading
		//var replacedMDs = tf.Where( x => x.GetComponent<MetaData>().isCreatedFromReplaced );
		//foreach ( var rmd in replacedMDs )
		//	Destroy( rmd.gameObject );


		//restore token states
		/*for ( int i = 0; i < tf.Length; i++ )
		{
			//token on tile
			MetaData md = tf[i].GetComponent<MetaData>();
			//find matching token state to token on tile
			TokenState token = ( from mjson in singleTileState.tokenStates
													 where mjson.metaData.GUID == md.GUID || mjson.metaData.interactionName == md.interactionName
													 select mjson ).FirstOr( null );
			if ( token != null )
			{
				md.gameObject.SetActive( token.isActive );
				md.transform.localScale = Vector3.one;
				md.transform.localPosition = token.localPosition;
				md.triggerName = token.metaData.triggerName;
				md.interactionName = token.metaData.interactionName;
				md.triggeredByName = token.metaData.triggeredByName;
				md.tokenType = token.metaData.tokenType;
				md.offset = token.metaData.offset;
				md.isRandom = token.metaData.isRandom;
				md.tileID = token.metaData.tileID;
				md.hasBeenReplaced = token.metaData.hasBeenReplaced;
				//md.isActive = token.metaData.isActive;
			}
		}*/
	}
}
