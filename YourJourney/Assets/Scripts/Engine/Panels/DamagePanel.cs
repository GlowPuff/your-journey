using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Text.RegularExpressions;

public class DamagePanel : MonoBehaviour
{
	public Text mainText, abilityText, damageText, fearText;
	public Image abilityIcon;
	public CanvasGroup overlay;
	public GameObject damageIcon, fearIcon, damageRoot, finalstandRoot;
	public Sprite[] icons;

	CanvasGroup group;
	Color[] testColors;
	string[] testChar;
	RectTransform rect;
	Vector3 sp;
	Vector2 ap;
	Action buttonAction;
	Action<bool> standAction;
	Transform root;
	bool done = false;
	FinalStand fStand;

	void Awake()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		gameObject.SetActive( false );
		sp = transform.position;
		ap = rect.anchoredPosition;
		root = transform.parent;
		testColors = new Color[6];
		//mit/agi/wis/spi/wit
		testColors[0] = Color.red;
		testColors[1] = Color.green;
		testColors[2] = Color.HSVToRGB( 300f / 360f, 1, .5f );
		testColors[3] = Color.HSVToRGB( 207f / 360f, .61f, .71f );
		testColors[4] = Color.yellow;
		testColors[5] = Color.white;

		// Might, Agility, Wisdom, Spirit, Wit, Wild
		testChar = new string[6];
		testChar[0] = "M";
		testChar[1] = "A";
		testChar[2] = "Z";
		testChar[3] = "S";
		testChar[4] = "W";
		testChar[5] = "X";
	}

	public void ShowCombatCounter( Monster m, Action action = null )
	{
		done = false;
		FindObjectOfType<TileManager>().ToggleInput( true );

		damageIcon.SetActive( true );
		fearIcon.SetActive( true );

		Tuple<int, int> damage = m.CalculateDamage();
		fearText.text = damage.Item1.ToString();
		damageText.text = damage.Item2.ToString();

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		buttonAction = action;

		Ability negatedBy = m.negatedBy;
		negatedBy = (Ability)GlowEngine.GenerateRandomNumbers(6)[0]; //Randomize the ability instead of taking it from the monster (which is always Might right now)

		Debug.Log("Color: " + testColors[(int)negatedBy].ToString());
		abilityText.text = AbilityUtility.ColoredText(negatedBy, 42) + "  " + negatedBy.ToString() + " negates.";

		SetText( $"A {m.dataName} attacks!" );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		//This is still in the UI panel but removed the code in favor of AbilityUtility.ColoredText
		//abilityIcon.gameObject.SetActive( true );
		//abilityIcon.sprite = icons[(int)negatedBy];
		//abilityIcon.color = testColors[(int)negatedBy];

		group.DOFade( 1, .5f );
	}

	public void ShowShadowFear( Action action )
	{
		FindObjectOfType<TileManager>().ToggleInput( true );

		damageIcon.SetActive( false );
		fearIcon.SetActive( true );

		fearText.text = FindObjectOfType<Engine>().scenario.shadowFear.ToString();

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		buttonAction = action;

		abilityText.text = "";

		SetText( "A menacing Darkness spreads across the land, overwhelming the heroes.\r\n\r\nIf a Hero is on a Space with a Darkness Icon or Token, suffer Fear.\r\n\r\nSpirit negates." );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		abilityIcon.gameObject.SetActive( false );
		group.DOFade( 1, .5f );
	}

	public void ShowFinalStand( int amount, FinalStand finalStand, Action<bool> action )
	{
		finalstandRoot.SetActive( true );
		damageRoot.SetActive( false );

		FindObjectOfType<TileManager>().ToggleInput( true );

		damageIcon.SetActive( false );
		fearIcon.SetActive( false );

		standAction = action;
		fStand = finalStand;

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		buttonAction = null;

		int test;
		if ( finalStand == FinalStand.Damage )
			test = UnityEngine.Random.Range( 0, 2 );
		else
			test = UnityEngine.Random.Range( 2, 5 );
		//Might, Agility, Wisdom, Spirit, Wit
		if ( test == 0 )
		{
			SetText( "You can still survive, push through it!" );
			abilityText.text = "Test Might: " + amount;
		}
		else if ( test == 1 )
		{
			SetText( "Skillful maneuvering can lead to escape!" );
			abilityText.text = "Test Agility: " + amount;
		}
		else if ( test == 2 )
		{
			SetText( "Put your knowledge of healing and survival to the test!" );
			abilityText.text = "Test Wisdom: " + amount;
		}
		else if ( test == 3 )
		{
			SetText( "You can still survive, fight the fear!" );
			abilityText.text = "Test Spirit: " + amount;
		}
		else if ( test == 4 )
		{
			SetText( "Quick thinking can save you!" );
			abilityText.text = "Test Wit: " + amount;
		}

		abilityIcon.gameObject.SetActive( true );
		abilityIcon.sprite = icons[test];
		abilityIcon.color = testColors[test];

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
		TextGenerator textGen = new TextGenerator();
		TextGenerationSettings generationSettings = mainText.GetGenerationSettings( mainText.rectTransform.rect.size );
		float height = textGen.GetPreferredHeight( t, generationSettings );

		rect.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, height + 80 + 80 );
	}

	public void OnContinue()
	{
		if ( done )
			return;

		done = true;
		buttonAction?.Invoke();
		Hide();
	}

	public void OnPass()
	{
		Hide();
		string t = fStand == FinalStand.Damage ? "DAMAGE" : "FEAR";
		var tb = FindObjectOfType<InteractionManager>().GetNewTextPanel();
		tb.ShowOkContinue( $"Discard all facedown {t} cards and gain 1 inspiration.", ButtonIcon.Continue, () =>
		{
			standAction( true );
		} );
	}

	public void OnFail()
	{
		Hide();
		var tb = FindObjectOfType<InteractionManager>().GetNewTextPanel();
		tb.ShowOkContinue( "Your Hero has fallen! Remove your figure from the board. If any Heroes remain, complete the mission by the next Shadow Phase or fail.", ButtonIcon.Continue, () =>
		{
			standAction( false );
		} );
	}
}
