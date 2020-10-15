using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CamControl : MonoBehaviour
{
	public Transform uiRoot;
	public float focusDistMin = 1.35f, focusDistMax = 3f;
	public PostProcessProfile postProcProf;
	public PartyPanel partyPanel;

	CombatPanel combatPanel;
	Camera cam;

	//old Z = -2.40108
	public float moveDragSpeed = .005f, rotateSpeed = 30, rotateDuration = .25f, doubleClickSpeed = .25f, smoothSpeed;

	float rotateAmount = 0, dClickTimer;
	Vector2 dragStart;
	Vector3 moveStart, targetPos, targetZoom, DOF, targetDOF, targetLookAt;
	int dClickCount;
	bool dragging = false;
	Vector3 dragOrigin;

	void Awake()
	{
		cam = Camera.main;
		cam.transform.LookAt( transform );
		targetZoom = cam.transform.localPosition;
		DOF = Vector3.one;
		combatPanel = FindObjectOfType<CombatPanel>();
	}

	//void lookAt()
	//{
	//	cam.transform.DOLookAt( target.transform.position, 2, AxisConstraint.Y | AxisConstraint.X, Vector3.up ).SetEase( Ease.InOutQuad );
	//}

	void Update()
	{
		//position
		if ( !dragging )
			transform.position = Vector3.Lerp( transform.position, targetPos, smoothSpeed );
		//zoom
		cam.transform.localPosition = Vector3.Lerp( cam.transform.localPosition, targetZoom, .5f );
		//lookat
		cam.transform.localEulerAngles = Vector3.Lerp( cam.transform.localEulerAngles, targetLookAt, .5f );
		//dof
		//DOF = Vector3.Lerp( DOF, targetDOF, .2f );
		//postProcProf.TryGetSettings<DepthOfField>( out DepthOfField dof );
		//FloatParameter fp = new FloatParameter
		//{
		//	value = DOF.x
		//};
		//dof.focusDistance.value = fp;

		//disable camera control if panels are showing
		if ( uiRoot.childCount > 0
			|| combatPanel.gameObject.activeInHierarchy
			|| partyPanel.gameObject.activeInHierarchy )
			return;

		HandleDragging();
		HandleRotation();
		HandleMove();
		HandleZoom();

		//junk
		//timer -= Time.deltaTime;
		//if ( timer <= 0 )
		//{
		//	if ( target.position != lastTargetPos )
		//	{
		//		timer = 2.1f;
		//		look();
		//		lastTargetPos = target.position;
		//	}
		//	else
		//		timer = .5f;
		//}
	}

	void HandleZoom()
	{
		float axis = Input.GetAxis( "Mouse ScrollWheel" );
		float y = cam.transform.localPosition.y;

		// scroll up
		if ( axis > 0f )
		{
			if ( y - .2f >= 3f )
				targetZoom = cam.transform.localPosition - new Vector3( 0, .2f, 0 );
		}
		// scroll down
		else if ( axis < 0f )
		{
			if ( y + .2f <= 6f )
				targetZoom = cam.transform.localPosition + new Vector3( 0, .2f, 0 );
		}

		float fdScalar = GlowEngine.RemapValue( y, 3, 6, focusDistMin, focusDistMax );
		targetDOF.Set( fdScalar, fdScalar, fdScalar );
		targetLookAt.y = 26.87f;
		//30.58
		targetLookAt.x = GlowEngine.RemapValue( targetZoom.y, .2f, 6f, 50f, 55f );
	}

	public void MoveTo( Vector3 pos, float speed = .35f )
	{
		if ( pos == Vector3.zero )
			return;
		pos.y = 0;
		targetPos = pos;
		smoothSpeed = speed;
	}

	void HandleDragging()
	{
		if ( Input.GetMouseButtonDown( 0 ) )
		{
			dragOrigin = Input.mousePosition;
			return;
		}

		if ( !Input.GetMouseButton( 0 ) || dClickCount > 1 )
		{
			dragging = false;
			return;
		}

		dragging = true;
		//Vector3 pos = cam.ScreenToViewportPoint( Input.mousePosition - dragOrigin );
		Vector3 direction = Input.mousePosition - dragOrigin;
		direction = Vector3.Normalize( direction );

		Vector3 move = new Vector3( direction.x * .1f, 0, direction.y * .2f );
		transform.Translate( -move, Space.Self );
		targetPos = transform.position;
		dragOrigin = Input.mousePosition;

	}

	void HandleMove()
	{
		int mask1 = 1 << 2;
		int mask2 = 1 << 9;

		//mask = ~mask;

		//single click
		if ( Input.GetMouseButtonDown( 0 ) )
		{
			dClickCount++;
			dClickTimer = doubleClickSpeed;
			Ray ray = cam.ScreenPointToRay( Input.mousePosition );
			RaycastHit hit;
			if ( Physics.Raycast( ray, out hit, Mathf.Infinity, mask2 ) )
			{
				if ( hit.collider.name == "hit Plane" )
				{
					moveStart = hit.point;
				}
			}

			//double click
			if ( dClickCount > 1 && Physics.Raycast( ray, out hit, Mathf.Infinity, ~( mask1 | mask2 ) ) )
			{
				//Debug.Log( "click::" + hit.collider.name );

				//cam.transform.LookAt( transform );

				MoveTo( hit.collider.transform.parent.GetComponent<Tile>().centerPosition, .2f );
			}
		}

		//dragging
		/*
		if ( dClickCount <= 1 && Input.GetMouseButton( 0 ) )
		{
			Ray ray = cam.ScreenPointToRay( Input.mousePosition );
			RaycastHit hit;
			if ( Physics.Raycast( ray, out hit, Mathf.Infinity, mask2 ) )
			{
				if ( Input.GetMouseButtonDown( 0 ) )
				{
					dragOrigin = Input.mousePosition;
					return;
				}

				dragging = true;
				//Debug.Log( "dragging::" + hit.point );

				float d = Vector3.Distance( moveStart, hit.point );
				float delta = GlowEngine.RemapValue( d, 0, 3, 0, moveDragSpeed );

				Vector3 p = hit.point;
				Vector3 direction = moveStart - p;
				direction = Vector3.Normalize( direction );

				//MoveTo( transform.position + ( delta * direction ) );
				Vector3 pos = cam.ScreenToViewportPoint( Input.mousePosition - dragOrigin );
				direction = Input.mousePosition - dragOrigin;
				direction = Vector3.Normalize( direction );

				Vector3 move = new Vector3( direction.x * .2f, 0, direction.y * .2f );
				//if ( pos != lastPos )
				transform.Translate( -move, Space.Self );
				//else
				//	dragOrigin = Input.mousePosition;

				lastPos = pos;
				//moveStart = hit.point;
			}
			else
				dragging = false;
		}*/

		dClickTimer -= Time.deltaTime;
		if ( dClickTimer <= 0 )
			dClickCount = 0;
	}

	void HandleRotation()
	{
		if ( Input.GetMouseButtonDown( 1 ) )
		{
			dragStart = Input.mousePosition;
		}

		if ( Input.GetMouseButton( 1 ) )
		{
			float d = Vector2.Distance( dragStart, Input.mousePosition );
			float delta = GlowEngine.RemapValue( d, 0, 50, 0, rotateSpeed );

			if ( Input.mousePosition.x < dragStart.x )
				rotateAmount = -delta;
			else if ( Input.mousePosition.x > dragStart.x )
				rotateAmount = delta;
			else
				rotateAmount = 0;

			transform.DORotate( new Vector3( 0, rotateAmount, 0 ), rotateDuration, RotateMode.WorldAxisAdd );
			dragStart = Input.mousePosition;
		}
	}

	public CamState GetState()
	{
		return new CamState()
		{
			position = transform.position,
			YRotation = transform.rotation.eulerAngles.y
		};
	}
}
