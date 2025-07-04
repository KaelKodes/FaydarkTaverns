using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static GuestLogic;



public partial class TavernManager : Node
{
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
	[Export] public Label TavernRenownDisplay;
	[Export] public Label TavernLevelDisplay;           // shows current level (e.g. "1")
	[Export] public Label TavernLevelLabel;             // shows static text "Tavern Level"
	[Export] public int MaxFloorGuests = 5;
	[Export] private Label floorLabel;
	[Export] public VBoxContainer AdventurerListContainer;
	[Export] public VBoxContainer TavernFloorPanel;
	[Export] public VBoxContainer FloorSlots;


	private List<Guest> floorGuests = new();
	[Export] public int Renown = 0;
	public static TavernManager Instance { get; private set; }
	private ClockManager Clock => GetNode<ClockManager>("/root/ClockManager");
	private Dictionary<string, int> tableCounters = new();





	private bool isPaused = false;
	public static int TimeMultiplier { get; private set; } = 1; // 0 = paused, 1 = normal, 2 = double, etc.

	// --- Tavern Stuff ---
	private int successComboCount = 0;
	public int SuccessComboCount => successComboCount;
	public static int TavernLevel { get; private set; } = 1;
	public static int TavernExp { get; private set; } = 0;
	public static int ExpToNextLevel => TavernLevel * 100;




	public static int Gold => currentGold;
	public Dictionary<string, int> PurchasedItems = new();
	public Dictionary<string, int> Supplies = new() {
	{ "Bread", 0 },
	{ "Ale", 0 }
	};

	// --- Adventurers ---
	private Dictionary<string, ClassTemplate> classTemplates;
	private List<Adventurer> streetOutsideGuests = new();
	private static List<Table> tables = new();

	// --- Connected Scenes ---
	private static PackedScene AdventurerCardScene = GD.Load<PackedScene>("res://Scenes/UI/AdventurerCard.tscn");
	private static VBoxContainer adventurerListUI;
	[Export] public NodePath AdventurerRosterPath;
	private AdventurerRosterPanel adventurerRosterPanel;
	[Export] public PackedScene ShopPanelScene;
	private Window shopPanelInstance;
	[Export] public NodePath FurniturePanelPath;
	private FurniturePanel furniturePanel;




	// --- Chaos! ---
	public override void _Ready()
	{
		//Random NumGen Seed
		GD.Seed((ulong)DateTime.Now.Ticks);

		if (Instance != null)
		{
			GD.PrintErr("❌ Multiple TavernManager instances detected!");
			return;
		}
		if (floorLabel == null)
			GD.PrintErr("❌ floorLabel not found!");


		Instance = this;

		// Wire up UI
		var logText = GetNode<RichTextLabel>("../LogPanel/LogText");
		GameLog.BindLogText(logText);

		ClockManager.OnNewDay += StartNewDay;
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
		TavernLevelDisplay = GetNode<Label>("../TavernLevelControl/VBoxContainer/TavernLevelDisplay");
		TavernLevelLabel = GetNode<Label>("../TavernLevelControl/VBoxContainer/TavernLevelLabel");
		TavernRenownDisplay = GetNode<Label>("../TavernRenown/VBoxContainer/TavernRenownDisplay");


		// Locate UI container to hold adventurer cards
		adventurerListUI = GetNodeOrNull<VBoxContainer>("../GuestPanels/AdventurerRosterPanel/ScrollContainer/AdventurerListContainer");

		if (adventurerListUI == null)
		{
			GD.PrintErr("❌ 'adventurerListUI' was not found — check scene tree and node paths.");
		}

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
		UpdateGoldLabel();

		// ✅ Delay anything that relies on singletons
		CallDeferred(nameof(DeferredStart));

	}

	private void DeferredStart()
	{
		UpdateTavernStatsDisplay();
	}


	public override void _Process(double delta)
	{
		if (isPaused || TimeMultiplier == 0)
			return;

		UpdateTimeLabel();
		RecheckSeating();


		// 🔁 Check for quest resolution
		foreach (var quest in QuestManager.Instance.GetAcceptedQuests())
		{
			if (!quest.IsComplete && ClockManager.CurrentTime >= quest.ExpectedReturn)
			{
				var result = QuestSimulator.Simulate(quest);
				quest.IsComplete = true;
				quest.Failed = !result.Success;

				AddGold(result.GoldEarned);

				foreach (var adventurer in quest.AssignedAdventurers)
				{
					adventurer.GainXP(result.ExpGained);
				}

				DisplayAdventurers();

				QuestManager.Instance.LogQuestResult(quest, result);
			}
		}

		foreach (var q in QuestManager.Instance.GetAcceptedQuests())
		{
			GD.Print($"🔍 Active Quest: {q.QuestId} | {q.Title} | Accepted: {q.IsAccepted}");
		}
	}






