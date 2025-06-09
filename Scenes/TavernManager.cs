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

		if (!string.IsNullOrEmpty(AdventurerRosterPath))
		{
			adventurerRosterPanel = GetNode<AdventurerRosterPanel>(AdventurerRosterPath);
		}

		UpdateTimeLabel();
		GenerateAdventurers();
		DisplayAdventurers();
		CallDeferred(nameof(DelayedDayStart));
	}

	private void DelayedDayStart()
	{
		StartNewDay();
		lastDay = 0;
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

		// 🔁 Check for quest resolution
		foreach (var quest in QuestManager.Instance.GetAcceptedQuests())
		{
			if (!quest.IsComplete && CurrentTU >= quest.ExpectedReturnTU)
			{
				var result = QuestSimulator.Simulate(quest);
				quest.IsComplete = true;
				quest.Failed = !result.Success;

				AddGold(result.GoldEarned);

				foreach (var adventurer in quest.AssignedAdventurers)
				{
					adventurer.GainXP(result.ExpGained);
					RestoreAdventurerToRoster(adventurer);
				}

				QuestManager.Instance.LogQuestResult(quest, result);
			}
		}
		foreach (var q in QuestManager.Instance.GetAcceptedQuests())
{
	GD.Print($"🔍 Active Quest: {q.QuestId} | {q.Title} | Accepted: {q.IsAccepted}");
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
		foreach (var child in adventurerListUI.GetChildren())
		{
			child.QueueFree();
		}

		foreach (var adventurer in adventurerList)
		{
			var card = AdventurerCardScene.Instantiate<AdventurerCard>();
			card.BoundAdventurer = adventurer;

			var nameLabel = card.GetNode<Label>("VBoxContainer/NameLabel");
			var classLabel = card.GetNode<Label>("VBoxContainer/ClassLabel");
			var vitalsLabel = card.GetNode<Label>("VBoxContainer/VitalsLabel");

			nameLabel.Text = adventurer.Name;
			classLabel.Text = adventurer.ClassName;
			vitalsLabel.Text = $"HP: {adventurer.GetHp()} | Mana: {adventurer.GetMana()}";

			adventurerListUI.AddChild(card);
		}
	}

	private void StartNewDay()
	{
		GD.Print("☀️ New day begins! Clearing old data...");

		QuestManager.Instance?.ClearUnclaimedQuests();
		adventurerList.RemoveAll(a => a.AssignedQuestId == null);

		adventurerList.Clear();
		int idCounter = 1;
		foreach (var kv in classTemplates)
		{
			var adventurer = AdventurerGenerator.GenerateAdventurer(idCounter++, kv.Value);
			adventurerList.Add(adventurer);
		}

		DisplayAdventurers();
		QuestManager.Instance?.GenerateDailyQuests();

		GD.Print("✨ Day refreshed with new quests and adventurers.");
	}

	public int GetCurrentTU()
	{
		return Mathf.FloorToInt(gameTime);
	}

	public void RestoreAdventurerToRoster(Adventurer adventurer)
	{
		if (adventurer == null || adventurerListUI == null || AdventurerCardScene == null)
			return;

		if (!adventurerList.Contains(adventurer))
			adventurerList.Add(adventurer);

		var card = AdventurerCardScene.Instantiate<AdventurerCard>();
		card.BoundAdventurer = adventurer;

		var nameLabel = card.GetNode<Label>("VBoxContainer/NameLabel");
		var classLabel = card.GetNode<Label>("VBoxContainer/ClassLabel");
		var vitalsLabel = card.GetNode<Label>("VBoxContainer/VitalsLabel");

		nameLabel.Text = adventurer.Name;
		classLabel.Text = adventurer.ClassName;
		vitalsLabel.Text = $"HP: {adventurer.GetHp()} | Mana: {adventurer.GetMana()}";

		adventurerListUI.AddChild(card);
	}

	public void AddGold(int amount)
	{
		if (int.TryParse(GoldLabel.Text.Replace("g", ""), out int currentGold))
		{
			currentGold += amount;
			GoldLabel.Text = $"{currentGold}g";
		}
	}
}
