using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class DamagePanel : MonoBehaviour
{
	public Text mainText, abilityText, damageText, fearText;
	public Image abilityIcon;
	public CanvasGroup overlay;
	public GameObject damageIcon;
	public Sprite[] icons;

	CanvasGroup group;
	RectTransform rect;
	Vector3 sp;
	Vector2 ap;
	Action buttonAction;
	Transform root;

	// Start is called before the first frame update
	void Awake()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		gameObject.SetActive( false );
		sp = transform.position;
		ap = rect.anchoredPosition;
		root = transform.parent;
	}

	public void ShowCombatCounter( Monster m, Action action = null )
	{
		FindObjectOfType<TileManager>().ToggleInput( true );

		damageIcon.SetActive( true );

		Tuple<int, int> damage = m.CalculateDamage();
		fearText.text = damage.Item1.ToString();
		damageText.text = damage.Item2.ToString();

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		buttonAction = action;

		abilityText.text = m.negatedBy.ToString() + " negates.";

		SetText( $"A {m.dataName} attacks!" );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		abilityIcon.gameObject.SetActive( true );
		abilityIcon.sprite = icons[(int)m.negatedBy];
		group.DOFade( 1, .5f );
	}

	public void ShowShadowFear( Action action )
	{
		FindObjectOfType<TileManager>().ToggleInput( true );

		damageIcon.SetActive( false );

		fearText.text = FindObjectOfType<Engine>().scenario.shadowFear.ToString();

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		buttonAction = action;

		abilityText.text = "";

		SetText( "Darkness strikes fear into the hearts and minds of the heroes.\r\n\r\nIf a Hero is on a Space with a Darkness Icon or Token, suffer Fear." );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		abilityIcon.gameObject.SetActive( false );
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
		buttonAction?.Invoke();
		Hide();
	}
}
