using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StoryBox : MonoBehaviour
{
	public CanvasGroup canvasGroup;
	public Text storyText;
	public RectTransform itemContainer;
	//public Image fader;

	RectTransform rect;
	Vector2 ap;
	Vector3 sp;
	Action action;

	private void Awake()
	{
		rect = canvasGroup.gameObject.GetComponent<RectTransform>();
		ap = itemContainer.anchoredPosition;
		sp = canvasGroup.gameObject.transform.position;
	}

	public void Show( string t, Action a )
	{
		rect = canvasGroup.gameObject.GetComponent<RectTransform>();
		ap = itemContainer.anchoredPosition;
		sp = canvasGroup.gameObject.transform.position;

		gameObject.SetActive( true );

		//if ( ptype == ProjectType.Standalone )
		//	fader.color = new Color( 0, 0, 0, 220f / 255f );
		//else
		//	fader.color = new Color( 0, 0, 0, 1 );

		action = a;
		canvasGroup.alpha = 0;
		canvasGroup.gameObject.SetActive( true );
		canvasGroup.DOFade( 1, .5f );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		canvasGroup.gameObject.transform.DOMoveY( sp.y, .75f );

		SetText( t );
	}

	void SetText( string t )
	{
		storyText.text = t;

		TextGenerator textGen = new TextGenerator();
		TextGenerationSettings generationSettings = storyText.GetGenerationSettings( storyText.rectTransform.rect.size );

		float height = textGen.GetPreferredHeight( t, generationSettings );

		itemContainer.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, height + 20 );
	}


	public void OnOK()
	{
		action?.Invoke();

		canvasGroup.DOFade( 0, .25f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			canvasGroup.gameObject.SetActive( false );
		} );
	}
}
