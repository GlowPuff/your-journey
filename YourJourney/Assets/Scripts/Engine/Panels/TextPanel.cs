using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class TextPanel : MonoBehaviour
{
	public Text mainText, btn1Text, btn2Text, btnSingleText;
	public GameObject btn1, btn2;
	public GameObject buttonSingle;
	public GameObject actionIcon;
	public CanvasGroup overlay;

	CanvasGroup group;

	RectTransform rect;
	Vector3 sp;
	Vector2 ap;
	Action btnSingleAction;
	Action<InteractionResult> buttonActions;
	Transform root;

	void Awake()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		gameObject.SetActive( false );
		sp = transform.position;
		ap = rect.anchoredPosition;
		root = transform.parent;
	}

	void Show( string t, string btn1, string btn2, ButtonIcon icon = ButtonIcon.None, Action<InteractionResult> actions = null )
	{
		FindObjectOfType<TileManager>().ToggleInput( true );

		this.btn1.SetActive( true );
		this.btn2.SetActive( true );
		buttonSingle.SetActive( false );

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		btn1Text.text = btn1;
		btn2Text.text = btn2;
		buttonActions = actions;

		actionIcon.SetActive( false );
		switch ( icon )
		{
			case ButtonIcon.Action:
				actionIcon.SetActive( true );
				break;
		}
		SetText( t );
		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		group.DOFade( 1, .5f );
	}

	public void ShowCustom( string t, string btn1, string btn2, Action<InteractionResult> actions = null )
	{
		Show( t, btn1, btn2, ButtonIcon.None, actions );
	}

	public void ShowYesNo( string s, Action<InteractionResult> actions = null )
	{
		Show( s, "Yes", "No", ButtonIcon.None, actions );
	}

	public void ShowOkContinue( string s, ButtonIcon icon, Action action = null )
	{
		FindObjectOfType<TileManager>().ToggleInput( true );

		btn1.SetActive( false );
		btn2.SetActive( false );
		buttonSingle.SetActive( true );

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );
		gameObject.SetActive( true );

		btnSingleText.text = icon.ToString();
		btnSingleAction = action;
		SetText( s );
		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		group.DOFade( 1, .5f );
	}

	/// <summary>
	/// Shows a dialog when QUERYING a token
	/// </summary>
	public void ShowQueryInteraction( IInteraction it, string btnName, Action<InteractionResult> actions )
	{
		Show( it.textBookData.pages[0], "Cancel", btnName, ButtonIcon.Action, actions );
	}

	public void ShowQueryExploration( Action<InteractionResult> actions )
	{
		Show( "Explore this tile?", "Yes", "No", ButtonIcon.None, actions );
	}

	/// <summary>
	/// Shows dialog with an interaction
	/// </summary>
	public void ShowTextInteraction( IInteraction it, Action actions )
	{
		ShowOkContinue( it.eventBookData.pages[0], ButtonIcon.Continue, actions );
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

	/// <summary>
	/// JUST remove the box, don't fade overlay, don't toggle input
	/// </summary>
	public void RemoveBox()
	{
		group.DOFade( 0, .25f ).OnComplete( () =>
		{
			Destroy( root.gameObject );
		} );
	}

	void SetText( string t )
	{
		mainText.text = t;
		TextGenerator textGen = new TextGenerator();
		TextGenerationSettings generationSettings = mainText.GetGenerationSettings( mainText.rectTransform.rect.size );
		float height = textGen.GetPreferredHeight( t, generationSettings );

		rect.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, height + 80 );
	}

	public void OnBtn1()
	{
		buttonActions?.Invoke( new InteractionResult() { btn1 = true } );
		Hide();
	}

	public void OnBtn2()
	{
		buttonActions?.Invoke( new InteractionResult() { btn2 = true } );
		Hide();
	}

	public void OnBtnSingle()
	{
		btnSingleAction?.Invoke();
		Hide();
	}
}
