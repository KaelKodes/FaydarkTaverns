using Godot;
using System;
using FaydarkTaverns.Objects;

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
	public bool IsInside => CurrentState is NPCState.TavernFloor or NPCState.Seats or NPCState.Lodging;
	public bool IsOnStreet => CurrentState == NPCState.StreetOutside;
	public bool IsElsewhere => CurrentState == NPCState.Elsewhere;
	public bool IsOnQuest => CurrentState == NPCState.Deployed;
	public bool IsAssignedToQuest => CurrentState is NPCState.StagingArea or NPCState.AssignedToQuest;
	public bool IsAdmitted => CurrentState is NPCState.TavernFloor or NPCState.Seats or NPCState.Lodging;
	public bool IsActive => CurrentState != NPCState.Elsewhere;
	public bool IsSeated => AssignedTable != null;
	public bool ShouldLeave => DepartureTime.HasValue && ClockManager.CurrentTime >= DepartureTime.Value;



	public bool HasQuest { get; set; } = false;
	public bool HasEaten { get; set; } = false;

	public int? SeatIndex { get; set; } = null;
	public Table AssignedTable { get; set; } = null;

	public DateTime LastSeatCheck = DateTime.MinValue;
	public DateTime? DepartureTime { get; set; } // Nullable
	
	public float EntryPatience { get; set; } // how long to wait outside
	public float TavernLingerTime { get; set; } // how long they stay
	public float SeatRetryInterval { get; set; } // how often they retry finding a seat
	public float SocializeDuration { get; set; } // optional social cap



	public NPCState CurrentState { get; set; } = NPCState.Elsewhere;


	public override string ToString()
	{
		return $"[GUEST:{Name}@{GetHashCode()}]";
	}

public void SetState(NPCState newState)
{
	CurrentState = newState;
}


	
	public void GenerateQuest()
{
	HasQuest = true;
	// TODO: Actually create quest object and link it
}

public bool TryPostQuest()
{
	if (!HasQuest)
		return false;

	// TODO: Add to quest board or logic system
	GameLog.Debug($"{Name} posted a quest to the board.");
	HasQuest = false;
	return true;
}



}
