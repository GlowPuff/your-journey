using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class StatTestPanel : MonoBehaviour
{
	public Text mainText, abilityText, counterText;
	public Image abilityIcon;
	public GameObject btn1, btn2, continueBtn;
	public CanvasGroup overlay;
	public GameObject progressRoot;
	public Sprite[] icons;

	CanvasGroup group;

	RectTransform rect;
	Vector3 sp;
	Vector2 ap;
	Action<InteractionResult> buttonActions;
	Transform root;
	int value;
	StatTestInteraction statTestInteraction;

	private void Awake()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		gameObject.SetActive( false );
		sp = transform.position;
		ap = rect.anchoredPosition;
		root = transform.parent;
	}

	public void Show( StatTestInteraction testInteraction, Action<InteractionResult> actions )
	{
		FindObjectOfType<TileManager>().ToggleInput( true );

		btn1.SetActive( testInteraction.passFail || !testInteraction.isCumulative );
		btn2.SetActive( testInteraction.passFail || !testInteraction.isCumulative );
		progressRoot.SetActive( !testInteraction.passFail && testInteraction.isCumulative );
		continueBtn.SetActive( false );

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		buttonActions = actions;

		if ( testInteraction.isCumulative && !testInteraction.passFail )
			abilityText.text = "Test: " + testInteraction.testAttribute.ToString();
		else
			abilityText.text = "Success: " + testInteraction.testAttribute.ToString() + " " + testInteraction.successValue;

		//if it's cumulative (and not simple pass/fail) and already started, show progress text
		if ( ( testInteraction.isCumulative && !testInteraction.passFail ) && testInteraction.accumulatedValue >= 0 )
			SetText( testInteraction.progressBookData.pages[0] );
		else//otherwise show normal event text
			SetText( testInteraction.eventBookData.pages[0] );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		abilityIcon.sprite = icons[(int)testInteraction.testAttribute];

		//acc value starts at -1, so set it to minimum of 0 to show the event has started
		testInteraction.accumulatedValue = Math.Max( 0, testInteraction.accumulatedValue );

		counterText.text = value.ToString();

		statTestInteraction = testInteraction;

		group.DOFade( 1, .5f );
	}

	public void ShowCombatCounter( Monster m, Action<InteractionResult> actions = null )
	{
		FindObjectOfType<TileManager>().ToggleInput( true );

		btn1.SetActive( false );
		btn2.SetActive( false );
		progressRoot.SetActive( false );
		continueBtn.SetActive( true );

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		buttonActions = actions;

		abilityText.text = m.negatedBy.ToString() + " negates.";

		SetText( $"A {m.dataName} attacks!" );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		abilityIcon.sprite = icons[(int)m.negatedBy];
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
		TextGenerator textGen = new TextGenerator();
		TextGenerationSettings generationSettings = mainText.GetGenerationSettings( mainText.rectTransform.rect.size );
		float height = textGen.GetPreferredHeight( t, generationSettings );

		rect.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, height + 80 + 80 );
	}

	public void OnAdd()
	{
		value++;
		counterText.text = value.ToString();
	}

	public void OnMinus()
	{
		value = Math.Max( 0, value - 1 );
		counterText.text = value.ToString();
	}

	public void OnSubmit()
	{
		//use btn4 = true to signify this as a cumulative result
		buttonActions?.Invoke( new InteractionResult() { btn4 = statTestInteraction.isCumulative, value = value } );
		Hide();
	}

	public void OnSuccess()
	{
		buttonActions?.Invoke( new InteractionResult() { btn4 = statTestInteraction.isCumulative, success = true, value = value } );
		Hide();
	}

	public void OnFail()
	{
		buttonActions?.Invoke( new InteractionResult() { btn4 = statTestInteraction.isCumulative, success = false, value = value } );
		Hide();
	}

	public void OnContinue()
	{
		buttonActions?.Invoke( new InteractionResult() { } );
		Hide();
	}
}
