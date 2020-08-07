using UnityEngine;

public class BattleTile : ITile
{
	public TileType tileType { get; set; }
	public int idNumber { get; set; }
	public Vector3 position { get; set; }
}