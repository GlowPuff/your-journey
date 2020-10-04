
using System;

public class DecisionInteraction : IInteraction
{
	public Guid GUID { get; set; }
	public string dataName { get; set; }
	public bool isEmpty { get; set; }
	public string triggerName { get; set; }
	public string triggerAfterName { get; set; }
	public TextBookData textBookData { get; set; }
	public TextBookData eventBookData { get; set; }
	public TokenType tokenType { get; set; }
	public PersonType personType { get; set; }
	public bool isTokenInteraction { get; set; }
	public int loreReward { get; set; }

	public string choice1;
	public string choice2;
	public string choice3;
	public bool isThreeChoices;
	public string choice1Trigger;
	public string choice2Trigger;
	public string choice3Trigger;

	public InteractionType interactionType { get { return InteractionType.Decision; } set { } }

	public static DecisionInteraction Create( Interaction interaction )
	{
		DecisionInteraction d = new DecisionInteraction
		{
			GUID = interaction.GUID,
			dataName = interaction.dataName,
			isEmpty = interaction.isEmpty,
			triggerName = interaction.triggerName,
			triggerAfterName = interaction.triggerAfterName,
			textBookData = interaction.textBookData,
			eventBookData = interaction.eventBookData,

			choice1 = interaction.choice1,
			choice2 = interaction.choice2,
			choice3 = interaction.choice3,
			isThreeChoices = interaction.isThreeChoices,
			choice1Trigger = interaction.choice1Trigger
		};

		return d;
	}
}

