using Godot;
using System;
using System.Collections.Generic;
using FaydarkTaverns.Objects;

public partial class GuestManager : Node
{
	// ========= STATIC / SINGLETON =========

	public static GuestManager Instance { get; private set; }

	public static List<Guest> GuestsOutside => guestsOutside;
	public static List<Guest> GuestsInside => guestsInside;

	private static List<Guest> guestsOutside = new();
	private static List<Guest> guestsInside = new();

	public List<Guest> AllKnownGuests { get; private set; } = new();

	public static event Action<Guest> OnGuestLeft;
	public static event Action<Guest> OnGuestAdmitted;

	// ========= PROPERTIES =========

	public int CurrentDay => (ClockManager.CurrentTime - ClockManager.GameStartTime).Days + 1;
	private int MaxSeats => TavernManager.TotalAvailableSeats();
	public int GuestsInsideCount() => guestsInside.Count;
	public int GuestsOutsideCount() => guestsOutside.Count;

	// Day flags
	private bool hasAnnouncedOpen = false;
	private bool hasAnnouncedLastCall = false;
	private bool hasClosedTavern = false;

	// ========= LIFECYCLE =========

	public override void _Ready()
	{
		// Safe singleton guard for reloads
		if (Instance != null && Instance != this && GodotObject.IsInstanceValid(Instance))
		{
			GD.PrintErr("❌ Duplicate GuestManager instance detected! Replacing old instance.");
		}

		Instance = this;

		// Periodic guest ticking
		var timer = new Timer
		{
			WaitTime = 3.0f,
			Autostart = true,
			OneShot = false
		};
		AddChild(timer);
		timer.Timeout += () => TickGuests(ClockManager.CurrentTime);
	}

	public override void _ExitTree()
	{
		if (Instance == this)
			Instance = null;
	}

	// ========= QUEUEING =========

	public static void QueueGuest(Guest guest)
	{
		if (Instance == null)
		{
			GameLog.Debug("⚠️ GuestManager.Instance is null. Cannot queue guest.");
			return;
		}

		Instance.QueueGuestInternal(guest);
	}

	private void QueueGuestInternal(Guest guest)
	{
		// Do not modify queues while restoring a save
		if (GameStateLoader.IsRestoring)
			return;

		if (guest == null)
			return;

		if (!guestsOutside.Contains(guest))
			guestsOutside.Add(guest);
	}

	// ========= TICK / DAILY FLOW =========

