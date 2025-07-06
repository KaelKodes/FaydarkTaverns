using Godot;
using System;

public class InformantManager
{
	private static InformantManager _instance;
	public static InformantManager Instance => _instance ??= new InformantManager();

	private int informantHappiness = 0; // Range -100 to +100, 0 is neutral

	public void LowerHappiness(int amount = 5)
{
	informantHappiness = Math.Max(-100, informantHappiness - amount);
	GameLog.Info($"😠 Informant happiness reduced to {informantHappiness}");
}

public void RaiseHappiness(int amount = 5)
{
	informantHappiness = Math.Min(100, informantHappiness + amount);
	GameLog.Info($"😊 Informant happiness increased to {informantHappiness}");
}


	public int GetHappiness() => informantHappiness;
}
