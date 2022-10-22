using System;
using UnityEngine;
using Newtonsoft.Json;
using System.ComponentModel;

public class Token
{
	public string dataName { get; set; }
	public Guid GUID { get; set; }
	public bool isEmpty { get; set; }
	public string triggerName { get; set; }//the event name this token fires
	public string triggeredByName { get; set; }//trigger that spawns this

	[DefaultValue( TokenType.Search )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public TokenType tokenType { get; set; }

	[DefaultValue( PersonType.Human )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public PersonType personType { get; set; }

	[DefaultValue(TerrainType.None)]
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
	public TerrainType terrainType { get; set; }

	public int idNumber { get; set; }
	public string position
	{
		get { return vposition.ToString(); }
		set
		{
			string[] s = value.Split( ',' );
			vposition = new Vector3( float.Parse( s[0] ), 0, float.Parse( s[1] ) );
		}
	}

	public double angle { get; set; }

	public Vector3 vposition { get; set; }
}
