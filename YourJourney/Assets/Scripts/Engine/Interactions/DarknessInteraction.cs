using System;

public class DarknessInteraction : IInteraction
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

	public InteractionType interactionType { get { return InteractionType.Darkness; } set { } }

	public static DarknessInteraction Create( Interaction interaction )
	{
		DarknessInteraction d = new DarknessInteraction()
		{
			GUID = interaction.GUID,
			dataName = interaction.dataName,
			isEmpty = interaction.isEmpty,
			triggerName = interaction.triggerName,
			triggerAfterName = interaction.triggerAfterName,
			textBookData = interaction.textBookData,
			eventBookData = interaction.eventBookData
		};
		return d;
	}
}

