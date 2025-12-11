using Godot;

public static class GameStateBuilder
{
	// ============================================================
	// BUILD SAVE DATA  
	// ============================================================
	public static SaveData BuildSaveData()
	{
		var data = new SaveData();

		// ---------- META ----------
		data.Meta = new MetaData();
		// We fill additional meta fields below once systems export their data

		// ---------- WORLD / TIME ----------
		data.World = ClockManager.Instance.ToData();

		// ---------- TAVERN ----------
		data.Tavern = TavernManager.Instance?.ToData() ?? new TavernData();

		// ---------- PLAYER DATA ----------
		data.Player = PlayerPantry.ToData();

		// ---------- GUESTS ----------
		data.Guests = GuestManager.Instance?.ToData() ?? new GuestDataBlock();

		// ---------- QUESTS ----------
		data.Quests = QuestManager.Instance?.ToData() ?? new QuestDataBlock();

		// ---------- JOURNAL ----------
		data.Journal = new JournalData
		{
			IsUnlocked = QuestJournalUnlockController.Instance != null &&
						 QuestJournalUnlockController.Instance.IsUnlocked
		};

		// ============================================================
		// COPY KEY INFO INTO META (for save slot UI)
		// ============================================================
		data.Meta.GameDay = ClockManager.GetCurrentDay();
		data.Meta.GameHour = ClockManager.CurrentTime.Hour;


		data.Meta.TavernName = data.Tavern.TavernName;
		data.Meta.TavernLevel = data.Tavern.Level;
		data.Meta.Renown = data.Tavern.Renown;

		return data;
	}


	// ============================================================
	// APPLY SAVE DATA  
	// ============================================================
	public static void ApplySaveData(SaveData data)
	{
		if (data == null)
		{
			GD.PrintErr("[GameStateBuilder] ERROR: ApplySaveData called with null SaveData.");
			return;
		}

		// ---------- WORLD ----------
		ClockManager.Instance.FromData(data.World);

		// ---------- TAVERN ----------
		if (TavernManager.Instance != null)
			TavernManager.Instance.FromData(data.Tavern);

		// ---------- PLAYER ----------
		PlayerPantry.FromData(data.Player);

		// ---------- GUESTS ----------
		if (GuestManager.Instance != null)
			GuestManager.Instance.FromData(data.Guests);

		// ---------- QUESTS ----------
		if (QuestManager.Instance != null)
			QuestManager.Instance.FromData(data.Quests);

		// ---------- JOURNAL ----------
		if (QuestJournalUnlockController.Instance != null)
		{
			QuestJournalUnlockController.Instance.SetUnlockedState(
				data.Journal != null && data.Journal.IsUnlocked
			);
		}

		GD.Print("[GameStateBuilder] Game restored from SaveData.");
	}
}
