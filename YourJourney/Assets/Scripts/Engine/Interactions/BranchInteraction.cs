using System;
using UnityEngine;

public class BranchInteraction : IInteraction
{
	public Guid GUID { get; set; }
	public string dataName { get; set; }
	public bool isEmpty { get; set; }
	public string triggerName { get; set; }
	public string triggerAfterName { get; set; }
	public TextBookData textBookData { get; set; }
	public TextBookData eventBookData { get; set; }
	public bool isTokenInteraction { get; set; }
	public TokenType tokenType { get; set; }
	public int loreReward { get; set; }

	public string triggerTest;
	public string triggerIsSet;
	public string triggerNotSet;
	public string triggerIsSetTrigger;
	public string triggerNotSetTrigger;
	public bool branchTestEvent;

	public InteractionType interactionType { get { return InteractionType.Branch; } set { } }

	public static BranchInteraction Create( Interaction interaction )
	{
		BranchInteraction sb = new BranchInteraction()
		{
			GUID = interaction.GUID,
			dataName = interaction.dataName,
			isEmpty = interaction.isEmpty,
			triggerName = interaction.triggerName,
			triggerAfterName = interaction.triggerAfterName,
			textBookData = interaction.textBookData,
			eventBookData = interaction.eventBookData,

			triggerTest = interaction.triggerTest,
			triggerIsSet = interaction.triggerIsSet,
			triggerNotSet = interaction.triggerNotSet,
			triggerIsSetTrigger = interaction.triggerIsSetTrigger,
			triggerNotSetTrigger = interaction.triggerNotSetTrigger,
			branchTestEvent = interaction.branchTestEvent,
		};
		return sb;
	}

	public void Resolve( InteractionManager im )
	{
		if ( branchTestEvent )//if it's an EVENT to activate
		{
			//if the trigger test HAS BEEN FIRED...
			if ( im.engine.triggerManager.IsTriggered( triggerTest ) )
				im.TryFireEventByName( triggerIsSet );
			else//otherwise...
				im.TryFireEventByName( triggerNotSet );
		}
		else//otherwise it's a TRIGGER, so fire it
		{
			//if the trigger test HAS BEEN FIRED...
			if ( im.engine.triggerManager.IsTriggered( triggerTest ) )
				im.engine.triggerManager.FireTrigger( triggerIsSetTrigger );
			else//otherwise...
				im.engine.triggerManager.FireTrigger( triggerNotSetTrigger );
		}
	}
}