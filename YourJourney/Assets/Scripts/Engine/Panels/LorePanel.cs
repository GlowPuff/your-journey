using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LorePanel : MonoBehaviour
{
	public Text msgText;
	public Transform panelTF;
	public Canvas canvas;

	bool busy = false;
	int acc = 0;
	float scalar;
	private void Start()
	{
		scalar = canvas.scaleFactor;
	}

	public void AddLore( int amount )
	{
		if ( amount == 0 )
			return;

		scalar = canvas.scaleFactor;
		Bootstrap.loreCount += amount;
		if ( busy )//accumulate if already showing
			acc += amount;
		else
			acc = amount;
		msgText.text = "You Have Earned\r\n" + acc + " Lore.";

		if ( !busy )
		{
			busy = true;

			var s = DOTween.Sequence();
			s.Append( panelTF.DOMoveX( 1920f * scalar, .25f ) );
			s.Append( panelTF.DOMoveX( ( 1920f * scalar ) + ( 405f * scalar ), .25f ).SetDelay( 4 ) );
			s.OnComplete( () => busy = false );
			s.Play();
		}
	}
}
