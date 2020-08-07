using UnityEngine;
using DG.Tweening;

public class MistController : MonoBehaviour
{
	public Transform[] mists;

	void Start()
	{
		InvokeRepeating( "DoAnim", 0, 30 );
		InvokeRepeating( "DoAnim1", 0, 35 );
		InvokeRepeating( "DoAnim2", 0, 35 );
	}

	void DoAnim()
	{
		mists[0].DOLocalRotate( new Vector3( 0, 0, 360 ), 30f, RotateMode.LocalAxisAdd ).SetEase( Ease.Linear );
	}

	void DoAnim1()
	{
		mists[1].DOLocalRotate( new Vector3( 0, 0, 360 ), 35f, RotateMode.LocalAxisAdd ).SetEase( Ease.Linear );
	}

	void DoAnim2()
	{
		mists[2].DOLocalRotate( new Vector3( 0, 0, 360 ), 35f, RotateMode.LocalAxisAdd ).SetEase( Ease.Linear );
	}
}
