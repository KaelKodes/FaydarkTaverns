using Godot;
using System;

public enum GuestLocation
{
	InTown = 0,         // Not in tavern flow
	StreetOutside = 1,  // Waiting outside
	TavernFloor = 2,    // On the tavern floor (standing)
	TableBase = 20,     // 2.2+ = Table ID
	Staging = 3,        // Assigned to a quest (waiting to depart)
	DeployedBase = 40   // 4.x = Deployed to quest ID
}

// ─── New top-level enum for Gender ─────────────────────────────────────
public enum Gender
{
	Male,
	Female
}

public class Guest
{
	public string Name;

	// ─── New property to track each guest’s gender ──────────────────────
	public Gender Gender;
	public int PortraitId { get; set; }


	public bool IsAdventurer;
	public Adventurer BoundAdventurer { get; set; }
	public QuestGiver BoundGiver { get; set; } = null;

	public int VisitDay;
	public int VisitHour;
	public int WaitDuration;   // How long they'll wait outside
	public int StayDuration;   // How long they stay once inside

	public bool IsInside = false;                // Actively inside the tavern
	public bool IsOnStreet = false;              // Waiting outside
	public bool IsElsewhere = false;             // Out of scope (in town, home, etc.)
	public bool IsOnQuest { get; set; } = false;
	public bool IsAssignedToQuest { get; set; } = false;

	public int? SeatIndex { get; set; } = null;
	public Table AssignedTable { get; set; } = null;

	public DateTime LastSeatCheck = DateTime.MinValue;
	public DateTime? DepartureTime { get; set; } // Nullable

	public int LocationCode { get; set; } = (int)GuestLocation.InTown;

	public event Action OnAdmitted;

	/// Call this method when the guest is admitted to the tavern.
	public void Admit()
	{
		IsInside = true;

		// 🕒 Calculate dynamic stay duration
		int baseStay    = GD.RandRange(2, 3);
		int seatBonus   = AssignedTable != null ? 1 : 0;
		int renownBonus = Mathf.Clamp(TavernManager.Instance.Renown / 50, 0, 3);

		StayDuration   = baseStay + seatBonus + renownBonus;
		DepartureTime  = ClockManager.CurrentTime.AddHours(StayDuration);

		LocationCode   = (int)GuestLocation.TavernFloor;
		OnAdmitted?.Invoke();
	}

	public override string ToString()
	{
		return $"[GUEST:{Name}@{GetHashCode()}]";
	}
	public void SetLocation(bool inside, bool onStreet, bool elsewhere)
	{
		IsInside   = inside;
		IsOnStreet = onStreet;
		IsElsewhere= elsewhere;
	}

}