	public void TickGuests(DateTime currentTime)
	{
		// During restore, do NOT advance guest logic at all.
		if (GameStateLoader.IsRestoring)
			return;

		if (ClockManager.TimeMultiplier == 0f)
			return;

		int currentHour = currentTime.Hour;

		// 🌞 Tavern opens at exactly 06:00 — announce once
		if (currentHour == 6 && !hasAnnouncedOpen)
		{
			GameLog.Info("🌞 The tavern is now open for the day!");
			hasAnnouncedOpen = true;

			// 🔁 Reset flags for new day
			hasAnnouncedLastCall = false;
			hasClosedTavern = false;
			AssignDailyHungerAndThirst();
		}

		// 🍻 Last Call warning (once)
		if (currentHour == 2 && !hasAnnouncedLastCall)
		{
			GameLog.Info("🍻 Last Call! The tavern will close at 03:00.");
			hasAnnouncedLastCall = true;
		}

		// 🚪 Tavern closes at 03:00 (once)
		if (currentHour == 3 && !hasClosedTavern)
		{
			for (int i = guestsInside.Count - 1; i >= 0; i--)
			{
				var guest = guestsInside[i];
				RemoveGuest(guest); // Force kick — still valid
			}

			GameLog.Info("🚪 The tavern has closed for the day.");
			hasAnnouncedOpen = false;
			hasClosedTavern = true;
			return;
		}

		// 🚶 Move Elsewhere guests to StreetOutside at visit time
		for (int i = 0; i < guestsOutside.Count; i++)
		{
			var guest = guestsOutside[i];
			if (guest.CurrentState == NPCState.Elsewhere &&
				guest.VisitDay <= ClockManager.CurrentDay &&
				currentHour >= guest.VisitHour)
			{
				guest.SetState(NPCState.StreetOutside);
				if (guest.BoundNPC != null)
					guest.BoundNPC.State = guest.CurrentState;

				GameLog.Debug($"🚶 {guest.Name} arrived outside the tavern (VisitHour={guest.VisitHour}).");
			}
		}

		// ✅ Entry logic for StreetOutside guests
		for (int i = guestsOutside.Count - 1; i >= 0; i--)
		{
			var guest = guestsOutside[i];

			bool itIsTheirDay = guest.VisitDay <= ClockManager.CurrentDay;
			bool itIsTimeToEnter = currentHour >= guest.VisitHour;
			bool theyHaveWaitedTooLong = currentHour > guest.VisitHour + guest.WaitDuration;

			if (itIsTheirDay && itIsTimeToEnter)
			{
				TryAdmitGuest(guest);
			}
			else if (itIsTheirDay && theyHaveWaitedTooLong)
			{
				guestsOutside.RemoveAt(i);
				guest.SetState(NPCState.Elsewhere);
				if (guest.BoundNPC != null)
					guest.BoundNPC.State = guest.CurrentState;

				GameLog.Debug($"{guest.Name} waited too long and left the street.");
			}
		}

		// 🕓 Departure debug (leave handled by TimerManager)
		for (int i = guestsInside.Count - 1; i >= 0; i--)
		{
			var guest = guestsInside[i];

			if (!guest.DepartureTime.HasValue)
			{
				GameLog.Debug($"⚠️ {guest.Name} has no DepartureTime set.");
			}
		}
	}

	// ========= ADMISSION / REMOVAL =========

	private void AdmitGuest(Guest guest)
	{
		// Do not admit anyone while restoring a save
		if (GameStateLoader.IsRestoring)
			return;

		if (guest == null)
			return;

		// ⏳ Schedule departure
		guest.DepartureTime = ClockManager.CurrentTime.AddMinutes(guest.TavernLingerTime);
		TimerManager.Instance.ScheduleEvent(guest.DepartureTime.Value, () => Leave(guest));

		// 📣 Notify systems of pending admission
		OnGuestAdmitted?.Invoke(guest);

		// 🏠 Let TavernManager handle full admission flow
		TavernManager.Instance.AdmitGuestToTavern(guest);

		// 🧹 Update tracking lists
		guestsOutside.Remove(guest);

		if (!guestsInside.Contains(guest))
			guestsInside.Add(guest);
	}

	public void RemoveGuest(Guest guest)
	{
		// Do not forcibly remove guests while restoring – state comes from save
		if (GameStateLoader.IsRestoring)
			return;

		if (guest == null)
			return;

		GameLog.Info($"❌ Forcibly removing {guest.Name} from the tavern.");

		// 🪑 Unseat if needed
		if (guest.AssignedTable != null)
		{
			guest.AssignedTable.RemoveGuest(guest);
			guest.AssignedTable = null;
		}
		guest.SeatIndex = null;

		// 📦 Clean from lists (safe even if not present)
		guestsInside.Remove(guest);
		guestsOutside.Remove(guest);

		// ⏳ Reset lifecycle state
		guest.DepartureTime = null;
		guest.StayDuration = 0;
		guest.SetState(NPCState.Elsewhere);

		// 🔁 Guest will return another day
		QueueGuest(guest);

		// 🎯 Tell systems to clean up any UI or linked panels
		TavernManager.Instance.OnGuestRemoved(guest);
	}

