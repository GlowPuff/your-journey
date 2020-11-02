using System;

public class DialogInteraction : IInteraction
{
	public string dataName { get; set; }
	public Guid GUID { get; set; }
	public bool isTokenInteraction { get; set; }
	public string triggerName { get; set; }
	public string triggerAfterName { get; set; }
	public TextBookData textBookData { get; set; }
	public TextBookData eventBookData { get; set; }
	public TokenType tokenType { get; set; }
	public PersonType personType { get; set; }
	public int loreReward { get; set; }
	public bool isPersistant { get; set; }


	public string choice1, choice2, choice3, c1Trigger, c2Trigger, c3Trigger, persistentText, c1Text, c2Text, c3Text;

	public bool c1Used = false, c2Used = false, c3Used = false, hasActivated = false, isPersistent, isDone = false;

	public InteractionType interactionType { get { return InteractionType.Dialog; } set { } }
}

