using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// JSON serialization/deserialization for .jime editor files
/// </summary>
public class FileManager
{
	public Guid scenarioGUID { get; set; }
	public Guid campaignGUID { get; set; }
	public int loreStartValue { get; set; }
	public string specialInstructions { get; set; }
	public string fileVersion { get; set; }
	//public string fileName { get; set; }
	public string saveDate { get; set; }

	[JsonConverter( typeof( InteractionConverter ) )]
	public List<IInteraction> interactions { get; set; }
	public List<Trigger> triggers { get; set; }
	public List<Objective> objectives { get; set; }
	public List<TextBookData> resolutions { get; set; }
	public List<Threat> threats { get; set; }
	public List<Chapter> chapters { get; set; }
	public List<int> globalTiles { get; set; }
	public TextBookData introBookData { get; set; }
	public ProjectType projectType { get; set; }
	public string scenarioName { get; set; }
	public string objectiveName { get; set; }
	public int threatMax { get; set; }
	public bool threatNotUsed { get; set; }
	public bool scenarioTypeJourney { get; set; }
	public int shadowFear { get; set; }

	public FileManager()
	{

	}

	public FileManager( Scenario source )
	{
		fileVersion = source.fileVersion;
		saveDate = source.saveDate;

		interactions = source.interactionObserver;
		triggers = source.triggersObserver.ToList();
		objectives = source.objectiveObserver.ToList();
		resolutions = source.resolutionObserver.ToList();
		threats = source.threatObserver.ToList();
		chapters = source.chapterObserver.ToList();
		globalTiles = source.globalTilePool.ToList();

		introBookData = source.introBookData;
		projectType = source.projectType;
		scenarioName = source.scenarioName;
		objectiveName = source.objectiveName;
		threatMax = source.threatMax;
		threatNotUsed = source.threatNotUsed;
		scenarioTypeJourney = source.scenarioTypeJourney;
		shadowFear = source.shadowFear;
		specialInstructions = source.specialInstructions;
		scenarioGUID = source.scenarioGUID;
		campaignGUID = source.campaignGUID;
		loreStartValue = source.loreStartValue;
	}

	/// <summary>
	/// Supply the FULL PATH with the filename
	/// </summary>
	public static Scenario Load( string filename )
	{
		string mydocs = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
		string basePath = Path.Combine( mydocs, "Your Journey" );
		if ( !Directory.Exists( basePath ) )
			Directory.CreateDirectory( basePath );

		try
		{
			string json = "";
			using ( StreamReader sr = new StreamReader( filename ) )
			{
				json = sr.ReadToEnd();
			}

			var fm = JsonConvert.DeserializeObject<FileManager>( json );

			return Scenario.CreateInstance( fm );
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// Return ProjectItem info for files in Project folder
	/// </summary>
	public static IEnumerable<ProjectItem> GetProjects()
	{
		string mydocs = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
		string basePath = Path.Combine( mydocs, "Your Journey" );
		if ( !Directory.Exists( basePath ) )
			Directory.CreateDirectory( basePath );
		//string basePath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Projects" );
		List<ProjectItem> items = new List<ProjectItem>();
		DirectoryInfo di = new DirectoryInfo( basePath );
		FileInfo[] files = di.GetFiles();
		foreach ( FileInfo fi in files )
		{
			//Debug.Log( fi.FullName );
			Scenario s = Load( fi.FullName );
			if ( s != null )
				items.Add( new ProjectItem()
				{
					Title = s.scenarioName,
					projectType = s.projectType,
					Date = s.saveDate,
					fileName = fi.Name,//s.fileName,
					fileVersion = s.fileVersion
				} );
		}
		return items;
	}

	/// <summary>
	/// this should build the full path to filename, including the documents folder from ANY system type: windows/mac/linux
	/// </summary>
	public static string GetFullPath( string filename )
	{
		string mydocs = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
		string basePath = Path.Combine( mydocs, "Your Journey", filename );

		return basePath;
	}
}
