using Godot;
using System;

public static class GameStateLoader
{
	// Holds data until TavernMain scene finishes loading
	public static SaveData PendingLoadData = null;
	public static bool IsRestoring = false;


	// Called by LoadWindow AFTER reading a file
	public static void Apply(SaveData data)
	{
		PendingLoadData = data;
	}

	// Called by TavernMain._Ready()
	public static void RestoreIntoScene(SaveData data)
	{
		IsRestoring = true;

		if (data == null)
		{
			GD.PrintErr("[GameStateLoader] No SaveData to restore.");
			return;
		}

		GD.Print("[GameStateLoader] Restoring save data into scene…");

		// ==============================================
		// 1. WORLD / CLOCK
		// ==============================================
		if (data.World != null)
		{
			ClockManager.Instance.FromData(data.World);
		}

		// ==============================================
		// 2. TAVERN (gold + stats + purchases)
		// ==============================================
		if (data.Tavern != null && TavernManager.Instance != null)
		{
			TavernManager.Instance.FromData(data.Tavern);
		}


		// ==============================================
		// 3. PLAYER PANTRY (PlayerData maps to pantry+gold)
		// ==============================================
		if (data.Player != null)
		{
			PlayerPantry.FromData(data.Player);
		}

		// ==============================================
		// 4. JOURNAL
		// ==============================================
		if (data.Journal != null)
		{
			QuestJournalUnlockController.Instance?
				.SetUnlockedState(data.Journal.IsUnlocked);
		}

		// ==============================================
		// 5. QUESTS
		// ==============================================
		if (data.Quests != null)
		{
			QuestManager.Instance.FromData(data.Quests);
		}

		// ==============================================
		// 6. GUESTS
		// ==============================================
		if (data.Guests != null)
		{
			GuestManager.Instance.FromData(data.Guests);
		}

		// ==============================================
		// 7. UI REFRESH
		// ==============================================
		TavernManager.Instance.OnGameStateLoaded();

		PendingLoadData = null;
		IsRestoring = false;

		GD.Print("[GameStateLoader] Save restored successfully.");
	}
}
