using UnityEngine;
using DG.Tweening;

public class Eye : MonoBehaviour
{
	public Transform eyeTransform;

	float timer, threshold = 3;

	void Update()
	{
		timer += Time.deltaTime;
		if ( timer >= threshold )
		{
			threshold = Random.Range( 1, 4 );
			timer = 0;
			eyeTransform.DOLocalMoveX( Random.Range( -20, 20 ), .25f ).SetEase( Ease.InOutQuad );
			eyeTransform.DOLocalMoveY( Random.Range( -4.75f, 10 ), .25f ).SetEase( Ease.InOutQuad );
		}
	}
}
