using UnityEngine;
using DG.Tweening;
using System;

public class ProvokeMessage : MonoBehaviour
{
	public Transform panel;
	[HideInInspector]
	public bool provokeMode { get; set; } = false;

	CanvasGroup group;
	Action callback;

	void Awake()
	{
		group = GetComponent<CanvasGroup>();
		provokeMode = false;
	}

	public void Show( Action cb = null )
	{
		FindObjectOfType<TileManager>().ToggleInput( true );

		provokeMode = true;
		callback = cb;
		group.interactable = true;
		panel.transform.localPosition = new Vector3( 0, 190 - 25, 0 );
		panel.transform.DOLocalMoveY( 190, .75f );
		group.DOFade( 1, .5f );
	}

	void Hide()
	{
		group.interactable = false;

		group.DOFade( 0, .25f ).OnComplete( () =>
		{
			FindObjectOfType<TileManager>().ToggleInput( false );
			FindObjectOfType<ProvokeButton>().DisableSpinner();
			provokeMode = false;
		} );
	}

	public void OnCancel()
	{
		Hide();
		callback?.Invoke();
	}
}
