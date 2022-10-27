﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

/// <summary>
/// JSON serialization/deserialization for .jime editor files
/// </summary>
public class FileManager
{
	public Guid scenarioGUID { get; set; }
	public Guid campaignGUID { get; set; }
	public int loreStartValue { get; set; }
	public int xpStartValue { get; set; }
	public string specialInstructions { get; set; }
	public string fileVersion { get; set; }
	//public string fileName { get; set; }
	public string saveDate { get; set; }

	[JsonConverter( typeof( InteractionConverter ) )]
	public List<IInteraction> interactions { get; set; }
	public List<Trigger> triggers { get; set; }
	public List<Objective> objectives { get; set; }
	public List<MonsterActivations> activations { get; set; }
	public List<TextBookData> resolutions { get; set; }
	public List<Threat> threats { get; set; }
	public List<Chapter> chapters { get; set; }
	//public List<Collection> collections { get; set; }
	public List<int> collections { get; set; }
	public List<int> globalTiles { get; set; }
	public Dictionary<string, bool> scenarioEndStatus { get; set; }
	public TextBookData introBookData { get; set; }
	public ProjectType projectType { get; set; }
	public string scenarioName { get; set; }
	public string objectiveName { get; set; }
	public int threatMax { get; set; }
	public bool threatNotUsed { get; set; }
	public bool scenarioTypeJourney { get; set; }
	public int shadowFear { get; set; }
	public int loreReward { get; set; }
	public int xpReward { get; set; }

	public FileManager()
	{
		//empty ctor for json deserialization
	}

	public FileManager( Scenario source )
	{
		fileVersion = source.fileVersion;
		saveDate = source.saveDate;

		interactions = source.interactionObserver;
		triggers = source.triggersObserver.ToList();
		objectives = source.objectiveObserver.ToList();
		activations = source.activationsObserver.ToList();
		resolutions = source.resolutionObserver.ToList();
		threats = source.threatObserver.ToList();
		chapters = source.chapterObserver.ToList();
		collections = source.collectionObserver.ToList();
		globalTiles = source.globalTilePool.ToList();
		scenarioEndStatus = source.scenarioEndStatus;

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
		loreReward = source.loreReward;
		xpReward = source.xpReward;
		xpStartValue = source.xpStartValue;
	}

	/// <summary>
	/// Supply the FULL PATH with the filename
	/// </summary>
	public static Scenario LoadScenario( string filename )
	{
		string mydocs = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
		string basePath = Path.Combine( mydocs, "Your Journey" );
		if ( !Directory.Exists( basePath ) )
			Directory.CreateDirectory( basePath );

		try
		{
			Debug.Log("StreamReader#ReadToEnd()");
			string json = "";
			using ( StreamReader sr = new StreamReader( filename ) )
			{
				json = sr.ReadToEnd();
			}

			Debug.Log("JsonConvert#DeserializeObject()");
			ITraceWriter traceWriter = new MemoryTraceWriter();
			var fm = JsonConvert.DeserializeObject<FileManager>( json, new JsonSerializerSettings()
			{
				TraceWriter = traceWriter,
				Error = (sender, error) => {
					Debug.Log("Scenario deserialize error: " + error);
					Debug.Log(traceWriter);
					error.ErrorContext.Handled = true;
				}
			});

			Debug.Log("Scenario#CreateInstance()");
			return Scenario.CreateInstance( fm );
		}
		catch(Exception e)
		{
			Debug.Log("LoadScenario exception: " + e.ToString());
			return null;
		}
	}

