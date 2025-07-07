using Godot;
using System;
using System.Collections.Generic;

public partial class GuestManager : Node
{
	public static List<Guest> guestsOutside = new();
	private static List<Guest> guestsInside = new();
	public int CurrentDay => (ClockManager.CurrentTime - ClockManager.GameStartTime).Days + 1;
	private int MaxSeats => TavernManager.TotalAvailableSeats();
	public int GuestsInsideCount() => guestsInside.Count;
	public int GuestsOutsideCount() => guestsOutside.Count;
	public static GuestManager Instance { get; private set; }
	private bool hasAnnouncedOpen = false;

	
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

public static void QueueGuest(Guest guest)
{
	guest.SetLocation(inside: false, onStreet: true, elsewhere: false);
	GuestManager.guestsOutside.Add(guest);
	GameLog.Debug($"🚶 {guest.Name} is walking by.");
}



public void TickGuests(DateTime currentTime)
{
	// ⛔ Prevent logic if game is paused
	if (ClockManager.TimeMultiplier == 0f)
		return;

	int currentHour = currentTime.Hour;

	// 🌞 Tavern opens at exactly 06:00 — announce once
	if (currentHour == 6 && !hasAnnouncedOpen)
	{
		GameLog.Info("🌞 The tavern is now open for the day!");
		hasAnnouncedOpen = true;
	}

	// 🍻 Last Call warning (fires once at 02:00)
	if (currentHour == 2)
		GameLog.Info("🍻 Last Call! The tavern will close at 03:00.");

	// 🚪 Close the tavern at exactly 03:00 and remove all guests
	if (currentHour == 3)
	{
		for (int i = guestsInside.Count - 1; i >= 0; i--)
		{
			var guest = guestsInside[i];
			RemoveGuest(guest);
		}

		GameLog.Info("🚪 The tavern has closed for the day.");
		hasAnnouncedOpen = false; // Reset for next morning
		return;
	}

	// ✅ Try to admit or remove guests from the street
	for (int i = guestsOutside.Count - 1; i >= 0; i--)
	{
		var guest = guestsOutside[i];

		bool itIsTheirDay = guest.VisitDay <= ClockManager.CurrentDay;
		bool itIsTimeToEnter = ClockManager.CurrentTime.Hour >= guest.VisitHour;
		bool theyHaveWaitedTooLong = ClockManager.CurrentTime.Hour > guest.VisitHour + guest.WaitDuration;

		if (itIsTheirDay && itIsTimeToEnter)
		{
			int standingGuests = TavernManager.Instance.GetGuestsInside().Count;
			if (standingGuests < TavernManager.Instance.MaxFloorGuests)
			{
				AdmitGuest(guest); // handles removal
			}
			else
			{
				GameLog.Debug($"⛔ Someone tries to enter... but the tavern floor is full.");
			}
		}
		else if (itIsTheirDay && theyHaveWaitedTooLong)
		{
			guestsOutside.RemoveAt(i);
			guest.SetLocation(false, false, true); // Elsewhere
			GameLog.Info($"😞 {guest.Name} has left the area.");
		}
	}

	// ✅ Remove guests who overstayed their floor/table time
	for (int i = guestsInside.Count - 1; i >= 0; i--)
	{
		var guest = guestsInside[i];

		if (guest.DepartureTime.HasValue && ClockManager.CurrentTime >= guest.DepartureTime.Value)
		{
			RemoveGuest(guest);
		}
	}
}




	public void AdmitGuest(Guest guest)
{
	guest.SetLocation(inside: true, onStreet: false, elsewhere: false);
	guest.Admit(); // handles DepartureTime, OnAdmitted event

	TavernManager.Instance.AdmitGuestToTavern(guest);
guestsOutside.Remove(guest);

if (!guestsInside.Contains(guest))
	guestsInside.Add(guest);

}



	public void RemoveGuest(Guest guest)
{
	if (guest == null)
		return;

	// 🪑 Remove from table if seated
	if (guest.AssignedTable != null)
	{
		guest.AssignedTable.RemoveGuest(guest);
		guest.AssignedTable = null;
		guest.SeatIndex = null;
	}

	// 📦 Remove from inside list
	if (guestsInside.Contains(guest))
		guestsInside.Remove(guest);
		guest.DepartureTime = null;
		guest.StayDuration = 0;


	// 🎯 Tell TavernManager this guest left
	TavernManager.Instance.OnGuestRemoved(guest);

	// 🔁 Queue to return another day
	QueueGuest(guest);
	guest.SetLocation(false, false, true); // Marks them as Elsewhere

	GameLog.Info($"😞 {guest.Name} has left the tavern.");

	// 🔄 Refresh visuals
	TavernManager.Instance.DisplayAdventurers();
	TavernManager.Instance.UpdateFloorLabel();
}

// Guest Spawning
public static Guest SpawnNewAdventurer(string className, string race = "Human", int level = 1)
{
	var template = ClassTemplate.GetTemplateByName(className);
	if (template == null)
	{
		GameLog.Debug($"❌ Invalid class name: {className}");
		return null;
	}

	var adventurer = AdventurerGenerator.GenerateAdventurer(level, template);
	var guest = new Guest
	{
		Name = adventurer.Name,
		IsAdventurer = true,
		VisitDay = ClockManager.CurrentDay,
		VisitHour = GD.RandRange(6, 18),
		WaitDuration = GD.RandRange(1, 2),
		StayDuration = GD.RandRange(4, 8),
		BoundAdventurer = adventurer
	};

	GameLog.Debug($"🧙 Spawned Adventurer: {guest.Name} ({className})");
	return guest;
}

public static Guest SpawnNewInformant()
{
	string name = AdventurerGenerator.GenerateName();
	var guest = new Guest
	{
		Name = name,
		IsAdventurer = false,
		VisitDay = ClockManager.CurrentDay,
		VisitHour = GD.RandRange(6, 18),
		WaitDuration = GD.RandRange(1, 2),
		StayDuration = GD.RandRange(4, 8)
	};

	guest.BoundGiver = new QuestGiver(name, guest);

	GameLog.Debug($"📜 Spawned Informant: {name}");
	return guest;
}



}
