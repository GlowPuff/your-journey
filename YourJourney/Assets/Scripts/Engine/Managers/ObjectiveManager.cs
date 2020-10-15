using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class ObjectiveManager : MonoBehaviour
{
	public Text objectiveText;

	List<Objective> objectiveList;
	[HideInInspector]
	public Objective currentObjective;

	public void Init( Scenario s )
	{
		objectiveList = new List<Objective>( s.objectiveObserver );
		currentObjective = null;
		//Debug.Log( $"Objective Manager: {objectiveList.Count} Objectives Found" );
	}

	/// <summary>
	/// sets the current objective by a TRIGGER, shows text box story, updates reminder text
	/// </summary>
	public bool TrySetObjective( string name, Action followupAction = null )
	{
		Debug.Log( $"TrySetObjective: {name}" );
		if ( objectiveList.Any( x => x.triggeredByName == name ) )
		{
			currentObjective = objectiveList.Where( x => x.triggeredByName == name ).First();
			Debug.Log( "Found Objective: " + currentObjective.dataName );
			//set reminder text
			objectiveText.text = currentObjective.objectiveReminder;

			//only show summary if not skipped
			if ( !currentObjective.skipSummary )
				FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( currentObjective.textBookData.pages[0], ButtonIcon.Continue, () =>
				{
					followupAction?.Invoke();
				} );
			else
				followupAction?.Invoke();
			return true;
		}

		Debug.Log( "TrySetObjective NOT FOUND: " + name );
		return false;
	}

	/// <summary>
	/// First Objective is set by its NAME, not a Trigger
	/// </summary>
	public void TrySetFirstObjective( string name, Action followupAction = null )
	{
		Debug.Log( $"TrySetFirstObjective: {name}" );
		if ( objectiveList.Any( x => x.dataName == name ) )
		{
			Debug.Log( "Found Objective" );
			currentObjective = objectiveList.Where( x => x.dataName == name ).First();
			//set reminder text
			objectiveText.text = currentObjective.objectiveReminder;

			//only show summary if not skipped
			if ( !currentObjective.skipSummary )
				FindObjectOfType<InteractionManager>().GetNewTextPanel().ShowOkContinue( currentObjective.textBookData.pages[0], ButtonIcon.Continue, () =>
				{
					followupAction?.Invoke();
				} );
			else
				followupAction?.Invoke();
			return;
		}

		Debug.Log( "TrySetFirstObjective NOT FOUND: " + name );
	}

	/// <summary>
	/// Quick debug - silently sets objective without showing dialog
	/// </summary>
	public void DebugSetObjective( string name )
	{
		if ( objectiveList.Any( x => x.dataName == name ) )
		{
			currentObjective = objectiveList.Where( x => x.dataName == name ).First();
			//set reminder text
			objectiveText.text = currentObjective.objectiveReminder;
		}
	}

	public bool Exists( string name )
	{
		return objectiveList.Any( x => x.dataName == name );
	}

	public Objective TryGetObjective( string name )
	{
		if ( objectiveList.Any( x => x.dataName == name ) )
			return objectiveList.Where( x => x.dataName == name ).First();
		else
			return null;
	}

	public bool TryCompleteObjective( string triggername )
	{
		if ( currentObjective == null )
			return false;
		Debug.Log( "TryCompleteObjective: " + name );

		//objective is complete, remove it and fire any on complete trigger
		//TODO - show completion textbox?  show lore earned?
		if ( currentObjective.triggerName == triggername )
		{
			Debug.Log( "Completed Objective: " + currentObjective.dataName );
			FindObjectOfType<LorePanel>().AddLore( currentObjective.loreReward );
			string t = currentObjective.nextTrigger;
			currentObjective = null;
			objectiveText.text = "No Objective";
			FindObjectOfType<TriggerManager>().FireTrigger( t );
			return true;
		}

		Debug.Log( "TryCompleteObjective NOT FOUND: " + name );
		return false;
	}

	public ObjectiveState GetState()
	{
		return new ObjectiveState()
		{
			currentObjective = currentObjective.GUID
		};
	}
}
