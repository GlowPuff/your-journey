using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Scenario
{
	public string fileVersion { get; set; }
	public string saveDate { get; set; }
	public bool scenarioTypeJourney { get; set; }
	//public string fileName { get; set; }
	public string scenarioName { get; set; }
	/// <summary>
	/// First Objective, if set in the editor
	/// </summary>
	public string objectiveName { get; set; }
	public int threatMax { get; set; }
	public bool threatNotUsed { get; set; }
	public ProjectType projectType { get; set; }
	public int shadowFear { get; set; }

	public TextBookData introBookData { get; set; }

	public List<IInteraction> interactionObserver { get; set; }
	public ObservableCollection<Trigger> triggersObserver { get; set; }
	public ObservableCollection<Objective> objectiveObserver { get; set; }
	public ObservableCollection<TextBookData> resolutionObserver { get; set; }
	public ObservableCollection<Threat> threatObserver { get; set; }
	public ObservableCollection<Chapter> chapterObserver { get; set; }
	public ObservableCollection<int> globalTilePool { get; set; }

	/// <summary>
	/// Load in data from FileManager
	/// </summary>
	public static Scenario CreateInstance( FileManager fm )
	{
		Scenario s = new Scenario();
		s.scenarioName = fm.scenarioName;
		s.fileVersion = fm.fileVersion;
		//s.fileName = fm.fileName;
		s.saveDate = fm.saveDate;
		s.projectType = fm.projectType;
		s.objectiveName = fm.objectiveName;
		s.interactionObserver = new List<IInteraction>( fm.interactions );
		s.triggersObserver = new ObservableCollection<Trigger>( fm.triggers );
		s.objectiveObserver = new ObservableCollection<Objective>( fm.objectives );
		s.resolutionObserver = new ObservableCollection<TextBookData>( fm.resolutions );
		s.threatObserver = new ObservableCollection<Threat>( fm.threats );
		s.chapterObserver = new ObservableCollection<Chapter>( fm.chapters );
		s.globalTilePool = new ObservableCollection<int>( fm.globalTiles );
		//s.fileName = fm.fileName;
		s.introBookData = fm.introBookData;
		s.threatMax = fm.threatMax;
		s.threatNotUsed = fm.threatNotUsed;
		s.scenarioTypeJourney = fm.scenarioTypeJourney;
		s.shadowFear = fm.shadowFear;

		return s;
	}
}