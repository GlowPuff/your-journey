using System;
using System.Collections.Generic;

public class MultiEventInteraction : IInteraction
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
	public List<string> eventList { get; set; }
	public List<string> triggerList { get; set; }
	public bool usingTriggers { get; set; }
	public bool isSilent { get; set; }

	public InteractionType interactionType { get { return InteractionType.MultiEvent; } set { } }
}