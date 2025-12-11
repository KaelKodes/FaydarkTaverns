using System;
using System.Collections.Generic;
using FaydarkTaverns.Objects;


[Serializable]
public class SaveData
{
	public MetaData Meta;
	public WorldData World;
	public TavernData Tavern;
	public PlayerData Player;
	public GuestDataBlock Guests;
	public QuestDataBlock Quests;
	public JournalData Journal;
}

/* ========== WORLD / TIME ========== */

[Serializable]
public class WorldData
{
	public string CurrentTimeISO;     // stores DateTime reliably
	public float TimeMultiplier;
	public int LastKnownDay;          // for debugging or future features
}


/* ========== TAVERN ========== */

[Serializable]
public class TavernData
{
	// Party
	public bool HasSpawnedInitialParty = false;

	// Economy
	public int Gold;

	// Tavern progression (from TavernStats)
	public int Level;
	public int Exp;
	public int Renown;
	public int MaxFloorGuests;
	public int TavernSignLevel;

	// Upgrades
	public Dictionary<string, int> TableCaps;
	public HashSet<string> UnlockedUpgrades;
	public Dictionary<string, int> UpgradeCounts;

	// Event stats
	public int AcceptedQuests;
	public int CompletedQuests;
	public int FailedQuests;
	public int AdventurersMet;
	public int InformantsMet;

	// Shop purchases
	public Dictionary<string, int> PurchasedItems;

	// Optional: tavern name
	public string TavernName;
}


/* ========== PLAYER / INVENTORY ========== */

[Serializable]
public class PlayerData
{
	public int Gold;
	public Dictionary<string, int> Ingredients; // PlayerPantry.Ingredients
	public Dictionary<string, int> Supplies;    // PlayerPantry.Supplies
}

/* ========== GUESTS / NPCS ========== */
[Serializable]
public class GuestDataBlock
{
	public List<GuestSaveData> Guests;
}

[Serializable]
public class GuestSaveData
{
	// NPC identity
	public string NpcId;
	public NPCData Npc;         // full snapshot of NPCData

	// Guest wrapper identity
	public string Name;
	public Gender Gender;
	public int PortraitId;

	// Time + state
	public NPCState CurrentState;
	public int VisitDay;
	public int VisitHour;
	public int WaitDuration;
	public int StayDuration;

	// Simple flags
	public bool HasQuest;
	public bool HasEaten;

	// Seating (we won’t restore tables yet, but we keep the info)
	public int? SeatIndex;

	// Departure time
	public string DepartureTimeISO; // null if none

	// Behavior tuning
	public float EntryPatience;
	public float TavernLingerTime;
	public float SeatRetryInterval;
	public float SocializeDuration;
}



[Serializable]
public class NPCSaveData
{
	public string ID;

	public string Name;
	public string Role;         // Adventurer, Informant, etc.
	public string Class;        // e.g. "Warrior", "Rogue"
	public int Level;

	public bool Favorite;

	public string CurrentState; // roaming, seated, leaving, etc.
	public int TableIndex;      // -1 if not seated

	public float Happiness;
	public float Hunger;

	// Quest-related links
	public string ActiveQuestId;
	public int ActiveQuestProgress;
}

/* ========== QUESTS ========== */

[Serializable]
public class QuestDataBlock
{
	public int MaxQuestSlots;
	public int NextQuestId;

	public List<QuestSaveData> ActiveQuests;
	public List<QuestSaveData> CompletedQuests;

	public QuestStatisticsData Stats;
}


[Serializable]
public class QuestStatisticsData
{
	public int Attempts;
	public int Successes;
	public int Failures;

	public int GoldEarned;
	public int HighestPayout;

	public Dictionary<string, int> QuestTypeCount;
	public Dictionary<string, int> QuestGiverCount;
}

[Serializable]
public class QuestSaveData
{
	public int QuestId;
	public string Title;
	public string Region;
	public string Type;  // enum as string
	public int Reward;

	public bool IsAccepted;
	public bool IsComplete;
	public bool Failed;

	// CHANGE THESE TO STRING:
	public List<string> AssignedAdventurerIDs; // NPCData.Id values
	public string PostedByNPCId;              // may be null
}



[Serializable]
public class ActiveQuestSave
{
	public string QuestID;
	public int Progress;
	public string Status; // "active", "ready_to_turn_in", etc.
}

/* ========== JOURNAL ========== */

[Serializable]
public class JournalData
{
	public bool IsUnlocked;
	public List<string> DiscoveredLore; // IDs or keys for lore entries
}
