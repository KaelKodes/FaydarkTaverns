using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class TavernManager : Node
{
	// --- Static Reference ---
	public static TavernManager Instance;

	// --- Game Time/Buttons/and Labels ---
	[Export] public Label TimeLabel;
	[Export] public Label GoldLabel;
	[Export] public Button PauseButton;
	[Export] public Button Button1x;
	[Export] public Button Button2x;
	[Export] public Button Button4x;
	[Export] public Button Button8x;
	[Export] public Button ShopButton;
	[Export] public Control ShopPanel;
	[Export] public NodePath QuestBoardPath;
	[Export] private int TavernSignLevel = 0;

	private bool isPaused = false;
	public static int TimeMultiplier { get; private set; } = 1; // 0 = paused, 1 = normal, 2 = double, etc.

	// --- Tavern Stuff ---
	

	public int Gold => currentGold;
	public int TavernLevel { get; private set; } = 1;
	public Dictionary<string, int> PurchasedItems = new();
	public Dictionary<string, int> Supplies = new() {
	{ "Bread", 0 },
	{ "Ale", 0 }
};

	// --- Adventurers ---
	private Dictionary<string, ClassTemplate> classTemplates;
	private List<Adventurer> adventurerList = new();
	private List<Table> tables = new();

	// --- Connected Scenes ---
	[Export] public PackedScene AdventurerCardScene;
	private VBoxContainer adventurerListUI;
	[Export] public NodePath AdventurerRosterPath;
	private AdventurerRosterPanel adventurerRosterPanel;
	[Export] public PackedScene ShopPanelScene;
	private Window shopPanelInstance;
	[Export] public NodePath FurniturePanelPath;
	private FurniturePanel furniturePanel;




	// --- Chaos! ---
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

		Button1x ??= GetNode<Button>("../ControlPanel/TopBar/Button1x");
		Button1x.Pressed += OnSpeed1xPressed;

		Button2x ??= GetNode<Button>("../ControlPanel/TopBar/Button2x");
		Button2x.Pressed += OnSpeed2xPressed;

		Button4x ??= GetNode<Button>("../ControlPanel/TopBar/Button4x");
		Button4x.Pressed += OnSpeed4xPressed;

		Button8x ??= GetNode<Button>("../ControlPanel/TopBar/Button8x");
		Button8x.Pressed += OnSpeed8xPressed;

		var board = GetNode<QuestBoardPanel>("../GuestPanels/QuestBoardPanel");
		
		ShopButton = GetNode<Button>("../ControlPanel/ShopButton");
		ShopButton.Pressed += ToggleShop;


		// Locate UI container to hold adventurer cards
		adventurerListUI = GetNode<VBoxContainer>("../GuestPanels/AdventurerRosterPanel/ScrollContainer/AdventurerListContainer");
		AdventurerCardScene ??= GD.Load<PackedScene>("res://Scenes/UI/AdventurerCard.tscn");

		if (!string.IsNullOrEmpty(AdventurerRosterPath))
		{
			adventurerRosterPanel = GetNode<AdventurerRosterPanel>(AdventurerRosterPath);
		}
		if (!string.IsNullOrEmpty(FurniturePanelPath))
		{
			var panel = GetNode<FurniturePanel>(FurniturePanelPath); // ✅ Uses the actual FurniturePanel class
			furniturePanel = panel;
		}



		UpdateTimeLabel();
		GenerateAdventurers();
		DisplayAdventurers();
		UpdateGoldLabel();
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
			classLabel.Text = $"{adventurer.Level} {adventurer.ClassName}";
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
		classLabel.Text = $"{adventurer.Level} {adventurer.ClassName}";
		vitalsLabel.Text = $"HP: {adventurer.GetHp()} | Mana: {adventurer.GetMana()}";

		adventurerListUI.AddChild(card);
	}

	public void SortQuestCards()
	{
		var board = GetNode<QuestBoardPanel>("../MainArea/QuestBoardPanel");
		board.SortQuestCards();
	}



	#region Gold
	private int currentGold = 0;

	public void AddGold(int amount)
	{
		currentGold += amount;
		UpdateGoldLabel();
		GameLog.Debug($"[Gold Update] Player now has {currentGold}g");
	}
	public bool SpendGold(int amount)
	{
		if (currentGold >= amount)
		{
			currentGold -= amount;
			UpdateGoldUI(); // if applicable
			return true;
		}
		return false;
	}

	private void UpdateGoldLabel()
	{
		if (GoldLabel != null)
			GoldLabel.Text = $"{currentGold}g";
	}
	private void UpdateGoldUI()
	{
		var label = GetNode<Label>("../ControlPanel/GoldLabel");
		label.Text = $"{currentGold}g";
	}

	#endregion
	#region Time

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
	}

	private void OnSpeed2xPressed()
	{
		TavernManager.SetTimeMultiplier(2);
	}

	private void OnSpeed4xPressed()
	{
		TavernManager.SetTimeMultiplier(4);
	}

	private void OnSpeed8xPressed()
	{
		TavernManager.SetTimeMultiplier(16);
	}
	#endregion
	#region Tavern Management
	public int TotalAvailableSeats()
	{
		int total = 0;
		foreach (var table in GetAvailableTables())
		{
			total += table.GetFreeSeatCount();
		}
		return total;
	}
	public int GetFreeSeatCount()
	{
		int total = 0;
		foreach (var table in tables)
		{
			total += table.GetFreeSeatCount();
		}
		return total;
	}

	public List<Table> GetAvailableTables()
	{
		var availableTables = new List<Table>();

		foreach (var table in tables)
		{
			if (table.HasFreeSeat())
				availableTables.Add(table);
		}

		return availableTables;
	}

	public void IncreaseTavernLevel()
	{
		TavernLevel++;
		GameLog.Info($"🏰 Tavern leveled up! New level: {TavernLevel}");
	}

	public void AddTable(string name)
{
	var tableScene = GD.Load<PackedScene>("res://Scenes/Table.tscn");
	var tableInstance = tableScene.Instantiate<Table>();
	tableInstance.SeatCount = 4; // You can improve this later based on table name

	// ✅ Now add it to the VBox inside FurniturePanel
	furniturePanel.FurnitureVBox.AddChild(tableInstance);

	GameLog.Info($"🪑 Table added: {name}");
}

	private void AddDecoration(string name)
{
	GameLog.Debug($"🖼️ Decoration added: {name}");
	// TODO: Instantiate decoration visuals here, once implemented.
}

	#endregion
	#region Shop
	
	private void ToggleShop()
{
	if (shopPanelInstance == null)
	{
		shopPanelInstance = (Window)ShopPanelScene.Instantiate();
		GetTree().Root.AddChild(shopPanelInstance);

		// Optional: Handle X button later too
		shopPanelInstance.CloseRequested += () => shopPanelInstance.Hide();
	}

	// Toggle visibility
	shopPanelInstance.Visible = !shopPanelInstance.Visible;
}




	public void PurchaseItem(ShopItem item)
{
	switch (item.Name)
	{
		case "Starting Table":
		case "Tiny Table":
		case "Small Table":
		case "Medium Table":
		case "Large Table":
			GameLog.Debug($"[PURCHASE] Purchased table: {item.Name}");
			AddTable(item.Name);
			break;

		case "Wall Banner":
		case "Fancy Rug":
		case "Mounted Trophy":
			AddDecoration(item.Name);
			break;

		case "10x Bread":
			Supplies["Bread"] += 10;
			break;

		case "10x Ale":
			Supplies["Ale"] += 10;
			break;

		case "Upgrade Tavern Sign":
			TavernSignLevel++;
			break;

		default:
			GameLog.Debug($"⚠️ Unknown shop item purchased: {item.Name}");
			break;
	}
}


	#endregion

}
