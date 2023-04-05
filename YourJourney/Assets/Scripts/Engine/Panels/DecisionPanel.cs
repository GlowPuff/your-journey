﻿using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DecisionPanel : MonoBehaviour
{
	public TextMeshProUGUI mainText, btn1Text, btn2Text, btn3Text, btn4Text, dummy;
	public GameObject btn1, btn2, btn3, btn4;
	public CanvasGroup overlay;

	CanvasGroup group;

	RectTransform rect;
	Vector3 sp;
	Vector2 ap;
	Action<InteractionResult> buttonActions;
	Transform root;

	private void CalculatePanelPosition()
	{
		rect = GetComponent<RectTransform>();
		group = GetComponent<CanvasGroup>();
		gameObject.SetActive(false);
		sp = transform.position;
		ap = rect.anchoredPosition;
	}

	private void Awake()
	{
		CalculatePanelPosition();
		root = transform.parent;
		mainText.alignment = TextAlignmentOptions.Top; //We set this here instead of the editor to make it easier to see mainText and dummy are lined up with each other in the editor
		dummy.alignment = TextAlignmentOptions.Top;
	}

	public void Show( DecisionInteraction branchInteraction, Action<InteractionResult> actions = null )
	{
		CalculatePanelPosition();
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

		float preferredHeight = dummy.preferredHeight; //Dummy text (which must be active) is used to find the correct preferredHeight so it can then be set on the mainText which is in a scroll view viewport
		dummy.text = ""; //After we have the height we clear dummy.text so it doesn't show up anymore

		var dialogHeight = Math.Min( 525, 30 + preferredHeight + 30 );

		rect.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, dialogHeight);
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