	/// <summary>
	/// Return ProjectItem info for Scenarios in Project folder
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
			Scenario s = LoadScenario( fi.FullName );
			if ( s != null )
				items.Add( new ProjectItem()
				{
					Title = s.scenarioName,
					projectType = s.projectType,
					Date = s.saveDate,
					fileName = fi.Name,
					fileVersion = s.fileVersion,
					collections = s.collectionObserver.ToList() //string.Join(" ", s.collectionObserver.Select(c=> Collection.FromID(c).FontCharacter))
				} );
		}
		return items;
	}

	/// <summary>
	/// Return ProjectItem info for Campaigns in Project folder
	/// </summary>
	public static IEnumerable<ProjectItem> GetCampaigns()
	{
		string basePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "Your Journey" );

		//make sure the project folder exists
		if ( !Directory.Exists( basePath ) )
		{
			var dinfo = Directory.CreateDirectory( basePath );
			if ( dinfo == null )
			{
				return null;
			}
		}

		List<ProjectItem> items = new List<ProjectItem>();
		DirectoryInfo di = new DirectoryInfo( basePath );
		FileInfo[] files = di.GetFiles().Where( file => file.Extension == ".jime" ).ToArray();
		//find campaigns
		foreach ( DirectoryInfo dInfo in di.GetDirectories() )
		{
			Campaign c = LoadCampaign( dInfo.Name );
			if ( c != null )
			{
				FileInfo fi = new FileInfo( Path.Combine( basePath, dInfo.Name, dInfo.Name + ".json" ) );
				ProjectItem pi = new ProjectItem();
				pi.projectType = ProjectType.Campaign;
				pi.Date = fi.LastWriteTime.ToString( "M/d/yyyy" );
				pi.Title = c.campaignName;
				pi.campaignDescription = c.description;
				pi.campaignGUID = dInfo.Name;
				pi.campaignStory = c.storyText;
				pi.fileVersion = c.fileVersion;
				pi.fileName = fi.FullName;
				pi.collections = new List<int>();

				int scIndex = 0;
				foreach ( CampaignItem item in c.scenarioCollection)
                {
					var scenario = FileManager.LoadScenario(FileManager.GetFullPathWithCampaign(item.fileName, pi.campaignGUID.ToString()));
					//TODO collections in CampaignScreen?
                    //c.scenarioCollection[scIndex].collections = scenario.collectionObserver.ToList();
					foreach ( int col in scenario.collectionObserver )
                    {
						if(!pi.collections.Contains( col)) { pi.collections.Add( col ); }
                    }
					scIndex++;
                }
				pi.collections.Sort();
				c.collections = pi.collections;

				items.Add( pi );
			}
			//var scenario = FileManager.LoadScenario(FileManager.GetFullPathWithCampaign(gameStarter.scenarioFileName, campaign.campaignGUID.ToString()));

		}

		return items;
	}

	public static Campaign LoadCampaign( string campaignGUID )
	{
		if ( campaignGUID == "Saves" )
			return null;

		string basePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "Your Journey", campaignGUID );
		string json = "";
		try
		{
			using ( StreamReader sr = new StreamReader( Path.Combine( basePath, campaignGUID + ".json" ) ) )
			{
				json = sr.ReadToEnd();
			}

			var c = JsonConvert.DeserializeObject<Campaign>( json );
			return c;
		}
		catch
		{
			return null;
		}
	}

	/// <summary>
	/// unzip Campaigns into folders using GUID as name
	/// </summary>
	public static void UnpackCampaigns()
	{
		string basePath = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "Your Journey" );
		if ( !Directory.Exists( basePath ) )
			Directory.CreateDirectory( basePath );

		DirectoryInfo di = new DirectoryInfo( basePath );
		//zip files only
		FileInfo[] files = di.GetFiles().Where( x => x.Extension == ".zip" ).ToArray();

		try
		{
			foreach ( FileInfo fi in files )
			{
				using ( ZipArchive archive = ZipFile.OpenRead( fi.FullName ) )
				{
					//UnityEngine.Debug.Log( "UNZIPPING: " + fi.FullName );
					//get the campain's metadata first
					var meta = archive.Entries.Where( x => x.FullName.EndsWith( ".json" ) ).First();
					//get the GUID
					string campaignGUID = meta.Name.Replace( ".json", "" );
					//UnityEngine.Debug.Log( "CAMPAIGN GUID: " + campaignGUID );
					//create campaign GUID folder if it doesn't exist
					DirectoryInfo extractPath = new DirectoryInfo( Path.Combine( basePath, campaignGUID ) );
					if ( !extractPath.Exists )
					{
						Directory.CreateDirectory( Path.Combine( basePath, campaignGUID ) );
					}
					//unzip into campaign folder
					foreach ( ZipArchiveEntry entry in archive.Entries )
					{
						//Gets the full path to ensure that relative segments are removed
						string destinationPath = Path.GetFullPath( Path.Combine( extractPath.FullName, entry.FullName ) );

						//Ordinal match is safest, case-sensitive volumes can be mounted within volumes that are case-insensitive
						if ( destinationPath.StartsWith( extractPath.FullName, StringComparison.Ordinal ) )
							entry.ExtractToFile( destinationPath );

						//UnityEngine.Debug.Log( entry.Name );
						//UnityEngine.Debug.Log( "destinationPath: " + destinationPath );
						//UnityEngine.Debug.Log( "extractPath: " + extractPath.FullName );
					}
				}
			}
		}
		catch ( Exception e )
		{
			UnityEngine.Debug.Log( "UnpackCampaigns() ERROR: " + e.Message );
		}
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

	public static string GetFullPathWithCampaign( string filename, string campaignGUID )
	{
		string mydocs = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
		string basePath = Path.Combine( mydocs, "Your Journey", campaignGUID, filename );

		return basePath;
	}
}
