using System;

public class StatTestInteraction : IInteraction
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

	public Ability testAttribute;
	public bool isCumulative;
	public bool passFail;
	public int successValue;
	public string successTrigger;
	public string failTrigger;
	public TextBookData passBookData;
	public TextBookData failBookData;
	public TextBookData progressBookData;

	public InteractionType interactionType { get { return InteractionType.StatTest; } set { } }

	public int accumulatedValue = -1;

	public static StatTestInteraction Create( Interaction interaction )
	{
		StatTestInteraction st = new StatTestInteraction
		{
			GUID = interaction.GUID,
			dataName = interaction.dataName,
			isEmpty = interaction.isEmpty,
			triggerName = interaction.triggerName,
			triggerAfterName = interaction.triggerAfterName,
			textBookData = interaction.textBookData,
			eventBookData = interaction.eventBookData,

			testAttribute = interaction.testAttribute,
			isCumulative = interaction.isCumulative,
			successValue = interaction.successValue,
			successTrigger = interaction.successTrigger,
			failTrigger = interaction.failTrigger,
			passBookData = interaction.passBookData,
			failBookData = interaction.failBookData,
			progressBookData = interaction.progressBookData
		};

		return st;
	}

	/// <summary>
	/// Returns true if test is successful, false if not
	/// </summary>
	public bool ResolveCumulative( int amount, Engine engine )
	{
		if ( !passFail )
			accumulatedValue += amount;
		else
			accumulatedValue = amount;

		if ( accumulatedValue >= successValue )
		{
			//engine.triggerManager.FireTrigger( successTrigger );
			return true;
		}
		else
			return false;//still in progress
	}
}