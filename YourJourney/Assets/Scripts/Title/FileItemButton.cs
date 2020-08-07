using UnityEngine;
using UnityEngine.UI;

public class FileItemButton : MonoBehaviour
{
	public Image image;
	public Text title;
	int index;

	public void Init( int idx, string t )
	{
		index = idx;
		title.text = t;
	}

	public void OnClick()
	{
		FindObjectOfType<TitleManager>().OnSelectQuest( index );
		image.color = new Color( 114f / 255f, 255, 0, .2f );
	}

	public void ResetColor()
	{
		image.color = new Color( 255, 255, 255, .2f );
	}
}
