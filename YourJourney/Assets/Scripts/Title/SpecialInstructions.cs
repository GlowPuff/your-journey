using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SpecialInstructions : MonoBehaviour
{
	public SelectHeroes selectHeroes;
	public Image finalFader;
	public Button beginButton, cancelButton, backButton;
	public Text loreText, instructions;
	public AudioSource music;

	int slotMode;
	ProjectItem selectedJourney;
	string[] selectedHeroes;
	RectTransform itemContainer;

	public void ActivateScreen( string[] heronames, ProjectItem journey, int smode )
	{
		gameObject.SetActive( true );
		slotMode = smode;
		selectedJourney = journey;
		selectedHeroes = heronames;
		itemContainer = instructions.rectTransform;

		loreText.text = "0";
		instructions.text = "";

		finalFader.DOFade( 0, .5f ).OnComplete( () =>
		{
			beginButton.interactable = true;
			backButton.interactable = true;
			cancelButton.interactable = true;
			Scenario s = Bootstrap.LoadLevel( journey.fileName );
			if ( s != null )
			{
				if ( !string.IsNullOrEmpty( s.specialInstructions ) )
					SetText( s.specialInstructions );
				else
					SetText( "There are no special instructions for this Scenario." );
				loreText.text = s.loreStartValue.ToString();
			}
			else
			{
				SetText( "There was a problem loading the Scenario." );
				beginButton.interactable = false;
			}
		} );
	}

	void SetText( string t )
	{
		instructions.text = t;

		TextGenerator textGen = new TextGenerator();
		TextGenerationSettings generationSettings = instructions.GetGenerationSettings( instructions.rectTransform.rect.size );

		float height = textGen.GetPreferredHeight( t, generationSettings );

		itemContainer.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, height + 20 );
	}

	public void OnBegin()
	{
		Bootstrap.heroes = selectedHeroes;
		Bootstrap.scenarioFileName = selectedJourney.fileName;
		DOTween.To( () => music.volume, setter => music.volume = setter, 0f, .5f );
		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			SceneManager.LoadScene( "gameboard" );
		} );
	}

	public void OnBack()
	{
		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			selectHeroes.ActivateScreen( selectedJourney, slotMode );
			gameObject.SetActive( false );
		} );
	}

	public void OnCancel()
	{
		beginButton.interactable = false;
		backButton.interactable = false;
		cancelButton.interactable = false;

		finalFader.DOFade( 1, .5f ).OnComplete( () =>
		{
			gameObject.SetActive( false );
			FindObjectOfType<TitleManager>().ResetScreen();
		} );
	}
}

