using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using System.Collections.Generic;

public class EndTurnButton : MonoBehaviour
{
	public Transform tickParent;
	public GameObject tickPrefab;
	public Text threatText;
	public Image meter;

	[HideInInspector]
	public float currentThreat, threatMax;

	float currentThreatAnimated;
	Queue<Threat> threatStack;

	/// <summary>
	/// set starting threat (new game/continue)
	/// </summary>
	public void InitialSet( Scenario s )
	{
		currentThreatAnimated = currentThreat = 0;
		threatMax = s.threatMax;
		threatStack = new Queue<Threat>();

		if ( s.threatNotUsed || s.threatObserver.Count() == 0 )
			return;

		int i = 0;
		foreach ( Threat t in s.threatObserver )
		{
			GameObject go = Instantiate( tickPrefab, tickParent );
			float angle = GlowEngine.RemapValue( t.threshold, 0, threatMax, 0, 360 );
			go.transform.DORotate( new Vector3( 0, 0, -angle ), 1 ).SetEase( Ease.OutBounce ).SetDelay( 2 + i++ );
			Text text = go.transform.GetComponentInChildren<Text>();
			text.text = t.threshold.ToString();
			text.transform.Rotate( new Vector3( 0, 0, angle ) );
			threatStack.Enqueue( t );
		}
	}

	public Threat AddThreat( float amount )
	{
		currentThreat = Mathf.Min( currentThreat + amount, threatMax );
		DOTween.To( () => currentThreatAnimated, x => currentThreatAnimated = x, currentThreat, 2 ).SetEase( Ease.InOutQuad );
		if ( threatStack.Count > 0 )
		{
			Threat t = threatStack.Peek();
			if ( currentThreat >= t.threshold )
				return threatStack.Dequeue();
			else
				return null;
		}
		else
			return null;
	}

	//SETS the threat level without firing events
	public void SetThreat( float amount )
	{
		currentThreat = amount;
		DOTween.To( () => currentThreatAnimated, x => currentThreatAnimated = x, currentThreat, 2 ).SetEase( Ease.InOutQuad );
	}

	private void Update()
	{
		threatText.text = Mathf.Ceil( currentThreatAnimated ).ToString() + "/" + threatMax.ToString();
		meter.fillAmount = currentThreatAnimated / threatMax;
	}
}
