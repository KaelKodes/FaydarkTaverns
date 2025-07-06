using Godot;
using System;
using System.Collections.Generic;

public partial class GuestManager : Node
{
	private static List<Guest> guestsOutside = new();
	private static List<Guest> guestsInside = new();
	public int CurrentDay => (ClockManager.CurrentTime - ClockManager.GameStartTime).Days + 1;
	private int MaxSeats => TavernManager.TotalAvailableSeats();
	public int GuestsInsideCount() => guestsInside.Count;
	public int GuestsOutsideCount() => guestsOutside.Count;
	
	public override void _Ready()
{

	var timer = new Timer
	{
		WaitTime = 3.0f,
		Autostart = true,
		OneShot = false
	};
	AddChild(timer);
	timer.Timeout += () => TickGuests(ClockManager.CurrentTime);
}

private Guest GenerateQuestGiverGuest()
{
	string name = AdventurerGenerator.GenerateName();

	var guest = new Guest
	{
		Name = name,
		IsAdventurer = false,
		VisitDay = ClockManager.CurrentDay,
		VisitHour = GD.RandRange(6, 12),  // morning arrivals
		WaitDuration = 2,
		StayDuration = GD.RandRange(4, 6)
	};

	guest.BoundGiver = new QuestGiver(name, guest);

	GameLog.Debug($"🧓 Quest Giver '{name}' generated.");

	return guest;
}

	public static void QueueGuest(Guest guest, TavernManager tavern)
{
	guestsOutside.Add(guest);
	guest.IsInside = false;
}


	public void TickGuests(DateTime currentTime)
{
	// ⛔ Prevent logic if game is paused
	if (ClockManager.TimeMultiplier == 0f)
		return;
		
	// ✅ Try to admit guests
	for (int i = guestsOutside.Count - 1; i >= 0; i--)
	{
		var guest = guestsOutside[i];

		// ✅ Guest is ready to enter
		bool itIsTheirDay = guest.VisitDay <= ClockManager.CurrentDay;
bool itIsTimeToEnter = ClockManager.CurrentTime.Hour >= guest.VisitHour;
bool theyHaveWaitedTooLong = ClockManager.CurrentTime.Hour > guest.VisitHour + guest.WaitDuration;

if (itIsTheirDay && itIsTimeToEnter &&
	guestsInside.Count < TavernManager.Instance.MaxFloorGuests)
{
	AdmitGuest(guest);
	TavernManager.Instance.AdmitGuestToTavern(guest);
	guestsOutside.RemoveAt(i);
}
else if (itIsTheirDay && theyHaveWaitedTooLong)
{
	guestsOutside.RemoveAt(i);
	GameLog.Info($"😞 {guest.Name} left after waiting.");
}

	}

	// ✅ Remove guests who overstayed
	for (int i = guestsInside.Count - 1; i >= 0; i--)
	{
		var guest = guestsInside[i];
		

		if (guest.DepartureTime.HasValue && ClockManager.CurrentTime >= guest.DepartureTime.Value)
{
	RemoveGuest(guest);
}

	}
}


	public static void AdmitGuest(Guest guest)
{
	guestsInside.Add(guest);
	guest.Admit(); // Use the built-in method that triggers OnAdmitted

	GameLog.Info($"🍺 {guest.Name} has entered the tavern!");
}

	private void RemoveGuest(Guest guest)
{
	guestsInside.Remove(guest);
	GameLog.Info($"👋 {guest.Name} has left the tavern.");

	if (guest.AssignedTable != null)
	{
		guest.AssignedTable.RemoveGuest(guest);
	}

	// Remove from floor list
	TavernManager.Instance?.OnGuestRemoved(guest);
}
	
	public static List<Guest> GetGuestsInside()
{
	return guestsInside;
}


}
