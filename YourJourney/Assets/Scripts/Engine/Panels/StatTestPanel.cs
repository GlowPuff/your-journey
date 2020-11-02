using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StatTestPanel : MonoBehaviour
{
	public Text mainText, abilityText, counterText, dummy;
	public Image abilityIcon;
	public GameObject btn1, btn2, continueBtn;
	public CanvasGroup overlay;
	public GameObject progressRoot;
	public Sprite[] icons;
	public RectTransform content;

	CanvasGroup group;
	Color[] testColors;
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
		testColors = new Color[5];
		//mit/agi/wis/spi/wit
		testColors[0] = Color.red;
		testColors[1] = Color.green;
		testColors[2] = Color.HSVToRGB( 300f / 360f, 1, .5f );
		testColors[3] = Color.HSVToRGB( 207f / 360f, .61f, .71f );
		testColors[4] = Color.yellow;
	}

	public void Show( StatTestInteraction testInteraction, Action<InteractionResult> actions )
	{
		FindObjectOfType<TileManager>().ToggleInput( true );

		statTestInteraction = testInteraction;

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

		abilityIcon.gameObject.SetActive( true );
		abilityIcon.sprite = icons[(int)testInteraction.testAttribute];
		abilityIcon.color = testColors[(int)testInteraction.testAttribute];

		//acc value starts at -1, so set it to minimum of 0 to show the event has started
		testInteraction.accumulatedValue = Math.Max( 0, testInteraction.accumulatedValue );

		counterText.text = value.ToString();

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
		mainText.alignment = TextAnchor.UpperCenter;
		mainText.text = t;
		dummy.text = t;
		int hmax = 525;
		if ( statTestInteraction.isCumulative && !statTestInteraction.passFail )
			hmax = 410;

		TextGenerator textGen = new TextGenerator();
		TextGenerationSettings generationSettings = mainText.GetGenerationSettings( mainText.rectTransform.rect.size );
		float height = textGen.GetPreferredHeight( t, generationSettings );

		//Debug.Log( height );//lineheight=35
		//Regex rx = new Regex( @"\r\n" );
		//MatchCollection matches = rx.Matches( t );
		//height -= matches.Count * 35;

		var windowH = Math.Min( hmax, height + 80 + 80 );

		if ( height + 80 + 80 > hmax )
			mainText.alignment = TextAnchor.UpperCenter;
		else
			mainText.alignment = TextAnchor.MiddleCenter;

		rect.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, windowH ); /*height + 80 + 80*/
		content.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, height + 80 );

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
		btn1.SetActive( false );
		btn2.SetActive( false );
		continueBtn.SetActive( false );

		//use btn4 = true to signify this as a cumulative result
		buttonActions?.Invoke( new InteractionResult() { btn4 = statTestInteraction.isCumulative, value = value } );
		Hide();
	}

	public void OnSuccess()
	{
		btn1.SetActive( false );
		btn2.SetActive( false );
		continueBtn.SetActive( false );

		int v = value;
		if ( statTestInteraction.passFail )
			v = 1;

		buttonActions?.Invoke( new InteractionResult() { btn4 = statTestInteraction.isCumulative, success = true, value = v } );
		Hide();
	}

	public void OnFail()
	{
		btn1.SetActive( false );
		btn2.SetActive( false );
		continueBtn.SetActive( false );

		int v = value;
		if ( statTestInteraction.passFail )
			v = -1;

		buttonActions?.Invoke( new InteractionResult() { btn4 = statTestInteraction.isCumulative, success = false, value = v } );
		Hide();
	}

	public void OnContinue()
	{
		btn1.SetActive( false );
		btn2.SetActive( false );
		continueBtn.SetActive( false );

		buttonActions?.Invoke( new InteractionResult() { } );
		Hide();
	}
}
