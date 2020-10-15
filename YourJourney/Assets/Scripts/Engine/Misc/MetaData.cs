using System;
using Newtonsoft.Json;
using UnityEngine;

[JsonObject( MemberSerialization.OptIn )]
public class MetaData : MonoBehaviour
{
	[JsonProperty]
	public string triggerName;
	[JsonProperty]
	public string interactionName;
	[JsonProperty]
	public string triggeredByName;
	[JsonProperty]
	public TokenType tokenType;
	//public string tokenTypeID;
	//public Vector3 position { get; set; }
	[JsonProperty]
	public Vector3 offset { get; set; }
	[JsonProperty]
	public Guid GUID;
	[JsonProperty]
	public bool isRandom;
	[JsonIgnore]
	public Tile tile;
}

