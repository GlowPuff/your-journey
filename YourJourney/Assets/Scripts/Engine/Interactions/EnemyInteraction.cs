using System;
using System.Collections.Generic;

public class ThreatInteraction : IInteraction
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
	public string triggerDefeatedName { get; set; }

	public List<Monster> monsterCollection;

	public InteractionType interactionType { get { return InteractionType.Threat; } set { } }

	//public static ThreatInteraction Create( Interaction interaction )
	//{
	//	ThreatInteraction ti = new ThreatInteraction()
	//	{
	//		GUID = interaction.GUID,
	//		dataName = interaction.dataName,
	//		isEmpty = interaction.isEmpty,
	//		triggerName = interaction.triggerName,
	//		triggerAfterName = interaction.triggerAfterName,
	//		textBookData = interaction.textBookData,
	//		eventBookData = interaction.eventBookData,
	//		monsterCollection = interaction.monsterCollection.Select( x => x ).ToList()
	//	};
	//	return ti;
	//	//Monster m = new Monster
	//	//{
	//	//	GUID = interaction.GUID,
	//	//	dataName = interaction.dataName,
	//	//	isEmpty = interaction.isEmpty,
	//	//	triggerName = "None",

	//	//	damage = 1,
	//	//	fear = 1,
	//	//	health = 8,
	//	//	currentHealth = 8,
	//	//	negatedBy = Ability.None,
	//	//	isLarge = false,
	//	//	count = 1
	//	//};

	//	//return m;
	//}
}