	// Handles guests leaving on their own, not by user
	public void Leave(Guest guest)
	{
		// If a departure event fires during restore, ignore it; the save already encoded state.
		if (GameStateLoader.IsRestoring)
			return;

		if (guest == null)
			return;

		if (guestsInside.Contains(guest))
			guestsInside.Remove(guest);

		if (guestsOutside.Contains(guest))
			guestsOutside.Remove(guest);

		TavernManager.Instance.NotifyGuestLeft(guest);

		// 🪑 Remove from table
		if (guest.AssignedTable != null)
		{
			guest.AssignedTable.RemoveGuest(guest);
			guest.AssignedTable = null;
		}
		guest.SeatIndex = null;

		guest.DepartureTime = null;
		guest.SetState(NPCState.Elsewhere);
		GameLog.Debug($"✅ {guest.Name} left the tavern at {ClockManager.CurrentTime}. DepartureTime was {guest.DepartureTime}");

		OnGuestLeft?.Invoke(guest);

		// 🟢 Move the log to here, last
		GameLog.Info($"🚶 {guest.Name} heads home.");
	}

	private void TryAdmitGuest(Guest guest)
	{
		if (guest == null || guest.CurrentState != NPCState.StreetOutside)
			return;

		if (TavernManager.Instance.GetGuestsInside().Count >= TavernStats.Instance.MaxFloorGuests)
			return;

		AdmitGuest(guest); // ✅ Let the real admission logic handle everything
	}

	// ========= SPAWNING =========

	// Guest Spawning
	public static Guest SpawnNewNPC(NPCRole role, string className = "Warrior")
	{
		// Do not generate brand-new NPCs during restore – state should come from save.
		if (GameStateLoader.IsRestoring)
			return null;

		var npc = NPCFactory.CreateBaseNPC();
		npc.Role = role;

		// Set portrait now that role is known
		NPCFactory.AssignPortrait(npc);

		// Apply role-based stat logic
		switch (role)
		{
			case NPCRole.Adventurer:
				var template = ClassTemplate.GetTemplateByName(className);
				NPCFactory.AssignAdventurerStats(npc, template);
				break;

			case NPCRole.QuestGiver:
				NPCFactory.AssignInformantStats(npc);
				break;
		}

		// 📈 Calculate renown-based linger bonus
		float renownBoost = Mathf.Clamp(TavernStats.Instance.Renown / 100f, 0f, 0.5f);
		float adjustedLingerTime = npc.TavernLingerTime * (1f + renownBoost);

		var guest = new Guest
		{
			Name = npc.Name,
			Gender = (Gender)Enum.Parse(typeof(Gender), npc.Gender),
			VisitDay = ClockManager.CurrentDay,
			VisitHour = GD.RandRange(7, 22),
			WaitDuration = GD.RandRange(1, 2),
			StayDuration = GD.RandRange(4, 8),
			PortraitId = npc.PortraitId,
			BoundNPC = npc,

			EntryPatience = npc.EntryPatience,
			TavernLingerTime = adjustedLingerTime,
			SeatRetryInterval = npc.SeatRetryInterval,
			SocializeDuration = npc.SocializeDuration
		};

		GameLog.Debug($"🛋️ {guest.Name} will linger {adjustedLingerTime:F1} minutes (Renown={TavernStats.Instance.Renown})");

		guest.SetState(NPCState.Elsewhere);
		Instance?.AllKnownGuests.Add(guest);
		return guest;
	}

	// ========= DAILY HUNGER / THIRST =========

	private void AssignDailyHungerAndThirst()
	{
		if (TavernManager.Instance == null)
		{
			GD.PrintErr("❌ TavernManager.Instance not found!");
			return;
		}

		foreach (var guest in AllKnownGuests)
		{
			if (guest?.BoundNPC == null)
				continue;

			guest.BoundNPC.HasEatenToday = false;
			guest.BoundNPC.HasDrankToday = false;

			int roll = (int)(GD.Randi() % 100);

			if (roll < 45)
			{
				guest.BoundNPC.IsHungry = true;
				guest.BoundNPC.IsThirsty = false;
			}
			else if (roll < 90)
			{
				guest.BoundNPC.IsHungry = false;
				guest.BoundNPC.IsThirsty = true;
			}
			else
			{
				guest.BoundNPC.IsHungry = true;
				guest.BoundNPC.IsThirsty = true;
			}

			GameLog.Debug($"🍽️ {guest.Name} rolled: Hungry={guest.BoundNPC.IsHungry}, Thirsty={guest.BoundNPC.IsThirsty}");
		}
	}

