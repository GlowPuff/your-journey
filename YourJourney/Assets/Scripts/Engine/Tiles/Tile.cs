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
	public bool isExplored = false;
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

	void Awake()
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
				connectorOffset[c++] = child.localPosition - Vector3.zero;
			}
		}

		exploreToken = transform.Find( "Exploration Token" ).transform;
		exploreToken.localPosition = new Vector3( exploreToken.localPosition.x, 2, exploreToken.localPosition.z );
		exploreToken.gameObject.SetActive( false );

		meshRenderer.material.SetFloat( "_sepiaValue", 1 );
		//StartCoroutine( Wait1Frame() );
	}

	public Vector3 GetExploretokenPosition()
	{
		Transform tf = transform.Find( "Exploration Token" );
		return new Vector3( tf.position.x, .26f, tf.position.z );
	}

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
		if ( idx >= connectorCount )
			return;
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
		if ( idx >= anchorCount )
			return;
		Transform[] anchors = GetChildren( "anchor" );
		currentAnchor = anchors[idx].position;
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
	/// Randomly attaches 2 tiles (random anchor/connector) within a given group
	/// </summary>
	public void AttachTo( Tile tile, TileGroup tg )
	{
		Transform[] anchorPoints = tile.GetChildren( "anchor" );
		int[] ra = GlowEngine.GenerateRandomNumbers( anchorPoints.Length );
		int[] rc = GlowEngine.GenerateRandomNumbers( connectorCount );
		bool success = false;

		for ( int c = 0; c < connectorCount; c++ )
		{
			for ( int a = 0; a < anchorPoints.Length; a++ )
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
			Debug.Log( "FAILED TO FIND SPOT" );
			throw new System.Exception( "FAILED TO FIND SPOT" );
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
		exploreToken.DOLocalMoveY( .3f, 1 ).SetEase( Ease.OutBounce );
	}

	public void RevealInteractiveTokens()
	{
		RevealToken( TokenType.Search );
		RevealToken( TokenType.Person );
		RevealToken( TokenType.Threat );
		RevealToken( TokenType.Darkness );
	}

	public void RemoveExplorationToken()
	{
		isExplored = true;
		Sequence sequence = DOTween.Sequence();
		sequence.Append( exploreToken.DOLocalMoveY( 1, 1 ).SetEase( Ease.InOutQuad ) );
		sequence.Join( exploreToken.DOScale( 0, 1 ) );
		sequence.Play().OnComplete( () => { exploreToken.gameObject.SetActive( false ); } );
	}

	public void RemoveInteractivetoken( Transform tf )
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append( tf.DOLocalMoveY( 1, 1 ).SetEase( Ease.InOutQuad ) );
		sequence.Join( tf.DOScale( 0, 1 ) );
		sequence.Play().OnComplete( () => { tf.gameObject.SetActive( false ); } );
	}

	/// <summary>
	/// Explore tile - colorize and optionally show tokens
	/// </summary>
	public void Colorize( bool revealTokens = false )
	{
		//if ( revealTokens )
		//{
		//	RevealToken( TokenType.Search );
		//	RevealToken( TokenType.Person );
		//	RevealToken( TokenType.Threat );
		//	RevealToken( TokenType.Darkness );
		//}
		isExplored = true;

		DOTween.To( () => sepiaValue, x =>
		{
			sepiaValue = x;
			meshRenderer.material.SetFloat( "_sepiaValue", sepiaValue );
		}, 0, 2f );
	}

	/// <summary>
	/// reveal/drop token of specified type onto the tile (ONLY if it's not TriggeredBy)
	/// </summary>
	void RevealToken( TokenType ttype )
	{
		//var size = tilemesh.GetComponent<MeshRenderer>().bounds.size;
		var center = tilemesh.GetComponent<MeshRenderer>().bounds.center;
		Transform[] tf = GetChildren( ttype.ToString() );

		for ( int i = 0; i < tf.Length; i++ )
		{
			if ( !tf[i].GetComponent<MetaData>().isRandom )
			{
				string tBy = tf[i].GetComponent<MetaData>().triggeredByName;
				//skip if it's triggeredBy
				if ( tBy != "None" )
					continue;
				//offset to token in EDITOR coords
				//Vector3 offset = tf[i].GetComponent<MetaData>().offset;
				////Debug.Log( "EDITOR offset:" + offset );
				//float scalar = Mathf.Max( size.x, size.z ) / 650f;
				//offset *= scalar;
				//offset = Vector3.Reflect( offset, new Vector3( 0, 0, 1 ) );

				//tf[i].position = new Vector3( center.x + offset.x, 2, center.z + offset.z );
				tf[i].position = tf[i].position.Y( 2 );
				tf[i].RotateAround( center, Vector3.up, hexTile.angle );
				tf[i].DOLocalMoveY( .3f, 1 ).SetEase( Ease.OutBounce );
			}
			else
			{
				//random tokens are already placed during tile creation using preset transforms built into the mesh "token attach"
				tf[i].position = tf[i].position.Y( 2 );
				tf[i].DOLocalMoveY( .3f, 1 ).SetEase( Ease.OutBounce );
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

	public Vector3 RevealTriggeredToken( string tname )
	{
		var size = tilemesh.GetComponent<MeshRenderer>().bounds.size;
		var center = tilemesh.GetComponent<MeshRenderer>().bounds.center;
		Transform[] tf = GetChildren( "Token" );
		Vector3 tpos = ( -12345f ).ToVector3();

		for ( int i = 0; i < tf.Length; i++ )
		{
			string tBy = tf[i].GetComponent<MetaData>().triggeredByName;
			if ( tBy != tname )
				continue;
			//offset to token in EDITOR coords
			Vector3 offset = tf[i].GetComponent<MetaData>().offset;
			float scalar = Mathf.Max( size.x, size.z ) / 650f;
			offset *= scalar;
			offset = Vector3.Reflect( offset, new Vector3( 0, 0, 1 ) );

			tf[i].position = new Vector3( center.x + offset.x, 2, center.z + offset.z );
			tf[i].RotateAround( center, Vector3.up, hexTile.angle );
			tf[i].DOLocalMoveY( .3f, 1 ).SetEase( Ease.OutBounce );
			tpos = tf[i].position;
		}

		return tpos;
	}

	/// <summary>
	/// Handle clicking on tokens
	/// </summary>
	public bool InputUpdate( Ray ray )
	{
		if ( Physics.Raycast( ray, out RaycastHit hit ) )
		{
			Transform objectHit = hit.transform;
			if ( objectHit.name == "Exploration Token" )
			{
				interactionManager.GetNewTextPanel().ShowQueryExploration( ( res ) =>
				{
					if ( res.btn1 )
					{
						ShowExplorationText( objectHit.parent.GetComponent<Tile>(), () =>
						 {
							 objectHit.parent.GetComponent<Tile>().RemoveExplorationToken();
							 objectHit.parent.GetComponent<Tile>().Colorize();
							 objectHit.parent.GetComponent<Tile>().RevealInteractiveTokens();
							 //fire trigger on exploration
							 FindObjectOfType<TriggerManager>().FireTrigger( objectHit.parent.GetComponent<Tile>().chapter.exploreTrigger );
							 //objectHit.parent.GetComponent<Tile>().tileGroup.ExploreTile();
						 } );
					}
				} );

				FindObjectOfType<CamControl>().MoveTo( objectHit.parent.GetComponent<Tile>().centerPosition, .2f );
				return true;
			}
			else if ( objectHit.name.Contains( "Token" ) )
			{
				FindObjectOfType<CamControl>().MoveTo( objectHit.parent.GetComponent<Tile>().centerPosition, .2f );
				QueryTokenInteraction( objectHit );
				return true;
			}
		}
		return false;
	}

	void ShowExplorationText( Tile tile, System.Action action )
	{
		string s = tile.hexTile.flavorBookData.pages.Count > 0 ? tile.hexTile.flavorBookData.pages[0] : "";
		if ( !string.IsNullOrEmpty( s ) )
			interactionManager.GetNewTextPanel().ShowOkContinue( tile.hexTile.flavorBookData.pages[0], ButtonIcon.Continue, action );
		else
			action?.Invoke();
	}

	/// <summary>
	/// Show interaction flavor text, see if player wants to use an Action
	/// </summary>
	void QueryTokenInteraction( Transform objectHit )
	{
		interactionManager.QueryTokenInteraction( objectHit.GetComponent<MetaData>().interactionName, objectHit.GetComponent<MetaData>().tokenType.ToString(), ( res ) =>
		{
			if ( res.btn2 )
			{
				Debug.Log( "INTERACT::" + res.interaction.dataName );
				DoTokenAction( res.interaction, objectHit );
				if ( res.removeToken )
					RemoveInteractivetoken( objectHit );
			}
		} );
	}

	void DoTokenAction( IInteraction interaction, Transform objectHit )
	{
		Debug.Log( "do interaction::" + interaction.dataName );
		interactionManager.ShowInteraction( interaction, objectHit, ( a ) =>
		 {
			 if ( a.removeToken )
				 RemoveInteractivetoken( objectHit );
		 } );
	}
}
