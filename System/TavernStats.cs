using Godot;
using System.Collections.Generic;

public partial class TavernStats : Node
{
	// --- Singleton Pattern ---
	public static TavernStats Instance { get; private set; }

	// --- Persistent Tavern Stats ---
	public int Level = 1;
	public int Exp = 0;
	public int Renown = 0;
	public int MaxFloorGuests = 4;         // Current floor cap
	public int TavernSignLevel = 0;


	// --- Table Limits (can be expanded by upgrades) ---
	public Dictionary<string, int> TableCaps = new()
	{
		{ "Starting Table", 1 },
		{ "Tiny Table", 2 },
		{ "Small Table", 2 },
		{ "Medium Table", 2 },
		{ "Large Table", 2 }
	};

	// --- Upgrade Unlocks (future) ---
	public HashSet<string> UnlockedUpgrades = new();
	public Dictionary<string, int> UpgradeCounts = new();

	// --- Event Tracking Stats ---
	public int AcceptedQuests = 0;
	public int CompletedQuests = 0;
	public int FailedQuests = 0;
	public int AdventurersMet = 0;
	public int InformantsMet = 0;

	// --- EXP to Next Level calculation (you can make this smarter later) ---
	public int ExpToNextLevel => Level * 10;

	// --- Methods to Add XP, Renown, Level Up, etc ---
	public void AddExp(int amount)
	{
		Exp += amount;
		while (Exp >= ExpToNextLevel)
		{
			Exp -= ExpToNextLevel;
			Level++;
		}
	}

	public void AddRenown(int amount)
	{
		Renown += amount;
	}

	public override void _Ready()
	{
		if (Instance != null)
		{
			GD.PrintErr("❌ Multiple TavernStats detected!");
			QueueFree();
			return;
		}
		Instance = this;
	}
	
	// Save Load
	public TavernData ToData()
{
	return new TavernData
	{
		// Core stats
		Level = Level,
		Exp = Exp,
		Renown = Renown,
		MaxFloorGuests = MaxFloorGuests,
		TavernSignLevel = TavernSignLevel,

		// Dictionaries and sets
		TableCaps = new Dictionary<string, int>(TableCaps),
		UnlockedUpgrades = new HashSet<string>(UnlockedUpgrades),
		UpgradeCounts = new Dictionary<string, int>(UpgradeCounts),

		// Event stats
		AcceptedQuests = AcceptedQuests,
		CompletedQuests = CompletedQuests,
		FailedQuests = FailedQuests,
		AdventurersMet = AdventurersMet,
		InformantsMet = InformantsMet
	};
}

public void FromData(TavernData data)
{
	if (data == null)
		return;

	// Core stats
	Level = data.Level;
	Exp = data.Exp;
	Renown = data.Renown;
	MaxFloorGuests = data.MaxFloorGuests;
	TavernSignLevel = data.TavernSignLevel;

	// Dictionaries and sets
	TableCaps = new Dictionary<string, int>(data.TableCaps ?? new());
	UnlockedUpgrades = new HashSet<string>(data.UnlockedUpgrades ?? new());
	UpgradeCounts = new Dictionary<string, int>(data.UpgradeCounts ?? new());

	// Event stats
	AcceptedQuests = data.AcceptedQuests;
	CompletedQuests = data.CompletedQuests;
	FailedQuests = data.FailedQuests;
	AdventurersMet = data.AdventurersMet;
	InformantsMet = data.InformantsMet;
}


}
