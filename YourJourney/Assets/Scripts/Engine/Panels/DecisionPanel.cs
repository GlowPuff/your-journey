using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DecisionPanel : MonoBehaviour
{
	public Text mainText, btn1Text, btn2Text, btn3Text, btn4Text, dummy;
	public GameObject btn1, btn2, btn3, btn4;
	public CanvasGroup overlay;
	public RectTransform content;

	CanvasGroup group;

	RectTransform rect;
	Vector3 sp;
	Vector2 ap;
	Action<InteractionResult> buttonActions;
	Transform root;

	private void Awake()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		gameObject.SetActive( false );
		sp = transform.position;
		ap = rect.anchoredPosition;
		root = transform.parent;
	}

	public void Show( DecisionInteraction branchInteraction, Action<InteractionResult> actions = null )
	{
		FindObjectOfType<TileManager>().ToggleInput( true );

		btn1.SetActive( true );
		btn2.SetActive( true );
		btn3.SetActive( branchInteraction.isThreeChoices );
		//btn4.SetActive( true );

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		btn1Text.text = branchInteraction.choice1;
		btn2Text.text = branchInteraction.choice2;
		btn3Text.text = branchInteraction.choice3;
		buttonActions = actions;

		SetText( branchInteraction.eventBookData.pages[0] );
		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		group.DOFade( 1, .5f );
	}

	public void Hide()
	{
		group.DOFade( 0, .25f );
		overlay.DOFade( 0, .25f ).OnComplete( () =>
		{
			FindObjectOfType<TileManager>().ToggleInput( false );
			Destroy( root.gameObject );
		} );
	}

	void SetText( string t )
	{
		mainText.text = t;
		dummy.text = t;

		TextGenerator textGen = new TextGenerator();
		TextGenerationSettings generationSettings = mainText.GetGenerationSettings( mainText.rectTransform.rect.size );
		float height = textGen.GetPreferredHeight( t, generationSettings );

		//Debug.Log( height );//lineheight=35
		//Regex rx = new Regex( @"\r\n" );
		//MatchCollection matches = rx.Matches( t );
		//height -= matches.Count * 35;

		var windowH = Math.Min( 525, height + 80 );

		if ( height + 80 > 525 )
			mainText.alignment = TextAnchor.UpperCenter;
		else
			mainText.alignment = TextAnchor.MiddleCenter;

		rect.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, windowH );
		content.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, height + 0 );
	}

	void DisableButtons()
	{
		btn1.SetActive( false );
		btn2.SetActive( false );
		btn3.SetActive( false );
		btn4.SetActive( false );
	}

	public void OnBtn1()
	{
		DisableButtons();

		buttonActions?.Invoke( new InteractionResult() { btn1 = true } );
		Hide();
	}

	public void OnBtn2()
	{
		DisableButtons();

		buttonActions?.Invoke( new InteractionResult() { btn2 = true } );
		Hide();
	}

	public void OnBtn3()
	{
		DisableButtons();

		buttonActions?.Invoke( new InteractionResult() { btn3 = true } );
		Hide();
	}

	public void OnBtn4()
	{
		DisableButtons();

		buttonActions?.Invoke( new InteractionResult() { btn4 = true } );
		Hide();
	}
}
