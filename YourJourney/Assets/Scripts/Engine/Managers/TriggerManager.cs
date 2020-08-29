using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerManager : MonoBehaviour
{
	public Dictionary<string, bool> firedTriggersList = new Dictionary<string, bool>();

	Queue<string> queue = new Queue<string>();

	/// <summary>
	/// Takes a trigger name or event name to fire
	/// </summary>
	public void FireTrigger( string name )
	{
		if ( !string.IsNullOrEmpty( name )
			&& name.ToLower() != "none"
			&& !firedTriggersList.ContainsKey( name ) )
		{
			if ( queue.Count == 0 )
			{
				Debug.Log( "FIRST ENQUEUE: " + name );
				queue.Enqueue( name );
				StartCoroutine( TriggerChain() );
			}
			else
			{
				Debug.Log( "ENQUEUE: " + name );
				queue.Enqueue( name );
			}
		}
		else
			Debug.Log( "FireTrigger::NO TRIGGER/NONE" );
	}

	IEnumerator TriggerChain()
	{
		Debug.Log( "*******************TriggerChain STARTED" );

		while ( queue.Count > 0 )
		{
			yield return WaitUntilFinished();
			string name = queue.Peek();
			Debug.Log( "FireTrigger::" + name );
			if ( firedTriggersList.ContainsKey( name ) )
			{
				Debug.Log( "WARNING: Trigger has already been fired: " + name );
				queue.Dequeue();
				continue;
			}
			firedTriggersList.Add( name, true );

			//first, check conditional events listening for triggers to fire
			foreach ( ConditionalInteraction con in FindObjectOfType<InteractionManager>().interactions.Where( x => x.interactionType == InteractionType.Conditional ) )
			{
				bool containsAll = con.triggerList.All( x => firedTriggersList.Keys.Contains( x ) );
				if ( containsAll )
					FireTrigger( con.finishedTrigger );
			}

			//trigger Token
			if ( FindObjectOfType<TileManager>().TryTriggerToken( name ) )
			{
				queue.Dequeue();
				continue;
			}
			yield return WaitUntilFinished();

			//trigger Objective
			if ( FindObjectOfType<ObjectiveManager>().TrySetObjective( name ) )
			{
				queue.Dequeue();
				continue;
			}
			yield return WaitUntilFinished();

			//trigger Objective complete
			if ( FindObjectOfType<ObjectiveManager>().TryCompleteObjective( name ) )
			{
				queue.Dequeue();
				continue;
			}
			yield return WaitUntilFinished();

			//trigger Event by trigger
			if ( FindObjectOfType<InteractionManager>().TryFireEventByTrigger( name ) )
			{
				queue.Dequeue();
				continue;
			}
			yield return WaitUntilFinished();

			//trigger Event by event name
			if ( FindObjectOfType<InteractionManager>().TryFireEventByName( name ) )
			{
				queue.Dequeue();
				continue;
			}
			yield return WaitUntilFinished();

			//trigger Chapter
			if ( FindObjectOfType<ChapterManager>().TriggerChapterByTrigger( name ) )
			{
				queue.Dequeue();
				continue;
			}
			yield return WaitUntilFinished();

			//trigger end scenario
			if ( FindObjectOfType<InteractionManager>().TryFireEndScenario( name ) )
			{
				queue.Dequeue();
				continue;
			}
			yield return WaitUntilFinished();

			string n = queue.Dequeue();
			Debug.Log( "Nothing listening to: " + n );
		}
		Debug.Log( "******************TriggerChain ENDED" );

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

		if ( FindObjectOfType<ShadowPhaseManager>().doingShadowPhase )
		{
			Debug.Log( "Waiting for Shadow Phase to finish..." );
			while ( FindObjectOfType<ShadowPhaseManager>().doingShadowPhase )
				yield return null;
		}
		//Debug.Log( "***DONE WAITING..." );
	}

	/// <summary>
	/// returns whether or not the given Trigger has fired
	/// </summary>
	public bool IsTriggered( string name )
	{
		return firedTriggersList.TryGetValue( name, out bool v ) ? v : false;
	}
}