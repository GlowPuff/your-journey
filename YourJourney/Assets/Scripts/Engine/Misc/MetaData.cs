﻿using System;
using UnityEngine;

public class MetaData : MonoBehaviour
{
	public string triggerName;
	public string interactionName;
	public string triggeredByName;
	public TokenType tokenType;
	public PersonType personType;
	public TerrainType terrainType;
	public Vector3 offset;
	public Vector3 size; //This is because I can't figure out how to get the ProBuilder object size from the script so I just manually entered them in the Prefabs/Terrain/ objects. Needed for rotating around the proper center point.
	public Guid GUID;
	public bool isRandom;
	public int tileID;
}

public class MetaDataJSON
{
	public string triggerName;
	public string interactionName;
	public string triggeredByName;
	public TokenType tokenType;
	public PersonType personType;
	public TerrainType terrainType;
	public Vector3 offset;
	public Guid GUID;
	public bool isRandom;
	public int tileID;

	public MetaDataJSON( MetaData md )
	{
		if ( md == null )
			return;
		triggerName = md.triggerName;
		interactionName = md.interactionName;
		triggeredByName = md.triggeredByName;
		tokenType = md.tokenType;
		personType = md.personType;
		terrainType = md.terrainType;
		offset = md.offset;
		GUID = md.GUID;
		isRandom = md.isRandom;
		tileID = md.tileID;
	}
}
