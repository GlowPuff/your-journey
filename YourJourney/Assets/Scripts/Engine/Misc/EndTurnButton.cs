using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using System.Collections.Generic;

public class EndTurnButton : MonoBehaviour
{
	class ThresholdMeta : MonoBehaviour
	{
		public int threshold;
	}

	public Transform tickParent;
	public GameObject tickPrefab;
	public Text threatText;
	public Image meter;
	public GameObject numberArea;

	[HideInInspector]
	public float currentThreat, threatMax;

	float currentThreatAnimated;
	Queue<Threat> threatStack;
	GameObject nextTickObject;

	/// <summary>
	/// set starting threat (new game/continue)
	/// </summary>
	public void Init( Scenario s )
	{
		currentThreatAnimated = currentThreat = 0;
		threatMax = s.threatMax;
		threatStack = new Queue<Threat>();

		if ( s.threatNotUsed || s.threatObserver.Count() == 0 )
		{
			numberArea.SetActive( false );
			return;
		}

		foreach ( Threat t in s.threatObserver )
			threatStack.Enqueue( t );

		AddThresholdTick();
	}

	void AddThresholdTick()
	{
		if ( threatStack.Count == 0 )
		{
			nextTickObject = null;
			return;
		}

		Threat t = threatStack.Peek();

		GameObject go = Instantiate( tickPrefab, tickParent );
		float angle = GlowEngine.RemapValue( t.threshold, 0, threatMax, 0, 360 );
		go.transform.DORotate( new Vector3( 0, 0, -angle ), 1 ).SetEase( Ease.OutBounce );
		Text text = go.transform.GetComponentInChildren<Text>();
		text.text = t.threshold.ToString();
		text.transform.Rotate( new Vector3( 0, 0, angle ) );
		go.AddComponent<ThresholdMeta>();
		go.GetComponent<ThresholdMeta>().threshold = t.threshold;

		nextTickObject = go;
	}

	public Threat[] AddThreat( float amount )
	{
		if ( FindObjectOfType<Engine>().scenario.threatNotUsed || FindObjectOfType<Engine>().scenario.threatObserver.Count() == 0 )
			return new Threat[0];

		currentThreat = Mathf.Min( currentThreat + amount, threatMax );
		DOTween.To( () => currentThreatAnimated, x => currentThreatAnimated = x, currentThreat, 2 ).SetEase( Ease.InOutQuad );

		List<Threat> retval = new List<Threat>();
		int max = threatStack.Count;
		for ( int i = 0; i < max; i++ )
		{
			Threat t = threatStack.Peek();
			if ( currentThreat >= t.threshold )
				retval.Add( threatStack.Dequeue() );
		}

		//remove tick from button
		if ( nextTickObject != null && nextTickObject.GetComponent<ThresholdMeta>() != null )
		{
			if ( currentThreat >= nextTickObject.GetComponent<ThresholdMeta>().threshold )
				GameObject.Destroy( nextTickObject );
			AddThresholdTick();
		}

		return retval.ToArray();
	}

	//SETS the threat level without firing events
	public void SetThreat( float amount )
	{
		currentThreat = amount;
		DOTween.To( () => currentThreatAnimated, x => currentThreatAnimated = x, currentThreat, 2 ).SetEase( Ease.InOutQuad );
	}

	private void Update()
	{
		threatText.text = Mathf.Ceil( currentThreatAnimated ).ToString() + "\r\n" + threatMax.ToString();
		meter.fillAmount = currentThreatAnimated / threatMax;
	}
}
