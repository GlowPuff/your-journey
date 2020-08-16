using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class ObjectiveManager : MonoBehaviour
{
	public Text objectiveText;

	List<Objective> objectiveList;
	Objective currentObjective;

	public void Init( Scenario s )
	{
		objectiveList = new List<Objective>( s.objectiveObserver );
		currentObjective = null;
		//Debug.Log( $"Objective Manager: {objectiveList.Count} Objectives Found" );
	}

	/// <summary>
	/// sets the current objective, shows text box story, updates reminder text
	/// </summary>
	public bool TrySetObjective( string name, Action followupAction = null )
	{
		//Debug.Log( $"TrySetObjective: {name}" );
		if ( objectiveList.Any( x => x.dataName == name ) )
		{
			//Debug.Log( "Found Objective" );
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
			return true;
		}

		//Debug.Log( "TrySetObjective NOT FOUND: " + name );
		return false;
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

		Debug.Log( "Completed Objective" );
		//objective is complete, remove it and fire any on complete trigger
		//TODO - show completion textbox?  show lore earned?
		if ( currentObjective.triggerName == triggername )
		{
			string t = currentObjective.nextTrigger;
			currentObjective = null;
			objectiveText.text = "No Objective";
			FindObjectOfType<TriggerManager>().FireTrigger( t );
			return true;
		}
		return false;
	}
}