	// ========= SAVE / LOAD =========

	public GuestDataBlock ToData()
	{
		var data = new GuestDataBlock
		{
			Guests = new List<GuestSaveData>()
		};

		foreach (var guest in AllKnownGuests)
		{
			if (guest != null)
				data.Guests.Add(ConvertGuestToSave(guest));
		}

		GD.Print($"[GuestManager] Saved {data.Guests.Count} guests.");
		return data;
	}


	private GuestSaveData ConvertGuestToSave(Guest g)
	{
		var npc = g.BoundNPC;

		return new GuestSaveData
		{
			NpcId = npc?.Id,
			Npc = npc, // full NPC snapshot

			Name = g.Name,
			Gender = g.Gender,
			PortraitId = g.PortraitId,

			CurrentState = g.CurrentState,
			VisitDay = g.VisitDay,
			VisitHour = g.VisitHour,
			WaitDuration = g.WaitDuration,
			StayDuration = g.StayDuration,

			HasQuest = g.HasQuest,
			HasEaten = g.HasEaten,

			SeatIndex = g.SeatIndex,

			DepartureTimeISO = g.DepartureTime?.ToString("o"),

			EntryPatience = g.EntryPatience,
			TavernLingerTime = g.TavernLingerTime,
			SeatRetryInterval = g.SeatRetryInterval,
			SocializeDuration = g.SocializeDuration
		};
	}

	public void FromData(GuestDataBlock data)
	{
		// Clear current runtime lists
		guestsInside.Clear();
		guestsOutside.Clear();
		AllKnownGuests.Clear();

		if (data == null || data.Guests == null)
		{
			GD.Print("[GuestManager] No guest data to restore.");
			return;
		}

		foreach (var save in data.Guests)
		{
			// Rebuild NPCData (full snapshot)
			NPCData npc = save.Npc ?? new NPCData();
			if (string.IsNullOrEmpty(npc.Id) && !string.IsNullOrEmpty(save.NpcId))
				npc.Id = save.NpcId;

			// Rebuild Guest wrapper
			var guest = new Guest
			{
				Name = save.Name,
				Gender = save.Gender,
				PortraitId = save.PortraitId,
				BoundNPC = npc,

				VisitDay = save.VisitDay,
				VisitHour = save.VisitHour,
				WaitDuration = save.WaitDuration,
				StayDuration = save.StayDuration,

				HasQuest = save.HasQuest,
				HasEaten = save.HasEaten,
				SeatIndex = save.SeatIndex,

				EntryPatience = save.EntryPatience,
				TavernLingerTime = save.TavernLingerTime,
				SeatRetryInterval = save.SeatRetryInterval,
				SocializeDuration = save.SocializeDuration
			};

			if (!string.IsNullOrEmpty(save.DepartureTimeISO)
				&& DateTime.TryParse(save.DepartureTimeISO, out var dt))
			{
				guest.DepartureTime = dt;
			}
			else
			{
				guest.DepartureTime = null;
			}

			guest.SetState(save.CurrentState);
			npc.State = guest.CurrentState;

			AllKnownGuests.Add(guest);

			if (guest.IsInside)
				guestsInside.Add(guest);
			else if (guest.IsOnStreet)
				guestsOutside.Add(guest);
		}

		// Reschedule departures for guests currently inside
		foreach (var guest in guestsInside)
		{
			if (guest.DepartureTime.HasValue)
			{
				TimerManager.Instance?.ScheduleEvent(
					guest.DepartureTime.Value,
					() => Leave(guest)
				);
			}
		}

		GD.Print($"[GuestManager] Restored {AllKnownGuests.Count} guests from save.");
	}
}
