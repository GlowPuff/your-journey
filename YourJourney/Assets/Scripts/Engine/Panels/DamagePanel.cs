using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class DamagePanel : MonoBehaviour
{
	public Text mainText, abilityText, damageText, fearText;
	public Image abilityIcon;
	public CanvasGroup overlay;
	public GameObject damageIcon, fearIcon, damageRoot, finalstandRoot;
	public Sprite[] icons;

	CanvasGroup group;
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
	}

	public void ShowCombatCounter( Monster m, Action action = null )
	{
		done = false;
		FindObjectOfType<TileManager>().ToggleInput( true );

		damageIcon.SetActive( true );
		fearIcon.SetActive( true );

		Ability negatedBy;
		string sNegatedBy;
		string sFear;
		string sDamage;
		string sAttack;
		string sEffect;

		//First check if there are Monster Activations available...
		ObservableCollection<MonsterActivations> activationsObserver = Engine.currentScenario.activationsObserver;
		ObservableCollection<MonsterActivationItem> activationItems = new ObservableCollection<MonsterActivationItem>();
		Debug.Log("ShowCombatCounter: activationsId=" + m.activationsId + " activationsObserver.Count=" + (activationsObserver == null ? "null" : activationsObserver.Count.ToString()));
		if (m.activationsId >= 0 && activationsObserver != null && activationsObserver.Count > 0)
        {
			activationItems = activationsObserver.Where(a => a.id == m.activationsId ).First().activations;
        }

		int groupIndex = 0;
		MonsterActivationItem item = null;
		Debug.Log("activationItems.Count=" + (activationItems == null ? "null" : activationItems.Count.ToString()));
		if(activationItems != null && activationItems.Count > 0)
        {
			groupIndex = m.ActiveMonsterCount - 1; //subtract one to make it work as an array index
			if (groupIndex < 0) { groupIndex = 0; }
			if (groupIndex > 2) { groupIndex = 2; }
			List<MonsterActivationItem> validItems;

			for (; groupIndex >= 0; groupIndex--)
			{
				validItems = activationItems.Where(a => a.valid[groupIndex]).ToList();
				Debug.Log("validItems.Count=" + (validItems == null ? "null" : validItems.Count.ToString()));
				if (validItems.Count > 0)
				{
					int randomIndex = UnityEngine.Random.Range(0, validItems.Count);
					Debug.Log("randomIndex=" + randomIndex);
					item = validItems[randomIndex];
					break;
				}
			}
		}

		if (item != null)
		{
			Debug.Log("item damage=" + item.damage.ToString() + " fear=" + item.fear.ToString() + " negate=" + ((Ability)item.negate));
			sDamage = item.damage[groupIndex].ToString();
			sFear = item.fear[groupIndex].ToString();
			negatedBy = (Ability)item.negate;
			sAttack = item.text;
			sEffect = item.effect;
		}
		else
		{
			//Only apply Default damage if there are no Monster Activations
			Tuple<int, int> damage = m.CalculateDamage();
			sFear = damage.Item1.ToString();
			sDamage = damage.Item2.ToString();
			negatedBy = m.negatedBy;
			negatedBy = (Ability)GlowEngine.GenerateRandomNumbers(6)[0]; //Randomize the ability instead of taking it from the monster (which is always Might right now)
			sAttack = $"A {m.dataName} attacks!";
			sEffect = "";
		}

		fearText.text = sFear;
		damageText.text = sDamage;

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		buttonAction = action;

		abilityText.text = "";
		sNegatedBy = AbilityUtility.ColoredText(negatedBy, 42) + "  " + negatedBy.ToString() + " negates.";

		SetText(sAttack + "\r\n\r\n" + sNegatedBy + (sEffect == "" ? "" : "\r\n\r\n" + sEffect));

        rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

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

		SetText( "A menacing Darkness spreads across the land, overwhelming the heroes.\r\n\r\nIf a Hero is on a Space with a Darkness Icon or Token, suffer Fear.\r\n\r\n" +
			AbilityUtility.ColoredText(Ability.Spirit, 42) + " Spirit negates." );

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
			SetText( "Strive for life with all your might!" );
		}
		else if ( test == 1 )
		{
			SetText( "Skillful maneuvering can lead to escape!" );
		}
		else if ( test == 2 )
		{
			SetText( "Put your knowledge of healing and survival to the test!" );
		}
		else if ( test == 3 )
		{
			SetText( "You can still survive, fight the fear!" );
		}
		else if ( test == 4 )
		{
			SetText( "Quick thinking can save you!" );
		}
		abilityText.text = "Test " +
			AbilityUtility.ColoredText((Ability)test, 42) + " " +
			((Ability)test).ToString() + "; " + amount + ".";

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
		//string t = fStand == FinalStand.Damage ? "DAMAGE" : "FEAR";
		string t = fStand == FinalStand.Damage ? "<b>D</b>" : "<b>F</b>";
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
