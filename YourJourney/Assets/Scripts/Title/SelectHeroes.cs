using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SelectHeroes : MonoBehaviour
{
	public SelectJourney selectJourney;
	public SpecialInstructions specialInstructions;
	public Image finalFader;
	public Button[] heroButtons;
	public Button beginButton, backButton;
	public Text[] heroNameText;
	public Text diffText;

	bool[] selectedHeroes;
	string tempName;
	bool isChangingName = false;
	int nameIndex = -1;
	ProjectItem selectedJourney;
	int slotMode;

	public void ActivateScreen( ProjectItem journey, int smode )
	{
		slotMode = smode;
		gameObject.SetActive( true );
		selectedJourney = journey;
		selectedHeroes = new bool[6].Fill( false );
		Bootstrap.difficulty = Difficulty.Normal;
		diffText.text = "Normal";
		ResetHeros();

		finalFader.DOFade( 0, .5f ).OnComplete( () =>
		{
			backButton.interactable = true;
		} );
	}

	public void OnHeroSelect( int index )
	{
		isChangingName = false;
		if ( nameIndex != -1 )
		{
			heroNameText[nameIndex].color = Color.white;
			heroNameText[nameIndex].text = tempName;
		}

		ColorBlock cb = heroButtons[index].colors;
		heroButtons[index].colors = new ColorBlock()
		{
			normalColor = new Color( 1, 167f / 255f, 124f / 255f, 1 ),
			pressedColor = cb.pressedColor,
			selectedColor = new Color( 1, 167f / 255f, 124f / 255f, 1 ),
			colorMultiplier = cb.colorMultiplier,
			disabledColor = cb.disabledColor,
			fadeDuration = cb.fadeDuration,
			highlightedColor = cb.highlightedColor
		};
		selectedHeroes[index] = !selectedHeroes[index];

		ResetHeros();

		beginButton.interactable = selectedHeroes.Any( b => b );
	}

	void ResetHeros()
	{
		for ( int i = 0; i < 6; i++ )
		{
			heroNameText[i].color = Color.white;
			heroNameText[i].text = Bootstrap.GetHeroName( i );
			ColorBlock cb = heroButtons[i].colors;
			if ( !selectedHeroes[i] )
			{
				heroButtons[i].colors = new ColorBlock()
				{
					normalColor = new Color( 1, 167f / 255f, 124f / 255f, 0 ),
					pressedColor = cb.pressedColor,
					selectedColor = new Color( 1, 167f / 255f, 124f / 255f, 0 ),
					colorMultiplier = cb.colorMultiplier,
					disabledColor = cb.disabledColor,
					fadeDuration = cb.fadeDuration,
					highlightedColor = cb.highlightedColor
				};
			}
		}
	}

	public void OnDifficulty()
	{
		if ( Bootstrap.difficulty == Difficulty.Easy )
			Bootstrap.difficulty = Difficulty.Normal;
		else if ( Bootstrap.difficulty == Difficulty.Normal )
			Bootstrap.difficulty = Difficulty.Hard;
		else if ( Bootstrap.difficulty == Difficulty.Hard )
			Bootstrap.difficulty = Difficulty.Easy;
		diffText.text = Bootstrap.difficulty.ToString();
	}

	public void OnChangeNameClick( int index )
	{
		isChangingName = true;
		nameIndex = index;
		heroNameText[index].color = Color.green;
		tempName = heroNameText[nameIndex].text;
		heroNameText[nameIndex].text = "";
	}

	public void OnNext()
	{
		beginButton.interactable = backButton.interactable = false;
		string[] sh = new string[6].Fill( "" );
		for ( int i = 0; i < 6; i++ )
		{
			if ( selectedHeroes[i] )
				sh[i] = heroNameText[i].text;
		}
		sh = sh.Where( s => !string.IsNullOrEmpty( s ) ).ToArray();

		Bootstrap.heroes = sh;
		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			specialInstructions.ActivateScreen( sh, selectedJourney, slotMode );
			gameObject.SetActive( false );
		} );

		//Bootstrap.SetGameVars( sh, selectedJourney.fileName );

		//DOTween.To( () => music.volume, setter => music.volume = setter, 0f, .5f );
		//finalFader.gameObject.SetActive( true );
		//finalFader.DOFade( 1, .5f ).OnComplete( () => SceneManager.LoadScene( "gameboard" ) );
	}

	public void OnBack()
	{
		beginButton.interactable = backButton.interactable = false;
		isChangingName = false;
		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			selectJourney.ActivateScreen( slotMode );
			gameObject.SetActive( false );
		} );
	}

	private void Update()
	{
		if ( isChangingName )
		{
			if ( Input.GetKeyDown( KeyCode.Escape ) )
			{
				isChangingName = false;
				heroNameText[nameIndex].color = Color.white;
				heroNameText[nameIndex].text = tempName;
				return;
			}

			foreach ( char c in Input.inputString )
			{
				if ( c == '\b' ) // has backspace/delete been pressed?
				{
					if ( heroNameText[nameIndex].text.Length != 0 )
					{
						heroNameText[nameIndex].text = heroNameText[nameIndex].text.Substring( 0, heroNameText[nameIndex].text.Length - 1 );
					}
				}
				else if ( ( c == '\n' ) || ( c == '\r' ) ) // enter/return
				{
					isChangingName = false;
					heroNameText[nameIndex].color = Color.white;
					if ( string.IsNullOrEmpty( heroNameText[nameIndex].text ) )
						heroNameText[nameIndex].text = tempName;
					else
						Bootstrap.SaveHeroName( nameIndex, heroNameText[nameIndex].text );
				}
				else
				{
					heroNameText[nameIndex].text += c;
				}
			}
		}
	}
}
