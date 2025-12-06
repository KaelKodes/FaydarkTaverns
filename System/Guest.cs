using Godot;
using System;
using FaydarkTaverns.Objects;
using System.Threading.Tasks;


public enum Gender
{
	Male,
	Female
}

public class Guest
{
	// -----------------------------------------
	//  BASIC IDENTITY
	// -----------------------------------------
	public string Name;
	public Gender Gender;
	public int PortraitId { get; set; }

	// The NPC this Guest wraps
	public NPCData BoundNPC { get; set; }

	// -----------------------------------------
	//  ROLE HELPERS
	// -----------------------------------------
	public bool IsAdventurer => BoundNPC?.Role == NPCRole.Adventurer;
	public bool IsQuestGiver => BoundNPC?.Role == NPCRole.QuestGiver;

	// -----------------------------------------
	//  TIME & STATE METRICS
	// -----------------------------------------
	public int VisitDay;
	public int VisitHour;
	public int WaitDuration;
	public int StayDuration;

	public bool IsInside => CurrentState is NPCState.TavernFloor or NPCState.Seats or NPCState.Lodging;
	public bool IsOnStreet => CurrentState == NPCState.StreetOutside;
	public bool IsElsewhere => CurrentState == NPCState.Elsewhere;
	public bool IsOnQuest => CurrentState == NPCState.Deployed;
	public bool IsAssignedToQuest => CurrentState is NPCState.StagingArea or NPCState.AssignedToQuest;
	public bool IsAdmitted => CurrentState is NPCState.TavernFloor or NPCState.Seats or NPCState.Lodging;
	public bool IsActive => CurrentState != NPCState.Elsewhere;
	public bool IsSeated => AssignedTable != null;
	public bool ShouldLeave => DepartureTime.HasValue && ClockManager.CurrentTime >= DepartureTime.Value;

	// -----------------------------------------
	//  UI FORWARDING
	// -----------------------------------------
	public GuestCard Card { get; set; }

	public void ShowRequestBubble(bool visible)
	{
		Card?.ShowRequestBubble(visible);
	}

	public void ShowReaction(ConsumptionReaction reaction)
	{
		Card?.ShowReaction(reaction);
	}

	// -----------------------------------------
	//  REPEAT ORDER LOGIC
	// -----------------------------------------
	public async void ScheduleAnotherOrder()
	{
		// Guest is NOT a Node, so we cannot call GetTree()
		// Use a safe global helper (TimerManager or GameRoot)
		await TimerManager.WaitSeconds(2.0f);

		if (BoundNPC == null || Card == null)
			return;

		// Re-enable the appropriate need based on last consumption
		if (BoundNPC.LastConsumedWasFood)
		{
			BoundNPC.IsHungry = true;
			Card.ShowRequestBubble(true);
		}
		else if (BoundNPC.LastConsumedWasDrink)
		{
			BoundNPC.IsThirsty = true;
			Card.ShowRequestBubble(true);
		}
	}

	// -----------------------------------------
	//  QUEST FLAGS
	// -----------------------------------------
	public bool HasQuest { get; set; } = false;
	public bool HasEaten { get; set; } = false;

	// -----------------------------------------
	//  SEATING
	// -----------------------------------------
	public int? SeatIndex { get; set; } = null;
	public Table AssignedTable { get; set; } = null;

	public DateTime LastSeatCheck = DateTime.MinValue;
	public DateTime? DepartureTime { get; set; }

	public float EntryPatience { get; set; }
	public float TavernLingerTime { get; set; }
	public float SeatRetryInterval { get; set; }
	public float SocializeDuration { get; set; }

	// -----------------------------------------
	//  STATE MACHINE
	// -----------------------------------------
	public NPCState CurrentState { get; set; } = NPCState.Elsewhere;

	public void SetState(NPCState newState)
	{
		CurrentState = newState;
	}

	// -----------------------------------------
	//  QUEST STUBS
	// -----------------------------------------
	public void GenerateQuest()
	{
		HasQuest = true;
		// TODO: Generate actual quest
	}

	public bool TryPostQuest()
	{
		if (!HasQuest)
			return false;

		GameLog.Debug($"{Name} posted a quest to the board.");
		HasQuest = false;
		return true;
	}



	// -----------------------------------------
	//  DEBUG
	// -----------------------------------------
	public override string ToString()
	{
		return $"[GUEST:{Name}@{GetHashCode()}]";
	}
}
