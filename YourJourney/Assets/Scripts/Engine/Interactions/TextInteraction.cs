using System;

public class TextInteraction : IInteraction
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
	public PersonType personType { get; set; }
	public int loreReward { get; set; }
	public bool isPersistant { get; set; }

	//text props
	public bool isPersistent, hasActivated = false;
	public string persistentText;

	public InteractionType interactionType { get { return InteractionType.Text; } set { } }
}

