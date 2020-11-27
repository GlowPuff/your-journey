using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class SettingsDialog : MonoBehaviour
{
	public CanvasGroup settingsCanvasGroup;
	public Toggle musicToggle, vignetteToggle, colorToggle;
	public PostProcessVolume volume;
	public AudioSource musicSource;
	public Text buttonText;

	RectTransform rect;
	Vector2 ap;
	Vector3 sp;
	Action quitAction;

	void Awake()
	{
		rect = settingsCanvasGroup.gameObject.GetComponent<RectTransform>();
		ap = rect.anchoredPosition;
		sp = settingsCanvasGroup.gameObject.transform.position;
	}

	public void Show( string bText, Action action = null )
	{
		quitAction = action;
		buttonText.text = bText;
		settingsCanvasGroup.alpha = 0;
		settingsCanvasGroup.gameObject.SetActive( true );
		settingsCanvasGroup.DOFade( 1, .5f );

		rect.anchoredPosition = new Vector2( 0, ap.y - 25 );
		settingsCanvasGroup.gameObject.transform.DOMoveY( sp.y, .75f );

		//populate checkboxes
		var settings = Bootstrap.LoadSettings();
		musicToggle.isOn = settings.Item1 == 1;
		vignetteToggle.isOn = settings.Item2 == 1;
		colorToggle.isOn = settings.Item3 == 1;
	}

	public void OnClose()
	{
		//save settings
		Bootstrap.SaveSettings( new System.Tuple<int, int, int>(
			musicToggle.isOn ? 1 : 0,
			vignetteToggle.isOn ? 1 : 0,
			colorToggle.isOn ? 1 : 0 ) );

		settingsCanvasGroup.DOFade( 0, .25f ).OnComplete( () =>
		{
			settingsCanvasGroup.gameObject.SetActive( false );
		} );
	}

	public void OnQuit()
	{
		//save settings
		Bootstrap.SaveSettings( new Tuple<int, int, int>(
			musicToggle.isOn ? 1 : 0,
			vignetteToggle.isOn ? 1 : 0,
			colorToggle.isOn ? 1 : 0 ) );

		if ( quitAction != null )
		{
			settingsCanvasGroup.DOFade( 0, .25f ).OnComplete( () =>
			{
				settingsCanvasGroup.gameObject.SetActive( false );
				quitAction();
			} );
		}
		else
			Application.Quit();
	}

	public void OnMusic()
	{
		musicSource.enabled = musicToggle.isOn;
		//only start music if it wasn't already playing
		if ( musicSource.enabled )
			musicSource.Play();
	}

	public void OnVignette()
	{
		Vignette v;
		if ( volume.profile.TryGetSettings( out v ) )
			v.active = vignetteToggle.isOn;
	}

	public void OnColor()
	{
		ColorGrading cg;
		if ( volume.profile.TryGetSettings( out cg ) )
			cg.active = colorToggle.isOn;
	}
}
