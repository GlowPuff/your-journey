﻿using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using static LanguageManager;

public class DamagePanel : MonoBehaviour
{
	public Text damageText, fearText;
	public TextMeshProUGUI mainText, dummy;
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

	private void CalculatePanelPosition()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		gameObject.SetActive(false);
		sp = transform.position;
		ap = rect.anchoredPosition;
	}

	void Awake()
	{
		CalculatePanelPosition();
		root = transform.parent;
		mainText.alignment = TextAlignmentOptions.Top; //We set this here instead of the editor to make it easier to see mainText and dummy are lined up with each other in the editor
		dummy.alignment = TextAlignmentOptions.Top;
	}

	public void ShowCombatCounter( Monster m, Action action = null )
	{
		CalculatePanelPosition();
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
		//Debug.Log("ShowCombatCounter: activationsId=" + m.activationsId + " activationsObserver.Count=" + (activationsObserver == null ? "null" : activationsObserver.Count.ToString()));
		if (m.activationsId >= 0 && activationsObserver != null && activationsObserver.Count > 0)
        {
			activationItems = activationsObserver.Where(a => a.id == m.activationsId ).First().activations;
        }

		int groupIndex = 0;
		MonsterActivationItem item = null;
		//Debug.Log("activationItems.Count=" + (activationItems == null ? "null" : activationItems.Count.ToString()));
		if(activationItems != null && activationItems.Count > 0)
        {
			groupIndex = m.ActiveMonsterCount - 1; //subtract one to make it work as an array index
			if (groupIndex < 0) { groupIndex = 0; }
			if (groupIndex > 2) { groupIndex = 2; }
			List<MonsterActivationItem> validItems;

			for (; groupIndex >= 0; groupIndex--)
			{
				validItems = activationItems.Where(a => a.valid[groupIndex]).ToList();
				//Debug.Log("validItems.Count=" + (validItems == null ? "null" : validItems.Count.ToString()));
				if (validItems.Count > 0)
				{
					int randomIndex = UnityEngine.Random.Range(0, validItems.Count);
					//Debug.Log("randomIndex=" + randomIndex);
					item = validItems[randomIndex];
					break;
				}
			}
		}

		if (item != null)
		{
			//Debug.Log("item damage=" + item.damage.ToString() + " fear=" + item.fear.ToString() + " negate=" + ((Ability)item.negate));
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

		//sNegatedBy = AbilityUtility.ColoredText(negatedBy, 30) + "  " + negatedBy.ToString() + " negates.";
		sNegatedBy = Translate("damage.text.Negates",
			new List<string> {
				AbilityUtility.ColoredText(negatedBy, 30) + " " +
				Translate("stat." + negatedBy.ToString(), negatedBy.ToString()) 
			}
		);

		SetText(sAttack + "\r\n\r\n" + sNegatedBy + (sEffect == "" ? "" : "\r\n\r\n" + sEffect));

        rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		group.DOFade( 1, .5f );
	}

	public void ShowShadowFear( Action action )
	{
		CalculatePanelPosition();
		FindObjectOfType<TileManager>().ToggleInput( true );

		damageIcon.SetActive( false );
		fearIcon.SetActive( true );

		fearText.text = FindObjectOfType<Engine>().scenario.shadowFear.ToString();

		overlay.alpha = 0;
		overlay.gameObject.SetActive( true );
		overlay.DOFade( 1, .5f );

		gameObject.SetActive( true );
		buttonAction = action;

		//SetText( "A menacing Darkness spreads across the land, overwhelming the heroes.\r\n\r\nIf a Hero is on a Space with a Darkness Icon or Token, suffer Fear.\r\n\r\n" +
		//	AbilityUtility.ColoredText(Ability.Spirit, 30) + " Spirit negates." );

		SetText(
			Translate("darkness.text.Flavor") + "\r\n\r\n" +
			Translate("darkness.text.Fear") + "\r\n\r\n" +
			Translate("darkness.text.Negates",
				new List<string> {
					AbilityUtility.ColoredText(Ability.Spirit, 30) + " " +
					Translate("stat." + Ability.Spirit.ToString(), Ability.Spirit.ToString())
				}
			)
		);

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		transform.DOMoveY( sp.y, .75f );

		group.DOFade( 1, .5f );
	}

	public void ShowFinalStand( int amount, FinalStand finalStand, Action<bool> action )
	{
		CalculatePanelPosition();
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

		//string abilityTest = "\r\n\r\nTest " +
		//	AbilityUtility.ColoredText((Ability)test, 30) + " " +
		//	((Ability)test).ToString() + "; " + amount + ".";

		string abilityTest = "\r\n\r\n" +
			Translate("stand.text.Test",
				new List<string> {
					AbilityUtility.ColoredText((Ability)test, 30) + " " +
					Translate("stat." + ((Ability)test).ToString(), ((Ability)test).ToString()),
					amount.ToString()
				}
			);


		//Might, Agility, Wisdom, Spirit, Wit
		if ( test == 0 )
		{
			SetText(Translate("stand.flavor.Might") + abilityTest);
		}
		else if ( test == 1 )
		{
			SetText(Translate("stand.flavor.Agility") + abilityTest);
		}
		else if ( test == 2 )
		{
			SetText(Translate("stand.flavor.Wisdom") + abilityTest);
		}
		else if ( test == 3 )
		{
			SetText(Translate("stand.flavor.Spirit") + abilityTest);
		}
		else if ( test == 4 )
		{
			SetText(Translate("stand.flavor.Wit") + abilityTest);
		}

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

		float preferredHeight = dummy.preferredHeight; //Dummy text (which must be active) is used to find the correct preferredHeight so it can then be set on the mainText which is in a scroll view viewport
		dummy.text = ""; //After we have the height we clear dummy.text so it doesn't show up anymore

		var dialogHeight = Math.Min(525, 30 + preferredHeight + 30);

		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dialogHeight);
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
		string t = fStand == FinalStand.Damage 
			? "<font=\"Icon\">D</font> " + Translate("damage." + FinalStand.Damage, FinalStand.Damage.ToString())
			: "<font=\"Icon\">F</font> " + Translate("damage." + FinalStand.Fear, FinalStand.Fear.ToString());
		var tb = FindObjectOfType<InteractionManager>().GetNewTextPanel();
		tb.ShowOkContinue( 
			//$"Discard all facedown {t} cards and gain 1 inspiration.", 
			Translate("stand.text.Success", new List<string> { t }),
			ButtonIcon.Continue, () =>
		{
			standAction( true );
		} );
	}

	public void OnFail()
	{
		Hide();
		var tb = FindObjectOfType<InteractionManager>().GetNewTextPanel();
		tb.ShowOkContinue( 
			//"Your Hero has fallen! Remove your figure from the board. If any Heroes remain, complete the mission by the next Shadow Phase or fail.",
			Translate("stand.text.Failure"),
			ButtonIcon.Continue, () =>
		{
			standAction( false );
		} );
	}
}
