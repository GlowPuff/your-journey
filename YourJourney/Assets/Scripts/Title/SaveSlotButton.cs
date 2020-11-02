using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButton : MonoBehaviour
{
	public int index;
	public Image image;
	public Text title, heroes;
	public GameObject emptyText;
	public Button delButton;
	[HideInInspector]
	public bool isEmpty { get => emptyText.activeInHierarchy; }

	Button thisButton;

	private void Awake()
	{
		thisButton = GetComponent<Button>();
	}

	public void Init( StateItem state )
	{
		if ( state == null )
			return;
		thisButton = GetComponent<Button>();
		emptyText.SetActive( false );
		thisButton.interactable = true;
		delButton.interactable = true;
		title.text = state.gameName;
		heroes.text = state.heroes;
		ResetColor();
	}

	public void SetName( string name )
	{
		title.text = name;
	}

	//interactable=false, set to empty, reset color
	public void ResetButton()
	{
		thisButton.interactable = false;
		delButton.interactable = false;
		title.text = heroes.text = "";
		emptyText.SetActive( true );
		ResetColor();
	}

	public void EnableButton( bool isEnabled = true )
	{
		thisButton.interactable = isEnabled;
	}

	public void SetIsEmpty( bool isempty )
	{
		emptyText.SetActive( isempty );
	}

	public void OnClick()
	{
		Debug.Log( "CLICK: " + index );
		FindObjectOfType<SelectSaveSlot>().OnSelectSlot( index );
		image.color = new Color( 1, 1, 1, 1 );
	}

	public void OnDelete()
	{
		FindObjectOfType<SelectSaveSlot>().OnDeleteSlot( index );
	}

	public void ResetColor()
	{
		image.color = new Color( 255, 255, 255, .2f );
	}
}
