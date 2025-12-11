using Godot;
using System;

public partial class PlayThroughSelectWindow : Control
{
	// Mode determines how Select buttons behave
	public static PlaythroughSelectMode Mode = PlaythroughSelectMode.LoadGame;

	// References to UI nodes per slot
	[Export] public Label Slot1Details;
	[Export] public Label Slot2Details;
	[Export] public Label Slot3Details;
	[Export] public Label Slot4Details;

	[Export] public Button Slot1Select;
	[Export] public Button Slot2Select;
	[Export] public Button Slot3Select;
	[Export] public Button Slot4Select;
	[Export] public Button CloseButton;


	public override void _Ready()
	{
		Visible = false; // Hidden until opened
		RefreshUI();

		// Wire up button events
		Slot1Select.Pressed += () => OnSelectSlot(1);
		Slot2Select.Pressed += () => OnSelectSlot(2);
		Slot3Select.Pressed += () => OnSelectSlot(3);
		Slot4Select.Pressed += () => OnSelectSlot(4);
		 CloseButton.Pressed += () => Close();  
	}

	// ===========================
	// PUBLIC API
	// ===========================
	public void Open(PlaythroughSelectMode mode)
	{
		Mode = mode;
		RefreshUI();
		Show();
	}

	public void Close()
	{
		Hide();
	}

	// ===========================
	// SLOT POPULATION
	// ===========================
	private void RefreshUI()
	{
		SetSlotDetails(1, Slot1Details);
		SetSlotDetails(2, Slot2Details);
		SetSlotDetails(3, Slot3Details);
		SetSlotDetails(4, Slot4Details);
	}

	private void SetSlotDetails(int slot, Label label)
	{
		var manual = SaveManager.GetManualSlotInfo(slot);

		if (manual == null)
		{
			label.Text = "Empty Slot";
			return;
		}

		string hourFormatted = FormatHour(manual.GameHour);

		label.Text =
			$"Day {manual.GameDay} — {hourFormatted}\n" +
			$"Level {manual.TavernLevel} — Renown {manual.Renown}\n" +
			$"Saved: {manual.RealWorldTimestamp}";
	}

	private string FormatHour(int hour24)
	{
		int h = hour24 % 12;
		if (h == 0) h = 12;
		string ampm = (hour24 < 12) ? "AM" : "PM";
		return $"{h}:00 {ampm}";
	}

	// ===========================
	// SLOT SELECTION LOGIC
	// ===========================
	private void OnSelectSlot(int slot)
	{
		// Set this as the active slot
		SaveManager.SetCurrentSlot(slot);

		switch (Mode)
		{
			case PlaythroughSelectMode.NewGame:
				HandleNewGame(slot);
				break;

			case PlaythroughSelectMode.LoadGame:
				HandleLoadGame(slot);
				break;

			case PlaythroughSelectMode.SaveGame:
				HandleSaveGame(slot);
				break;
		}
	}

	// ===========================
	// MODE: NEW GAME
	// ===========================
	private void HandleNewGame(int slot)
	{
		var manual = SaveManager.GetManualSlotInfo(slot);

		if (manual != null)
		{
			// Confirm overwrite
			ConfirmationWindow.Instance.ShowWindow(
				"Overwrite this playthrough?",
				() => StartNewGame(slot),
				() => { }
			);
		}
		else
		{
			StartNewGame(slot);
		}
	}

	private void StartNewGame(int slot)
	{
		// TODO: load character creation or new game scene
		GD.Print($"[Playthrough] Starting NEW GAME in Slot {slot}");
		Close();
		GetTree().ChangeSceneToFile("res://Scenes/CharacterCreation.tscn");
	}

	// ===========================
	// MODE: LOAD GAME
	// ===========================
	private void HandleLoadGame(int slot)
	{
		var manual = SaveManager.GetManualSlotInfo(slot);
		var auto = SaveManager.GetAutoSlotInfo(slot);

		if (manual == null && auto == null)
		{
			GD.Print($"[Playthrough] Slot {slot} is empty — cannot load.");
			return;
		}

		// If both exist → show popup choice
		if (manual != null && auto != null)
		{
			ConfirmationWindow.Instance.ShowTripleChoice(
				$"Load Slot {slot}?",
				$"Manual: Day {manual.GameDay}, {FormatHour(manual.GameHour)}",
				$"Auto: Day {auto.GameDay}, {FormatHour(auto.GameHour)}",
				onManual: () => LoadManual(slot),
				onAuto: () => LoadAuto(slot),
				onCancel: () => { }
			);
		}
		else if (manual != null)
		{
			LoadManual(slot);
		}
		else
		{
			LoadAuto(slot);
		}
	}

	private void LoadManual(int slot)
	{
		var data = SaveManager.LoadManualFromSlot(slot);
		if (data != null)
		{
			ApplyGameState(data);
		}
	}

	private void LoadAuto(int slot)
	{
		var data = SaveManager.LoadAutoFromSlot(slot);
		if (data != null)
		{
			ApplyGameState(data);
		}
	}

	private void ApplyGameState(SaveData data)
{
	GD.Print("[Playthrough] Loading saved game…");

	// Store only — DO NOT restore into current scene
	GameStateLoader.Apply(data);

	GetTree().ChangeSceneToFile("res://Scenes/TavernMain.tscn");
}



	// ===========================
	// MODE: SAVE GAME
	// ===========================
	private void HandleSaveGame(int slot)
	{
		var manual = SaveManager.GetManualSlotInfo(slot);

		if (manual != null)
		{
			ConfirmationWindow.Instance.ShowWindow(
				"Overwrite this save?",
				() => SaveNow(slot),
				() => { }
			);
		}
		else
		{
			SaveNow(slot);
		}
	}

	private void SaveNow(int slot)
	{
		var data = GameStateBuilder.BuildSaveData();
		SaveManager.SaveManualToSlot(slot, data);

		Close();
	}
}
