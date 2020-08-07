using System;
using UnityEngine;

public class MetaData : MonoBehaviour
{
	public string triggerName;
	public string interactionName;
	public string triggeredByName;
	public TokenType tokenType;
	//public string tokenTypeID;
	public Vector3 position { get; set; }
	public Vector3 offset;
	public Guid GUID;
	public bool isRandom;
}

