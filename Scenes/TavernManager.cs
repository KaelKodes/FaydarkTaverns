using Godot;
using System;
using System.Collections.Generic;

public partial class TavernManager : Node
{
	// --- Static Reference ---
	public static TavernManager Instance;

	// --- Game Time ---
	[Export] public Label TimeLabel;
	[Export] public Label GoldLabel;
	[Export] public Button PauseButton;
	[Export] public Button Button1x;
	[Export] public Button Button2x;
	[Export] public Button Button4x;
	
	private bool isPaused = false;
	public static int TimeMultiplier { get; private set; } = 1; // 0 = paused, 1 = normal, 2 = double, etc.

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
		var logText = GetNode<RichTextLabel>("../LogPanel/LogText");
		GameLog.BindLogText(logText);

		ClockManager.Instance.OnNewDay += StartNewDay;
		TimeLabel ??= GetNode<Label>("../TopBar/TimeLabel");
		GoldLabel ??= GetNode<Label>("../TopBar/GoldLabel");
		PauseButton ??= GetNode<Button>("../TopBar/PauseButton");
		PauseButton.Pressed += TogglePause;
		
		Button1x ??= GetNode<Button>("../VBoxContainer/TopBar/Button1x");
		Button1x.Pressed += OnSpeed1xPressed;

		Button2x ??= GetNode<Button>("../VBoxContainer/TopBar/Button2x");
		Button2x.Pressed += OnSpeed2xPressed;

		Button4x ??= GetNode<Button>("../VBoxContainer/TopBar/Button4x");
		Button4x.Pressed += OnSpeed4xPressed;


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
	}

public override void _Process(double delta)
{
	if (isPaused || TimeMultiplier == 0)
		return;
	UpdateTimeLabel();


	// 🔁 Check for quest resolution
	foreach (var quest in QuestManager.Instance.GetAcceptedQuests())
	{
		if (!quest.IsComplete && ClockManager.Instance.CurrentTime >= quest.ExpectedReturn)
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
	if (ClockManager.Instance != null)
	{
		TimeLabel.Text = ClockManager.Instance.GetFormattedTime();
	}
}

	private void TogglePause()
	{
		bool paused = ClockManager.Instance.TimeMultiplier == 0;
ClockManager.Instance.SetTimeMultiplier(paused ? 1 : 0);
PauseButton.Text = paused ? "Pause" : "Play";

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
			GameLog.Debug($"{adventurer.Name} the {adventurer.ClassName} [Role {adventurer.RoleId}] | HP: {adventurer.GetHp()} Mana: {adventurer.GetMana()}");
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

	private void StartNewDay(DateTime currentDate)
{
	GD.Print($"🌅 New day triggered by ClockManager: {currentDate:D}");
	
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

public static void SetTimeMultiplier(int multiplier)
{
	if (ClockManager.Instance != null)
	{
		ClockManager.Instance.SetTimeMultiplier(multiplier);
	}
}


private void OnSpeed1xPressed()
{
	TavernManager.SetTimeMultiplier(1);
	GD.Print("⏱️ Time speed set to 1x");
	GameLog.Info("⏱️ Time speed set to 1x");
}

private void OnSpeed2xPressed()
{
	TavernManager.SetTimeMultiplier(2);
	GD.Print("⏩ Time speed set to 2x");
	GameLog.Info("⏱️ Time speed set to 2x");
}

private void OnSpeed4xPressed()
{
	TavernManager.SetTimeMultiplier(4);
	GD.Print("⏩⏩ Time speed set to 4x");
	GameLog.Info("⏱️ Time speed set to 4x");
}

}
