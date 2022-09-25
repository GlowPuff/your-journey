using System;

public class MonsterActivationItem
{
	public string dataName;
	public int id;
	public Ability negate;
	public bool[] valid = new bool[] { false, false, false };
	public int[] damage = new int[] { 0, 0, 0 }, fear = new int[] { 0, 0, 0 };
	public string text, effect;
}