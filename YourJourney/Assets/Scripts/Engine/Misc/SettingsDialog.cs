using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class SettingsDialog : MonoBehaviour
{
	public CanvasGroup settingsCanvasGroup;
	public Toggle musicToggle, vignetteToggle, colorToggle, fullscreenToggle;
	public PostProcessVolume volume;
	public AudioSource musicSource;
	public Text buttonText;
	public TMP_Dropdown resolutionDropdown;

	RectTransform rect;
	Vector2 ap;
	Vector3 sp;
	Action quitAction;
	Resolution[] resolutions;
	List<TMP_Dropdown.OptionData> resolutionList;

	void Awake()
	{
		CalculateDialogPosition();
	}

	public void Show( string bText, Action action = null )
	{
		CalculateDialogPosition();

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
		fullscreenToggle.isOn = settings.Item6 == 1;

		//populate resolutions dropdown
		Resolution savedResolution = new Resolution();
		savedResolution.width = settings.Item4;
		savedResolution.height = settings.Item5;
		resolutions = Screen.resolutions;
		resolutionList = new List<TMP_Dropdown.OptionData>();
		int selectedIndex = 0;
		foreach (var res in resolutions)
		{
			resolutionList.Add(new TMP_Dropdown.OptionData(res.width + "x" + res.height));
			if(res.width == savedResolution.width && res.height == savedResolution.height)
            {
				selectedIndex = resolutionList.Count - 1;
            }
		}
		resolutionDropdown.ClearOptions();
		resolutionDropdown.AddOptions(resolutionList);
		resolutionDropdown.SetValueWithoutNotify(selectedIndex);
	}

	public void OnClose()
	{
		//save settings
		Bootstrap.SaveSettings( new System.Tuple<int, int, int, int, int, int>(
			musicToggle.isOn ? 1 : 0,
			vignetteToggle.isOn ? 1 : 0,
			colorToggle.isOn ? 1 : 0,
			GetSelectedResolution().width,
			GetSelectedResolution().height,
			fullscreenToggle.isOn ? 1 : 0) );

		settingsCanvasGroup.DOFade( 0, .25f ).OnComplete( () =>
		{
			settingsCanvasGroup.gameObject.SetActive( false );
		} );
	}

	public void OnQuit()
	{
		//save settings
		Bootstrap.SaveSettings( new Tuple<int, int, int, int, int, int>(
			musicToggle.isOn ? 1 : 0,
			vignetteToggle.isOn ? 1 : 0,
			colorToggle.isOn ? 1 : 0,
			GetSelectedResolution().width,
			GetSelectedResolution().height,
			fullscreenToggle.isOn ? 1 : 0) );

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

	public void OnFullscreen()
    {
		Screen.fullScreen = fullscreenToggle.isOn;
	}

	public void OnResolution()
    {
		Resolution res = GetSelectedResolution();
		Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn);
		CalculateDialogPosition();
	}

	private Resolution GetSelectedResolution()
    {
		int index = resolutionDropdown.GetComponent<TMP_Dropdown>().value;
		string[] resString = resolutionDropdown.options[index].text.Split('x');
		Resolution res = new Resolution();
		res.width = Int32.Parse(resString[0]);
		res.height = Int32.Parse (resString[1]);
		return res;
	}

	private void CalculateDialogPosition()
    {
		rect = settingsCanvasGroup.gameObject.GetComponent<RectTransform>();
		ap = rect.anchoredPosition;
		sp = settingsCanvasGroup.gameObject.transform.position;
	}
}
