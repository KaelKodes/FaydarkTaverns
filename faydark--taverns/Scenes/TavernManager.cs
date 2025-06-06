using Godot;
using System;
using System.Collections.Generic;

public partial class TavernManager : Node
{
	// --- Static Reference ---
	public static TavernManager Instance;
	public static int CurrentTU => Instance != null ? Instance.GetCurrentTU() : 0;

	// --- Game Time ---
	[Export] public Label TimeLabel;
	[Export] public Label GoldLabel;
	[Export] public Button PauseButton;

	private float gameTime = 6 * 60;
	private bool isPaused = false;
	private int lastDay = -1;
	private const int DayStartHour = 6;

	// --- Adventurers ---
	private Dictionary<string, ClassTemplate> classTemplates;
	private List<Adventurer> adventurerList = new();

	[Export] public PackedScene AdventurerCardScene;
	private VBoxContainer adventurerListUI;
	[Export] public NodePath AdventurerRosterPath;
	private AdventurerRosterPanel adventurerRosterPanel;


	public override void _Ready()
{
	Instance = this;

	// Wire up UI
	TimeLabel ??= GetNode<Label>("../TopBar/TimeLabel");
	GoldLabel ??= GetNode<Label>("../TopBar/GoldLabel");
	PauseButton ??= GetNode<Button>("../TopBar/PauseButton");
	PauseButton.Pressed += TogglePause;

	// Locate UI container to hold adventurer cards
	adventurerListUI = GetNode<VBoxContainer>("../MainArea/AdventurerRosterPanel/ScrollContainer/AdventurerListContainer");
	AdventurerCardScene ??= GD.Load<PackedScene>("res://Scenes/UI/AdventurerCard.tscn");

	// 👇 This line ensures the exported path works at runtime
	if (!string.IsNullOrEmpty(AdventurerRosterPath))
	{
		GetNode<AdventurerRosterPanel>("../MainArea/AdventurerRosterPanel").Populate(adventurerList);
	}

	UpdateTimeLabel();
	GenerateAdventurers();
	DisplayAdventurers();
	CallDeferred(nameof(DelayedDayStart));
}
private void DelayedDayStart()
{
	StartNewDay(); // Now safe, all Instances will be initialized
}


	public override void _Process(double delta)
	{
		if (isPaused)
			return;

		gameTime += (float)delta;
		if (gameTime >= 24 * 60)
			gameTime = 6 * 60;

		UpdateTimeLabel();

		int currentDay = Mathf.FloorToInt(gameTime / (24 * 60));
		int currentHour = Mathf.FloorToInt(gameTime / 60f) % 24;

		if (currentHour == DayStartHour && currentDay != lastDay)
		{
			lastDay = currentDay;
			StartNewDay();
		}
	}

	private void UpdateTimeLabel()
	{
		int hours = Mathf.FloorToInt(gameTime / 60f);
		int minutes = Mathf.FloorToInt(gameTime % 60f);
		TimeLabel.Text = $"{hours:D2}:{minutes:D2}";
	}

	private void TogglePause()
	{
		isPaused = !isPaused;
		PauseButton.Text = isPaused ? "Play" : "Pause";
	}

	private void GenerateAdventurers()
	{
		GD.Print("Generating adventurers...");
		classTemplates = ClassTemplate.GetDefaultClassTemplates();

		int idCounter = 1;
		foreach (var kv in classTemplates)
		{
			var adventurer = AdventurerGenerator.GenerateAdventurer(idCounter++, kv.Value);
			adventurerList.Add(adventurer);
			GD.Print($"{adventurer.Name} the {adventurer.ClassName} [Role {adventurer.RoleId}] | HP: {adventurer.GetHp()} Mana: {adventurer.GetMana()}");
		}
	}

	private void DisplayAdventurers()
	{
		foreach (var adventurer in adventurerList)
		{
			var card = AdventurerCardScene.Instantiate<Control>();
			card.GetNode<Label>("VBoxContainer/NameLabel").Text = adventurer.Name;
			card.GetNode<Label>("VBoxContainer/ClassLabel").Text = adventurer.ClassName;
			card.GetNode<Label>("VBoxContainer/VitalsLabel").Text = $"HP: {adventurer.GetHp()} | Mana: {adventurer.GetMana()}";
			adventurerListUI.AddChild(card);
		}
	}

	private void StartNewDay()
	{
		GD.Print("☀️ New day begins! Clearing old data...");

		// Clear unclaimed quests
		QuestManager.Instance?.ClearUnclaimedQuests();

		// Clear idle adventurers
		adventurerList.RemoveAll(a => a.AssignedQuestId == null);

		// Generate new adventurers
		adventurerList.Clear();
		int idCounter = 1;
		foreach (var kv in classTemplates)
		{
			var adventurer = AdventurerGenerator.GenerateAdventurer(idCounter++, kv.Value);
			adventurerList.Add(adventurer);
		}

		// Display adventurers in UI
		adventurerRosterPanel?.Populate(adventurerList);


		// Generate quests
		QuestManager.Instance?.GenerateDailyQuests();

		GD.Print("✨ Day refreshed with new quests and adventurers.");
	}

	public int GetCurrentTU()
	{
		return Mathf.FloorToInt(gameTime); // 1 TU = 1 in-game minute
	}
}
