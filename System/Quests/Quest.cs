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
	public int TravelTimeTU;
	public int TaskTimeTU;
	public int DeadlineTU;      // Total time allowed
	public int StartTimeTU;     // When assigned (gameTime in TUs)
	public int ExpectedReturnTU;
	public bool IsOverdue => Assigned && GetElapsedTU() > DeadlineTU;

	// Party assignment
	public List<Adventurer> AssignedParty = new();
	public bool Assigned => AssignedParty.Count > 0;
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
		return TavernManager.CurrentTU - StartTimeTU;
	}

	public int GetTotalExpectedTU()
	{
		int total = TravelTimeTU * 2 + TaskTimeTU;
		total -= CalculatePartyBonus(); // e.g. Bard or synergy bonus
		return Math.Max(1, total);
	}

	private int CalculatePartyBonus()
	{
		int bonus = 0;

		if (AssignedParty.Exists(a => a.ClassName == "Bard"))
			bonus += (int)(TravelTimeTU * 0.1); // 10% faster travel

		var uniqueRoles = new HashSet<int>();
		foreach (var a in AssignedParty)
			uniqueRoles.Add(a.RoleId);

		if (uniqueRoles.Count >= 3)
			bonus += (int)(TaskTimeTU * 0.05); // 5% bonus for good synergy

		return bonus;
	}

	public void Accept()
	{
		if (AssignedAdventurers.Count == 0) return;

		IsAccepted = true;
		IsLocked = true;
		StartTimeTU = TavernManager.CurrentTU;
		ExpectedReturnTU = StartTimeTU + GetTotalExpectedTU();
	}
}