	private void StartNewDay(DateTime currentDate)
	{
		// ✅ Remove old .Instance null check — these are now autoloads
		GD.Print($"🌅 New day triggered by ClockManager: {currentDate:D}");

		QuestManager.Instance?.ClearUnclaimedQuests(); // ✅ Still uses .Instance since QuestManager is not autoloaded (yet)
		classTemplates = ClassTemplate.GetDefaultClassTemplates();

		int idCounter = 1;
		foreach (var kv in classTemplates)
		{
			var adventurer = AdventurerGenerator.GenerateAdventurer(idCounter++, kv.Value);

			int visitHour = GD.RandRange(6, 18);
int wait = GD.RandRange(1, 2);
int stay = GD.RandRange(4, 8);

var guest = new Guest
{
	Name = adventurer.Name,
	IsAdventurer = true,
	VisitDay = ClockManager.CurrentDay,
	VisitHour = visitHour,
	WaitDuration = wait,
	StayDuration = stay,
	AssignedTable = null,
	SeatIndex = null,
	BoundAdventurer = adventurer
};

			GameLog.Debug($"🕐 {guest.Name} plans to visit at {visitHour}:00, wait {wait}h, stay {stay}h.");
			GuestManager.QueueGuest(guest, this); // from inside TavernManager

		}

		QuestManager.Instance?.GenerateDailyQuests();
		GD.Print("✨ Day refreshed with new quests and adventurers.");
		DisplayAdventurers(); // ✅ show empty slots even before anyone arrives
		UpdateFloorLabel();   // ✅ show 0 / MaxFloorGuests

	}




	public void SortQuestCards()
	{
		var board = GetNode<QuestBoardPanel>("../GuestPanels/QuestBoardPanel");
		board.SortQuestCards();
	}

	#region Guests
	public void DisplayAdventurers()
{
	if (FloorSlots == null || !IsInstanceValid(FloorSlots))
	{
		GD.PrintErr("❌ FloorSlots container is null or invalid.");
		return;
	}

	// Clear all floor slot contents
	foreach (var child in FloorSlots.GetChildren())
		child.QueueFree();

	// Render exactly MaxFloorGuests number of slots
	for (int i = 0; i < MaxFloorGuests; i++)
	{
		// Try to find an unseated guest for this slot
		var guest = floorGuests
			.Where(g => g.AssignedTable == null)
			.ElementAtOrDefault(i);

		if (guest != null)
		{
			var card = AdventurerCardScene.Instantiate<AdventurerCard>();
			card.BoundAdventurer = guest.BoundAdventurer;

			var nameLabel = card.GetNode<Label>("VBoxContainer/NameLabel");
			var classLabel = card.GetNode<Label>("VBoxContainer/ClassLabel");
			var vitalsLabel = card.GetNode<Label>("VBoxContainer/VitalsLabel");

			nameLabel.Text = guest.BoundAdventurer.Name;
			classLabel.Text = $"{guest.BoundAdventurer.Level} {guest.BoundAdventurer.ClassName}";
			vitalsLabel.Text = $"HP: {guest.BoundAdventurer.GetHp()} | Mana: {guest.BoundAdventurer.GetMana()}";

			FloorSlots.AddChild(card);
		}
		else
		{
			var emptySlot = new Panel(); // or a custom placeholder
			emptySlot.CustomMinimumSize = new Vector2(250, 50); // adjust as needed
			emptySlot.AddThemeColorOverride("bg_color", new Color(0.2f, 0.2f, 0.2f)); // make it visibly "empty"
			FloorSlots.AddChild(emptySlot);
		}
	}
	// Update all table panels to reflect seated guests
foreach (var table in tables)
{
	if (table.LinkedPanel != null)
		table.LinkedPanel.UpdateSeats(floorGuests); // pass the full list
}

}




	



