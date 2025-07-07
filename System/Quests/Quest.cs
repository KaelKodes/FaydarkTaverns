using Godot;
using System;
using System.Collections.Generic;

public enum QuestType
{
	Slay, Escort, Heist, Explore, Tame, Rescue, Research, Treasure
}

public enum Region
{
	Frostmere, Ironhold, Willowbank, Mossvale,
	Brinemarsh, Ravenmoor, Sundrift, AshenRuins, HowlingPlains
}

public class Quest
{
	public int QuestId;
	public string Title;
	public QuestType Type;
	public Region Region;
	public int Reward;

	// Timing
	public int TravelHours;
public int TaskHours;
public DateTime StartTime;
public DateTime ExpectedReturn;
public DateTime Deadline;

public TimeSpan Elapsed => ClockManager.CurrentTime - StartTime;
public bool IsOverdue => Assigned && ClockManager.CurrentTime > Deadline;
public DateTime LastSeatCheck = DateTime.MinValue;
public QuestGiver PostedBy { get; set; }





	// Party assignment
	public bool Assigned => AssignedAdventurers.Count > 0;
	public List<Adventurer> AssignedAdventurers = new();

	// Status
	public bool IsAccepted = false;
	public bool IsLocked = false;
	public bool IsComplete = false;
	public bool Failed = false;


	// Synergy / Role guidance
	public List<int> OptimalRoles = new(); // E.g. [1, 2, 4] for Tank/DPS/Healer

	// Narrative flavor
	public string Description;
	public string Quirk; // Optional – e.g., "In a rush", "Very picky", etc.

	// Calculate elapsed time
	public int GetElapsedTU()
{
	return (int)(Elapsed.TotalHours);
}



	public int GetTotalExpectedTU()
{
	int total = TravelHours * 2 + TaskHours;
	total -= CalculatePartyBonus(); // e.g. Bard or synergy bonus
	return Math.Max(1, total); // Always return at least 1 hour
}


	private int CalculatePartyBonus()
{
	int bonus = 0;

	if (AssignedAdventurers.Exists(a => a.ClassName == "Bard"))
		bonus += (int)(TravelHours * 0.1); // 10% faster travel

	var uniqueRoles = new HashSet<int>();
	foreach (var a in AssignedAdventurers)
		uniqueRoles.Add(a.RoleId);

	if (uniqueRoles.Count >= 3)
		bonus += (int)(TaskHours * 0.05); // 5% bonus for good synergy

	return bonus;
}


public void Accept()
{
	if (AssignedAdventurers.Count == 0)
	{
		GameLog.Info($"❌ Attempted to accept Quest {QuestId}, but has no adventurers.");
		return;
	}

	IsAccepted = true;
	IsLocked = true;
	StartTime = ClockManager.CurrentTime;
	ExpectedReturn = StartTime.AddHours(GetTotalExpectedTU());

	int buffer = new Random().Next(2, 7); // Between 2–6 hours of slack
	Deadline = ExpectedReturn.AddHours(buffer);

	GameLog.Info($"📜 Quest Accepted: {Title}");
	GameLog.Debug($"⏳ Estimated Return: {ExpectedReturn:MMM dd, HH:mm}");
	GameLog.Debug($"🛑 Deadline (with buffer): {Deadline:MMM dd, HH:mm}");

	QuestManager.Instance?.NotifyQuestStateChanged(this);

	// ✅ Schedule Quest Completion
	TimerManager.Instance.ScheduleEvent(ExpectedReturn, () =>
	{
		GameLog.Info($"🏁 Quest Completed: {Title}");
		QuestManager.Instance.CompleteQuest(this);
	});

	// ✅ Schedule Deadline Enforcement
	TimerManager.Instance.ScheduleEvent(Deadline, () =>
	{
		if (!IsComplete)
		{
			GameLog.Info($"⏳ Quest Deadline Missed: {Title}");
			QuestManager.Instance.EnforceDeadline(this);
		}
	});
}
}
