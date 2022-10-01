using System;
using System.Collections.ObjectModel;
using System.Globalization;
using UnityEngine;

public class HexTile : BaseTile
{
	public HexTile()
	{
		tileType = TileType.Hex;
	}

	public HexTile( int n, Vector position, float angle )
	{
		tileType = TileType.Hex;
		idNumber = n;
		tokenCount = ( n / 100 ) % 10;
		GUID = Guid.NewGuid();
		tileSide = "A";
		vposition = position.ToVector3();
		this.angle = angle;
		tokenList = new ObservableCollection<Token>();
		flavorBookData = new TextBookData() { pages = new System.Collections.Generic.List<string>() { "" } };
		isStartTile = false;
		triggerName = "None";
	}
}