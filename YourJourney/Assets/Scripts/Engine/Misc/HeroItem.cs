using UnityEngine;
using UnityEngine.UI;

public class HeroItem : MonoBehaviour
{
	public int heroIndex;
	public Text heroNameText, thresholdText;
	public Button dButton, fButton;
	public Image skullImage;
	[HideInInspector]
	public PartyPanel pPanel;

	CanvasGroup cg;

	private void Awake()
	{
		cg = GetComponent<CanvasGroup>();
	}

	public void OnDamageFinalStand()
	{
		pPanel.ToggleVisible( false );
		FindObjectOfType<InteractionManager>().GetNewDamagePanel().ShowFinalStand( Bootstrap.lastStandCounter[heroIndex], FinalStand.Damage, ( pass ) =>
		{
			Bootstrap.lastStandCounter[heroIndex]++;
			if ( !pass )
				Bootstrap.isDead[heroIndex] = true;
			UpdateUI();
			pPanel.ToggleVisible( true );
		} );
	}

	public void OnFearFinalStand()
	{
		pPanel.ToggleVisible( false );
		FindObjectOfType<InteractionManager>().GetNewDamagePanel().ShowFinalStand( Bootstrap.lastStandCounter[heroIndex], FinalStand.Fear, ( pass ) =>
		{
			Bootstrap.lastStandCounter[heroIndex]++;
			if ( !pass )
				Bootstrap.isDead[heroIndex] = true;
			UpdateUI();
			pPanel.ToggleVisible( true );
		} );
	}

	public void UpdateUI()
	{
		if ( heroIndex >= Bootstrap.heroes.Length )
			return;

		//skullImage.gameObject.SetActive( Bootstrap.lastStandCounter[heroIndex] > 1 );
		if ( Bootstrap.lastStandCounter[heroIndex] == 1 )
		{
			skullImage.color = new Color( 1, 1, 1, .05f );
			thresholdText.gameObject.SetActive( false );
		}
		else
		{
			skullImage.color = Color.white;
			thresholdText.gameObject.SetActive( true );
		}

		thresholdText.text = Bootstrap.lastStandCounter[heroIndex].ToString();
		heroNameText.text = Bootstrap.heroes[heroIndex];
		dButton.interactable = !Bootstrap.isDead[heroIndex];
		fButton.interactable = !Bootstrap.isDead[heroIndex];
		if ( Bootstrap.isDead[heroIndex] )
		{
			cg.alpha = .25f;
			skullImage.color = Color.red;
		}
	}
}
