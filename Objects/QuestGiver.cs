using Godot;
using System;

public class QuestGiver
{
	public string Name;
	public int QuestsPosted = 0;
	public int QuestsCompleted = 0;
	public int QuestsFailed = 0;

	public int GoldGiven = 0;
	public int ExpGiven = 0;
	public int Level = 1;
	public int Xp = 0;
	public int XpToNext => Level * 100;

	public float Happiness = 1.0f; // ranges 0.0–2.0 for now
	public Quest ActiveQuest { get; set; } = null;


	// Link to Guest
	public Guest HostGuest;

	public QuestGiver(string name, Guest guest)
	{
		Name = name;
		HostGuest = guest;
	}
	public void GainXP(int amount)
{
	Xp += amount;
	while (Xp >= XpToNext)
	{
		Xp -= XpToNext;
		Level++;
		GameLog.Info($"📈 Quest Giver {Name} leveled up to {Level}!");
	}
}
}
