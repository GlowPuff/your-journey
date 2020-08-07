using System;
using System.Collections.ObjectModel;
using UnityEngine;

public class HexTile : ITile
{
	public float angle { get; set; }
	public int idNumber { get; set; }
	public int tokenCount { get; set; }
	public Guid GUID { get; set; }
	public string position
	{
		get { return vposition.ToString(); }
		set
		{
			string[] s = value.Split( ',' );
			vposition = new Vector3( float.Parse( s[0] ), 0, float.Parse( s[1] ) );
		}
	}
	public TileType tileType { get; set; }
	public string tileSide { get; set; }
	public TextBookData flavorBookData { get; set; }
	public ObservableCollection<Token> tokenList { get; set; }
	public string hexRoot { get; set; }
	public Vector3 vposition { get; set; }
	public bool isStartTile { get; set; }

	public HexTile()
	{

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
	}
}