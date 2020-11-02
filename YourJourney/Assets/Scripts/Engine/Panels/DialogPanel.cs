using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanel : MonoBehaviour
{
	public Text mainText, btn1Text, btn2Text, btn3Text, dummy;
	public Button btn1, btn2, btn3, cancelBtn;
	public CanvasGroup overlay;
	public RectTransform content;

	CanvasGroup group;

	RectTransform rect;
	Vector3 sp;
	Vector2 ap;
	Action<InteractionResult> buttonActions;
	Transform root;

	DialogInteraction dialogInteraction;

	private void Awake()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		//gameObject.SetActive( false );
		sp = transform.position;
		ap = rect.anchoredPosition;
		root = transform.parent;
	}

	public void Show( DialogInteraction di, Action<InteractionResult> actions = null )
	{
		gameObject.SetActive( true );
		FindObjectOfType<TileManager>().ToggleInput( true );

		btn1.gameObject.SetActive( !string.IsNullOrEmpty( di.choice1 ) );
		btn2.gameObject.SetActive( !string.IsNullOrEmpty( di.choice2 ) );
		btn3.gameObject.SetActive( !string.IsNullOrEmpty( di.choice3 ) );
		cancelBtn.gameObject.SetActive( true );

		if ( !btn1.gameObject.activeInHierarchy )
			di.c1Used = true;
		if ( !btn2.gameObject.activeInHierarchy )
			di.c2Used = true;
		if ( !btn3.gameObject.activeInHierarchy )
			di.c3Used = true;

		btn1.interactable = !di.c1Used;
		btn2.interactable = !di.c2Used;
		btn3.interactable = !di.c3Used;

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		group.alpha = 0;
		btn1Text.text = di.choice1;
		btn2Text.text = di.choice2;
		btn3Text.text = di.choice3;
		buttonActions = actions;

		if ( !di.isDone )
			SetText( di.eventBookData.pages[0] );
		else
			SetText( di.persistentText );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		group.DOFade( 1, .5f );

		dialogInteraction = di;
	}

	void SetText( string t )
	{
		mainText.alignment = TextAnchor.UpperCenter;
		mainText.text = t;
		dummy.text = t;

		TextGenerator textGen = new TextGenerator();
		TextGenerationSettings generationSettings = dummy.GetGenerationSettings( dummy.rectTransform.rect.size );
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
		content.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, height + 20 );
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

	void DisableButtons()
	{
		btn1.gameObject.SetActive( false );
		btn2.gameObject.SetActive( false );
		btn3.gameObject.SetActive( false );
		cancelBtn.gameObject.SetActive( false );
	}

	public void OnBtn1()
	{
		DisableButtons();

		dialogInteraction.c1Used = true;
		if ( dialogInteraction.c1Used && dialogInteraction.c2Used && dialogInteraction.c3Used )
			dialogInteraction.isDone = true;

		buttonActions?.Invoke( new InteractionResult() { btn1 = true, removeToken = false } );
		Hide();
	}

	public void OnBtn2()
	{
		DisableButtons();

		dialogInteraction.c2Used = true;
		if ( dialogInteraction.c1Used && dialogInteraction.c2Used && dialogInteraction.c3Used )
			dialogInteraction.isDone = true;

		buttonActions?.Invoke( new InteractionResult() { btn2 = true, removeToken = false } );
		Hide();
	}

	public void OnBtn3()
	{
		DisableButtons();

		dialogInteraction.c3Used = true;
		if ( dialogInteraction.c1Used && dialogInteraction.c2Used && dialogInteraction.c3Used )
			dialogInteraction.isDone = true;

		buttonActions?.Invoke( new InteractionResult() { btn3 = true, removeToken = false } );
		Hide();
	}

	public void OnCancel()
	{
		DisableButtons();

		buttonActions?.Invoke( new InteractionResult() { canceled = true, removeToken = false } );
		Hide();
	}
}