	#endregion
	#region Gold
	private static int currentGold = 0;

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
		if (Clock != null)
		{
			TimeLabel.Text = ClockManager.GetFormattedTime();
		}
	}


	private void TogglePause()
	{
		bool paused = ClockManager.TimeMultiplier == 0;
		SetTimeMultiplier(paused ? 1 : 0);
		PauseButton.Text = paused ? "Pause" : "Play";
	}

	public static void SetTimeMultiplier(int multiplier)
	{
		ClockManager.SetTimeMultiplier(multiplier);
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
	public static int TotalAvailableSeats()
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

	public static List<Table> GetAvailableTables()
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
	public void GainTavernExp(int amount)
	{
		TavernExp += amount;
		while (TavernExp >= ExpToNextLevel)
		{
			TavernExp -= ExpToNextLevel;
			TavernLevel++;
		}

		UpdateTavernStatsDisplay(); // ✅ update the UI after stat change
	}



	public void AddTable(string name)
{
	// 🔢 Generate a unique name like "Tiny Table 1"
	if (!tableCounters.ContainsKey(name))
		tableCounters[name] = 1;
	else
		tableCounters[name]++;

	string uniqueName = $"{name} {tableCounters[name]}";

	// 🪑 Instantiate table
	var tableScene = GD.Load<PackedScene>("res://Scenes/Table.tscn");
	var tableInstance = tableScene.Instantiate<Table>();
	tableInstance.SeatCount = 4;
	tableInstance.TableName = uniqueName; // ✅ Assign the name

	furniturePanel.FurnitureVBox.AddChild(tableInstance);
	tables.Add(tableInstance);

	// 🪑 Instantiate matching TablePanel
	var panelScene = GD.Load<PackedScene>("res://Scenes/UI/TablePanel.tscn");
	var panelInstance = panelScene.Instantiate<TablePanel>();

	panelInstance.LinkedTable = tableInstance;
	AdventurerListContainer.AddChild(panelInstance);

	tableInstance.LinkedPanel = panelInstance;

	GameLog.Info($"🪑 Table added: {uniqueName}");
}


	private void AddDecoration(string name)
	{
		GameLog.Debug($"🖼️ Decoration added: {name}");
		// TODO: Instantiate decoration visuals here, once implemented.
	}
	public void IncrementSuccessCombo()
	{
		successComboCount++;
	}

	public void ResetSuccessCombo()
	{
		successComboCount = 0;
	}
	public void UpdateTavernStatsDisplay()
	{
		if (TavernLevelDisplay != null)
			TavernLevelDisplay.Text = TavernLevel.ToString();

		if (TavernRenownDisplay != null)
			TavernRenownDisplay.Text = Renown.ToString();

		if (TavernLevelLabel != null)
			TavernLevelLabel.TooltipText = $"{TavernExp} / {ExpToNextLevel} EXP";
	}
	private bool TrySeatGuest(Guest guest)
	{
		foreach (var table in GetAvailableTables())
		{
			if (table.HasFreeSeat())
			{
				table.AssignGuest(guest);
				guest.AssignedTable = table;
				guest.SeatIndex = table.GetFreeSeatIndex();
				return true;
			}
		}
		return false;
	}

	public void UpdateFloorLabel()
{
	if (floorLabel == null || !IsInstanceValid(floorLabel))
		return;

	int onFloor = floorGuests.Count(g => g.AssignedTable == null);
	floorLabel.Text = $"Tavern Floor: {onFloor} / {MaxFloorGuests}";
}


	public void OnGuestEntered(Guest guest)
	{
		floorGuests.Add(guest);
		UpdateFloorLabel();
		DisplayAdventurers();
	}
	public void OnGuestRemoved(Guest guest)
	{
		floorGuests.Remove(guest);
		UpdateFloorLabel();
		DisplayAdventurers();
	}

	public void AdmitGuestToTavern(Guest guest)
{
	if (floorGuests.Contains(guest))
		return; // ✅ already inside

	if (floorGuests.Count >= MaxFloorGuests)
	{
		GameLog.Debug($"🚫 {guest.Name} couldn't enter — floor full.");
		return;
	}

	floorGuests.Add(guest);
	UpdateFloorLabel();
	DisplayAdventurers();

	if (TrySeatGuest(guest))
	{
		GameLog.Info($"🪑 {guest.Name} found a seat.");
	}
	else
	{
		GameLog.Debug($"🚶 {guest.Name} is standing (no seat yet).");
	}
}

	public void NotifyGuestLeft(Guest guest)
	{
		if (floorGuests.Contains(guest))
		{
			floorGuests.Remove(guest);
			UpdateFloorLabel();
		}
	}
	public void RecheckSeating()
	{
		foreach (var guest in floorGuests)
		{
			if (guest.AssignedTable != null)
				continue; // already seated

			if ((ClockManager.CurrentTime - guest.LastSeatCheck).TotalSeconds < 5)
				continue; // not time yet

			guest.LastSeatCheck = ClockManager.CurrentTime;

			if (TrySeatGuest(guest))
			{
				GameLog.Info($"🔁 {guest.Name} finally found a seat.");
			}
		}
	}

public List<Guest> GetGuestsInside()
{
	return floorGuests;
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

		// ✅ Grant Renown
		if (item.RenownValue > 0)
		{
			Renown += item.RenownValue;
			UpdateTavernStatsDisplay();
			GameLog.Info($"🏅 Gained {item.RenownValue} Renown from {item.Name}!");
		}

	}


	#endregion

}
