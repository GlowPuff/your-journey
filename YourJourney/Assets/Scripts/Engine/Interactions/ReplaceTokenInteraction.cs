using System;

public class ReplaceTokenInteraction : IInteraction
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

	public InteractionType interactionType { get { return InteractionType.Replace; } set { } }

	public string eventToReplace { get; set; }
	public string replaceWithEvent { get; set; }
	public bool noText { get; set; }
	public Guid replaceWithGUID { get; set; }

	public bool hasActivated = false;
}
