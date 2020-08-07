using System;
using UnityEngine;

public class Token
{
	public string dataName { get; set; }
	public Guid GUID { get; set; }
	public bool isEmpty { get; set; }
	public string triggerName { get; set; }
	public string triggeredByName { get; set; }
	public TokenType tokenType { get; set; }
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
