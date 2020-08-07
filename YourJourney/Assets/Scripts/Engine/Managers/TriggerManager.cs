using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TriggerManager : MonoBehaviour
{
	public Dictionary<string, bool> triggerList = new Dictionary<string, bool>();

	public void FireTrigger( string name )
	{
		if ( !string.IsNullOrEmpty( name )
			&& name.ToLower() != "none"
			&& !triggerList.ContainsKey( name ) )
			StartCoroutine( TriggerChain( name ) );
		else
			Debug.Log( "FireTrigger::NO TRIGGER/NONE" );
	}

	IEnumerator TriggerChain( string name )
	{
		//if ( !string.IsNullOrEmpty( name )
		//	&& name.ToLower() != "none"
		//	&& !triggerList.ContainsKey( name ) )
		//{
		Debug.Log( "FireTrigger::" + name );
		triggerList.Add( name, true );

		//trigger Token
		if ( FindObjectOfType<TileManager>().TryTriggerToken( name ) )
			yield break;
		yield return WaitUntilFinished();

		//trigger Objective
		if ( FindObjectOfType<ObjectiveManager>().TrySetObjective( name ) )
			yield break;
		yield return WaitUntilFinished();

		//trigger Objective complete
		if ( FindObjectOfType<ObjectiveManager>().TryCompleteObjective( name ) )
			yield break;
		yield return WaitUntilFinished();

		//trigger Interactions
		if ( FindObjectOfType<InteractionManager>().TryFireInteraction( name ) )
			yield break;
		yield return WaitUntilFinished();

		//trigger Chapter
		if ( FindObjectOfType<ChapterManager>().TriggerChapterByTrigger( name ) )
			yield break;
		yield return WaitUntilFinished();

		//trigger end scenario
		if ( FindObjectOfType<InteractionManager>().TryFireEndScenario( name ) )
			yield break;
		yield return WaitUntilFinished();

		//}
		//else
		//	Debug.Log( "FireTrigger::NO TRIGGER/NONE" );
	}

	IEnumerator WaitUntilFinished()
	{
		//Debug.Log( "***WAITING..." );
		var im = FindObjectOfType<InteractionManager>();
		//wait for all UI screens to go away - signifies all triggers finished
		//poll IManager uiRoot for children (panels)
		while ( im.PanelShowing )
			yield return null;
		//Debug.Log( "***DONE WAITING..." );
	}

	public bool IsTriggered( string name )
	{
		return triggerList.TryGetValue( name, out bool v ) ? v : false;
	}
}