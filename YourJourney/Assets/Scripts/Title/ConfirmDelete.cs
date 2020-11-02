using System;
using System.IO;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDelete : MonoBehaviour
{
	public CanvasGroup canvasGroup;
	public Text gameName, filePath;

	RectTransform rect;
	Vector2 ap;
	Vector3 sp;
	StateItem state;
	Action onYes, onNo;

	private void Awake()
	{
		rect = canvasGroup.gameObject.GetComponent<RectTransform>();
		ap = rect.anchoredPosition;
		sp = canvasGroup.gameObject.transform.position;
	}

	public void Show( StateItem s, Action yes, Action no )
	{
		rect = canvasGroup.gameObject.GetComponent<RectTransform>();
		ap = rect.anchoredPosition;
		sp = canvasGroup.gameObject.transform.position;

		onYes = yes;
		onNo = no;
		state = s;
		gameName.text = state.gameName;
		filePath.text = state.fullSavePath;

		canvasGroup.alpha = 0;
		canvasGroup.gameObject.SetActive( true );
		canvasGroup.DOFade( 1, .5f );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		canvasGroup.gameObject.transform.DOMoveY( sp.y, .75f );
	}

	public void OnYes()
	{
		FileInfo fi = new FileInfo( state.fullSavePath );
		fi.Delete();
		onYes?.Invoke();

		canvasGroup.DOFade( 0, .25f ).OnComplete( () =>
		{
			canvasGroup.gameObject.SetActive( false );
		} );
	}

	public void OnNo()
	{
		onNo?.Invoke();

		canvasGroup.DOFade( 0, .25f ).OnComplete( () =>
		{
			canvasGroup.gameObject.SetActive( false );
		} );
	}
}
