using UnityEngine;
using DG.Tweening;

public class TitleCam : MonoBehaviour
{
	public Transform button1, button2, optionButton;

	bool gotime = false;
	float yval = 0;
	float timer = 3, xtimer;
	Camera cam;

	// Start is called before the first frame update
	void Start()
	{
		cam = Camera.main;

		GlowTimer.SetTimer( 8, () =>
		{
			GetComponent<Animator>().enabled = false;
			button1.DOLocalMoveX( -700, 1 ).SetEase( Ease.InOutQuad );
			button2.DOLocalMoveX( -700, 1 ).SetEase( Ease.InOutQuad ).SetDelay( 1 );
			optionButton.DOLocalMoveX( 880, 1 ).SetEase( Ease.InOutQuad );

			gotime = true;
			InvokeRepeating( "animateCam", 0, 8 );
		} );
	}

	void animateCam()
	{
		cam.transform.DORotate( new Vector3( Random.Range( -1f, 2.5f ), 0, -1 ), 4f )
		.SetDelay( 1 )
		.SetEase( Ease.InOutQuad )
		.OnComplete( () =>
		{
			cam.transform.DORotate( new Vector3( Random.Range( -1f, 2.5f ), 0, 1 ), 4f ).SetEase( Ease.InOutQuad );
		} );
	}

	// Update is called once per frame
	void Update()
	{
		xtimer += Time.deltaTime;

		if ( gotime )
		{
			if ( xtimer >= 4 )
			{
				xtimer = 0;
				//cam.transform.DOLocalRotate( new Vector3( Random.Range( -1f, 2.5f ), 0, 0 ), 4 );
			}
			//float value = Mathf.Sin( timer * .5f );
			//value = -.4f + ( value - -1 ) * ( 0 - -.4f ) / ( 1 - -1 );

			yval = GlowEngine.SineAnimation( -.4f, 0f, .5f, timer );
			timer += Time.deltaTime;//bounce
		}

		cam.transform.position = new Vector3( 0, 3.2f + yval, -8.95f );
	}
}
