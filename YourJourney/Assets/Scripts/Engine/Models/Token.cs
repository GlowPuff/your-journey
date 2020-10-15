using System;
using UnityEngine;
using Newtonsoft.Json;
using System.ComponentModel;

public class Token
{
	public string dataName { get; set; }
	public Guid GUID { get; set; }
	public bool isEmpty { get; set; }
	public string triggerName { get; set; }
	public string triggeredByName { get; set; }

	[DefaultValue( TokenType.Search )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public TokenType tokenType { get; set; }

	[DefaultValue( PersonType.Human )]
	[JsonProperty( DefaultValueHandling = DefaultValueHandling.Populate )]
	public PersonType personType { get; set; }

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

	public Vector3 vposition { get; set; }
}
