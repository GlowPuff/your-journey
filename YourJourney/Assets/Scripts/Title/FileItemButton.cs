using System;
using UnityEngine;
using UnityEngine.UI;

public class FileItemButton : MonoBehaviour
{
	public Image image, icon;
	public Text title;
	int index;
	Action<int> callback;
	Color clickColor = new Color( 1, 1, 1, 1 );

	public void Init( int idx, string t, ProjectType ptype, Action<int> cb = null )
	{
		index = idx;
		title.text = t;
		callback = cb;
		if ( ptype == ProjectType.Campaign )
			icon.color = new Color( 1, 197f / 255f, 0 );
		else
			icon.color = new Color( 1, 1, 1 );
	}

	public void SetSuccess( ScenarioStatus status )
	{
		if ( status == ScenarioStatus.Success )
		{
			image.color = new Color( 0, 1, 0, .2f );
			clickColor = new Color( 0, 1, 0, 1 );
		}
		else if ( status == ScenarioStatus.Failure )
		{
			image.color = new Color( 1, 0, 0, .2f );
			clickColor = new Color( 1, 0, 0, 1 );
		}
		else
		{
			image.color = new Color( 1, 1, 1, .2f );
			clickColor = new Color( 1, 1, 1, 1 );
		}
	}

	public void RemoveRing()
	{
		icon.gameObject.SetActive( false );
	}

	public void OnClick()
	{
		callback?.Invoke( index );
		image.color = clickColor;// new Color( 1, 1, 1, 1 );

		//( 114f / 255f, 255, 0, .2f );
	}

	public void ResetColor()
	{
		image.color = new Color( clickColor.r, clickColor.g, clickColor.b, .2f );
	}
}
