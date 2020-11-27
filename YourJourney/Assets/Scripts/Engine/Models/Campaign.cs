using System;
using System.Collections.ObjectModel;

public class Campaign
{
	public Guid campaignGUID { get; set; }
	public string campaignName { get; set; }
	public string fileVersion { get; set; }
	public string storyText { get; set; }
	public string description { get; set; }

	public ObservableCollection<CampaignItem> scenarioCollection { get; set; }
	public ObservableCollection<Trigger> triggerCollection { get; set; }

	public Campaign()
	{
		scenarioCollection = new ObservableCollection<CampaignItem>();
		triggerCollection = new ObservableCollection<Trigger>();
	}
}
