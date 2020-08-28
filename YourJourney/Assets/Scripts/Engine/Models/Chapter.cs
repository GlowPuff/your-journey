﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public class Chapter
{
	public string dataName { get; set; }
	public Guid GUID { get; set; }
	public bool isEmpty { get; set; }
	public string triggerName { get; set; }
	public TextBookData flavorBookData { get; set; }
	public string triggeredBy { get; set; }
	public string exploreTrigger { get; set; }
	public bool isRandomTiles { get; set; }
	public bool noFlavorText { get; set; }

	[JsonConverter( typeof( TileConverter ) )]
	public List<ITile> tileObserver { get; set; }
	public List<int> randomTilePool { get; set; }
	public string randomInteractionGroup { get; set; }
	public int randomInteractionGroupCount { get; set; }
}
