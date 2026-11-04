using System;
using System.Collections.Generic; // Для словаря
public class BuildingDef
{
	public string Id;
	public string NameKey;
	public Dictionary<string, double> Cost;
	public double Growth;
	public Dictionary<string, double> Income;
	public int Plot;
}