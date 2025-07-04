using Godot;
using System;
using System.Collections.Generic;

public partial class GuestManager : Node
{
	public static GuestManager Instance;

	private List<Guest> guestsOutside = new();
	private List<Guest> guestsInside = new();
	public int CurrentDay => (ClockManager.Instance.CurrentTime - ClockManager.Instance.GameStartTime).Days + 1;

	private int MaxSeats => TavernManager.Instance.TotalAvailableSeats();

	public override void _Ready()
{
	Instance = this;

	var timer = new Timer
	{
		WaitTime = 5.0f,
		Autostart = true,
		OneShot = false
	};
	AddChild(timer);
	timer.Timeout += () => TickGuests(ClockManager.Instance.CurrentTime);
}


	public void QueueGuest(Guest guest)
	{
		guestsOutside.Add(guest);
		GameLog.Debug($"🚶 Guest queued: {guest.Name} ({(guest.IsAdventurer ? "Adventurer" : "QuestGiver")})");
	}

	public void TickGuests(DateTime currentTime)
	{
		// Try to admit guests
		for (int i = guestsOutside.Count - 1; i >= 0; i--)
		{
			var guest = guestsOutside[i];

			if (guest.VisitDay <= ClockManager.Instance.CurrentDay &&
				guest.VisitHour <= ClockManager.Instance.CurrentTime.Hour &&
				guestsInside.Count < MaxSeats)
			{
				AdmitGuest(guest);
				var tables = TavernManager.Instance.GetAvailableTables();

foreach (var table in tables)
{
	if (table.HasFreeSeat())
	{
		table.AssignGuest(guest);
		break;
	}
}

				guestsOutside.RemoveAt(i);
			}
			else if (guest.VisitHour + guest.WaitDuration < ClockManager.Instance.CurrentTime.Hour)
			{
				guestsOutside.RemoveAt(i);
				GameLog.Info($"😞 {guest.Name} left after waiting.");
			}
		}

		// Remove inside guests who overstayed
		for (int i = guestsInside.Count - 1; i >= 0; i--)
		{
			var guest = guestsInside[i];
			guest.StayDuration--;

			if (guest.StayDuration <= 0)
			{
				RemoveGuest(guest);
			}
		}
	}

	private void AdmitGuest(Guest guest)
	{
		guestsInside.Add(guest);
		guest.IsInside = true;
		GameLog.Info($"🍺 {guest.Name} has entered the tavern!");
		// TODO: Assign to table slot
	}

	private void RemoveGuest(Guest guest)
{
	guestsInside.Remove(guest);
	GameLog.Info($"👋 {guest.Name} has left the tavern.");

	// ✅ Free their table seat
	if (guest.AssignedTable != null)
	{
		guest.AssignedTable.RemoveGuest(guest);
	}
}


	public int GuestsInsideCount() => guestsInside.Count;
	public int GuestsOutsideCount() => guestsOutside.Count;
}
