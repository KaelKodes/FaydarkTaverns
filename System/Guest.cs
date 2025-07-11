using Godot;
using System;
using FaydarkTaverns.Objects;

public enum GuestLocation
{
	InTown = 0,         // Not in tavern flow
	StreetOutside = 1,  // Waiting outside
	TavernFloor = 2,    // On the tavern floor (standing)
	TableBase = 20,     // 2.2+ = Table ID
	Staging = 3,        // Assigned to a quest (waiting to depart)
	DeployedBase = 40   // 4.x = Deployed to quest ID
}

public enum Gender
{
	Male,
	Female
}

public class Guest
{
	public string Name;

	public Gender Gender;
	public int PortraitId { get; set; }

	// ✅ Unified NPC reference
	public NPCData BoundNPC { get; set; }

	// ✅ Role helpers
	public bool IsAdventurer => BoundNPC?.Role == NPCRole.Adventurer;
	public bool IsQuestGiver => BoundNPC?.Role == NPCRole.QuestGiver;

	public int VisitDay;
	public int VisitHour;
	public int WaitDuration;   // How long they'll wait outside
	public int StayDuration;   // How long they stay once inside

	public bool IsInside = false;
	public bool IsOnStreet = false;
	public bool IsElsewhere = false;
	public bool IsOnQuest { get; set; } = false;
	public bool IsAssignedToQuest { get; set; } = false;

	public int? SeatIndex { get; set; } = null;
	public Table AssignedTable { get; set; } = null;

	public DateTime LastSeatCheck = DateTime.MinValue;
	public DateTime? DepartureTime { get; set; } // Nullable

	public int LocationCode { get; set; } = (int)GuestLocation.InTown;

	public event Action OnAdmitted;

	public void Admit()
	{
		IsInside = true;

		int baseStay = GD.RandRange(2, 3);
		int seatBonus = AssignedTable != null ? 1 : 0;
		int renownBonus = Mathf.Clamp(TavernStats.Instance.Renown / 50, 0, 3);

		StayDuration = baseStay + seatBonus + renownBonus;
		DepartureTime = ClockManager.CurrentTime.AddHours(StayDuration);

		LocationCode = (int)GuestLocation.TavernFloor;
		OnAdmitted?.Invoke();
	}

	public override string ToString()
	{
		return $"[GUEST:{Name}@{GetHashCode()}]";
	}

	public void SetLocation(bool inside, bool onStreet, bool elsewhere)
	{
		IsInside = inside;
		IsOnStreet = onStreet;
		IsElsewhere = elsewhere;
	}
}
