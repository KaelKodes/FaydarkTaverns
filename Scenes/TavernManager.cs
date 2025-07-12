using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FaydarkTaverns.Objects;




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
	//[Export] private int TavernSignLevel = 0;
	[Export] public Label TavernRenownDisplay;
	[Export] public Label TavernLevelDisplay;           // shows current level (e.g. "1")
	[Export] public Label TavernLevelLabel;             // shows static text "Tavern Level"
	//[Export] public int MaxFloorGuests = 5;
	[Export] private Label floorLabel;
	[Export] public VBoxContainer AdventurerListContainer;
	[Export] public VBoxContainer TavernFloorPanel;
	[Export] public VBoxContainer FloorSlots;
	[Export] public Label TavernFloorLabel;
	//[Export] public int Renown = 0;


	public List<Guest> floorGuests = new();
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
	public static int ExpToNextLevel => TavernLevel * 10;




	public static int Gold => currentGold;
	public Dictionary<string, int> PurchasedItems = new();
	public Dictionary<string, int> Supplies = new() {
	{ "Bread", 0 },
	{ "Ale", 0 }
	};

	// --- Adventurers ---
	private List<Guest> streetOutsideGuests = new();
	private static List<Table> tables = new();
	public List<Guest> AllVillagers = new();
	public int MaxVillagers => 16;
	public int CurrentVillagerCount => AllVillagers.Count;
	private bool hasSpawnedInitialParty = false; 


	// --- Connected Scenes ---
	private static PackedScene GuestCardScene = GD.Load<PackedScene>("res://Scenes/UI/GuestCard.tscn");
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
		var logText = GetNode<RichTextLabel>("../LogControl/LogPanel/LogText");
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

		var board = GetNode<QuestBoardPanel>("../QuestBoardPanel");

		ShopButton = GetNode<Button>("../TavernDisplay/ControlPanel/ShopButton");
		ShopButton.Pressed += ToggleShop;
		TavernLevelDisplay = GetNode<Label>("../TavernDisplay/TavernLevelControl/VBoxContainer/TavernLevelDisplay");
		TavernLevelLabel = GetNode<Label>("../TavernDisplay/TavernLevelControl/VBoxContainer/TavernLevelLabel");
		TavernRenownDisplay = GetNode<Label>("../TavernDisplay/TavernRenown/VBoxContainer/TavernRenownDisplay");
		
		QuestManager.Instance.OnQuestsUpdated += UpdateQuestCapacityLabel;
		UpdateQuestCapacityLabel();


		// Locate UI container to hold adventurer cards
		adventurerListUI = GetNodeOrNull<VBoxContainer>("../AdventurerRosterPanel/AdventurerListContainer");

		if (adventurerListUI == null)
		{
			GD.PrintErr("❌ 'adventurerListUI' was not found — check scene tree and node paths.");
		}

		GuestCardScene ??= GD.Load<PackedScene>("res://Scenes/UI/GuestCard.tscn");

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
	RecheckQuestPosting();

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
		GameLog.Debug($"🔍 Active Quest: {q.QuestId} | {q.Title} | Accepted: {q.IsAccepted}");
	}

	// 🕒 Check for guests who should leave
	GuestManager.Instance.TickGuests(ClockManager.CurrentTime);
}


private void StartNewDay(DateTime currentDate)
{
	GameLog.Debug($"🌅 New day triggered by ClockManager: {currentDate:D}");

	QuestManager.Instance?.ClearUnclaimedQuests();

	// 🧍 Only spawn the initial set of villagers once
	if (!hasSpawnedInitialParty)
	{
		// 🎯 Spawn one adventurer per class
		foreach (var className in ClassTemplate.GetAllClassNames())
		{
			var guest = GuestManager.SpawnNewNPC(NPCRole.Adventurer, className);
if (guest != null)
{
	guest.SetState(NPCState.Elsewhere); // ✅ Start as Elsewhere
	guest.VisitDay = ClockManager.CurrentDay;
	guest.VisitHour = GD.RandRange(7, 22); // 9AM–8PM range
	guest.WaitDuration = GD.RandRange(2, 4);

	GuestManager.QueueGuest(guest);

	if (!AllVillagers.Contains(guest))
		AllVillagers.Add(guest);
}

		}

		// 🧓 Spawn 4 informants (quest givers)
		for (int i = 0; i < 4; i++)
		{
			var guest = GuestManager.SpawnNewNPC(NPCRole.QuestGiver);
			if (guest != null)
			{
				guest.SetState(NPCState.Elsewhere);
				guest.VisitDay = ClockManager.CurrentDay;
				guest.VisitHour = GD.RandRange(6, 18);
				GuestManager.QueueGuest(guest);

				if (!AllVillagers.Contains(guest))
					AllVillagers.Add(guest);
			}
		}

		hasSpawnedInitialParty = true;
	}

	// 🔁 Persistent Guests
	foreach (var guest in AllVillagers)
	{
		// 🔄 Reset their "HasPostedToday" flag
		if (guest.BoundNPC != null)
			guest.BoundNPC.HasPostedToday = false;

		// 🧍 Only persistent guests return
		bool isPersistent = guest.BoundNPC != null;
		bool isQuestBound = guest.CurrentState == NPCState.StagingArea || guest.CurrentState == NPCState.AssignedToQuest;
		bool isDeployed = guest.CurrentState == NPCState.AssignedToQuest;
		bool isElsewhere = guest.CurrentState == NPCState.Elsewhere;

		if (guest != null && isPersistent && isElsewhere && !isQuestBound && !isDeployed)
		{
			guest.VisitDay = ClockManager.CurrentDay;
			guest.VisitHour = GD.RandRange(6, 18);
			GuestManager.QueueGuest(guest);
			GameLog.Debug($"🔁 {guest.Name} is returning to town today.");
		}
	}

	GD.Print("✨ Midnight, a New Day begins...");
	DisplayAdventurers();
	UpdateFloorLabel();
	RecheckSeating();
}



	
	private void UpdateQuestCapacityLabel()
{
	var questCapacityLabel = GetNodeOrNull<Label>("../QuestCapacity");
	if (questCapacityLabel != null)
	{
		questCapacityLabel.Text = QuestManager.Instance.GetQuestBoardStatusLabel();
	}
	else
	{
		GD.PrintErr("❌ Could not find QuestCapacity label. Check node path.");
	}
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

	// ✅ Sanitize: show guests currently on the tavern floor and not seated
	var floorList = AllVillagers
		.Where(g =>
			g != null &&
			g.CurrentState == NPCState.TavernFloor &&
			g.AssignedTable == null)
		.ToList();

	// Render exactly MaxFloorGuests number of slots
	for (int i = 0; i < TavernStats.Instance.MaxFloorGuests; i++)
	{
		var guest = floorList.ElementAtOrDefault(i);

		var card = GuestCardScene.Instantiate<GuestCard>();
		card.SetMouseFilter(Control.MouseFilterEnum.Stop);

		if (guest != null && guest.BoundNPC != null)
		{
			card.BoundGuest = guest;
			card.BoundNPC = guest.BoundNPC;

			string name = guest.BoundNPC.FirstName;
			string roleLabel = guest.BoundNPC.ClassName;

			card.GetNode<Label>("VBoxContainer/NameLabel").Text = name;
			card.GetNode<Label>("VBoxContainer/ClassLabel").Text = $"{guest.BoundNPC.Level} {roleLabel}";
		}
		else
		{
			card.SetEmptySlot();
		}

		FloorSlots.AddChild(card);
	}

	// Update all table panels to reflect current seating
	foreach (var table in tables)
	{
		table.LinkedPanel?.UpdateSeatSlots();
	}
}

	public void OnGuestSeated(Guest guest)
	{
		if (floorGuests.Contains(guest))
			floorGuests.Remove(guest);

		UpdateFloorLabel();
		DisplayAdventurers();
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
		var label = GetNode<Label>("../TavernDisplay/ControlPanel/GoldLabel");
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

	//public void IncreaseTavernLevel()
	//{
	//	TavernLevel++;
	//	GameLog.Info($"🏰 Tavern leveled up! New level: {TavernLevel}");
	//}
	
	public void GainTavernExp(int amount)
{
	TavernStats.Instance.AddExp(amount);
	UpdateTavernStatsDisplay();
}




	public void AddTable(string name)
{
	if (!tableCounters.ContainsKey(name))
		tableCounters[name] = 1;
	else
		tableCounters[name]++;

	string uniqueName = $"{name} {tableCounters[name]}";

	var tableScene = GD.Load<PackedScene>("res://Scenes/Table.tscn");
	var tableInstance = tableScene.Instantiate<Table>();
	
	// 👇 Set correct seat count per table type
	switch (name)
	{
		case "Starting Table": tableInstance.SeatCount = 2; break; 
		case "Tiny Table": tableInstance.SeatCount = 2; break;
		case "Small Table": tableInstance.SeatCount = 4; break;
		case "Medium Table": tableInstance.SeatCount = 6; break;
		case "Large Table": tableInstance.SeatCount = 8; break;
		default: tableInstance.SeatCount = 4; break;
	}

	tableInstance.TableName = uniqueName;

	// Layout fix (optional): add margin or spacing if needed
	furniturePanel.FurnitureVBox.AddChild(tableInstance);
	tables.Add(tableInstance);

	var panelScene = GD.Load<PackedScene>("res://Scenes/UI/TablePanel.tscn");
	var panelInstance = panelScene.Instantiate<TablePanel>();

	panelInstance.LinkedTable = tableInstance;
	AdventurerListContainer.AddChild(panelInstance);

	tableInstance.LinkedPanel = panelInstance;

	GameLog.Info($"🪑 Table added: {uniqueName}");
}

public void RecheckQuestPosting()
{
	foreach (var guest in AllVillagers)
	{
		if (guest.BoundNPC == null || guest.BoundNPC.Role != NPCRole.QuestGiver)
			continue;

		if (guest.BoundNPC.HasPostedToday)
			continue;

		if (guest.CurrentState == NPCState.TavernFloor &&
			guest.AssignedTable == null &&
			!guest.IsAssignedToQuest &&
			!guest.IsOnQuest &&
			guest.BoundNPC.PostedQuest == null)
		{
			if (QuestManager.Instance.CanAddQuest())
			{
				var quest = QuestGenerator.GenerateFromGiver(guest.BoundNPC, guest);
				if (quest != null)
				{
					QuestManager.Instance.AddQuest(quest);
					guest.BoundNPC.PostedQuest = quest;
					guest.BoundNPC.HasPostedToday = true;
					GameLog.Info($"🧾 {guest.Name} posted a new quest: '{quest.Title}'");
				}
			}
		}
	}
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
		TavernLevelDisplay.Text = TavernStats.Instance.Level.ToString();

	if (TavernRenownDisplay != null)
		TavernRenownDisplay.Text = TavernStats.Instance.Renown.ToString();

	if (TavernLevelLabel != null)
		TavernLevelLabel.TooltipText = $"{TavernStats.Instance.Exp} / {TavernStats.Instance.ExpToNextLevel} EXP";
}

public bool TrySeatGuest(Guest guest)
{
	// ✅ Only seat if the guest is inside and not already seated
	if (guest == null || guest.AssignedTable != null || guest.CurrentState != NPCState.TavernFloor)
		return false;

	foreach (var table in tables)
	{
		if (table.HasFreeSeat())
		{
			int seatIndex = table.AssignGuest(guest);
			if (seatIndex >= 0)
			{
				GameLog.Debug($"🪑 {guest.Name} took a seat at {table.TableName}.");
				return true;
			}
		}
	}

	return false;
}

public int GetPurchasedCount(string itemName)
{
	if (PurchasedItems.TryGetValue(itemName, out var count))
		return count;

	return 0;
}

public void UpdateFloorLabel()
{
	int onFloor = AllVillagers.Count(g =>
		g != null &&
		g.CurrentState == NPCState.TavernFloor &&
		g.AssignedTable == null &&
		!g.IsOnQuest &&
		!g.IsAssignedToQuest
	);

	TavernFloorLabel.Text = $"{onFloor}/{TavernStats.Instance.MaxFloorGuests}";
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
	// 🧍 Skip if already inside the tavern
	if (guest.IsInside)
	{
		GameLog.Debug($"⛔ Guest {guest.Name} is already inside. Skipping admission.");
		return;
	}

	// 🔒 Enforce guest cap (standing guests only)
	int standingGuests = AllVillagers.Count(g =>
		g != null &&
		g.CurrentState == NPCState.TavernFloor &&
		g.AssignedTable == null &&
		!g.IsOnQuest &&
		!g.IsAssignedToQuest
	);

	if (standingGuests >= TavernStats.Instance.MaxFloorGuests)
	{
		GameLog.Debug($"⛔ Guest {guest.Name} was denied entry (tavern floor full).");
		return;
	}

	// ✅ Proceed with admission: now safe to set state
	guest.SetState(NPCState.TavernFloor);

	// 🧠 Register in AllVillagers list if new
	if (!AllVillagers.Contains(guest))
		AllVillagers.Add(guest);

	// 🎯 Quest givers must stand to post quests
	if (guest.BoundNPC?.Role == NPCRole.QuestGiver)
	{
		GameLog.Debug($"📜 {guest.Name} entered to post a quest.");

		if (!floorGuests.Contains(guest))
			floorGuests.Add(guest);

		RecheckQuestPosting();
	}
	else
	{
		// 🪑 Try to seat adventurers
		if (TrySeatGuest(guest))
		{
			GameLog.Debug($"✅ {guest.Name} was seated immediately.");
			return;
		}

		if (!floorGuests.Contains(guest))
			floorGuests.Add(guest);
	}

	GameLog.Info($"🏠 {guest.Name} entered the tavern.");
	UpdateFloorLabel();
	DisplayAdventurers();
}







	public void NotifyGuestLeft(Guest guest)
{
	// ✅ Always attempt to clear them from floor
	if (floorGuests.Contains(guest))
		floorGuests.Remove(guest);

	UpdateFloorLabel();

	// ✅ Ensure proper seat cleanup
	if (guest.AssignedTable != null)
	{
		guest.AssignedTable.RemoveGuest(guest);
	}
	else if (guest.SeatIndex != null)
	{
		GameLog.Debug($"⚠️ {guest.Name} had a seat index but no table assigned. Forcing seat cleanup.");
		guest.SeatIndex = null;
	}

	DisplayAdventurers();
}



	public void RecheckSeating()
{
	foreach (var guest in floorGuests.ToList()) // ✅ avoid modification errors
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
	// ✅ Clean up invalid entries
	floorGuests = floorGuests
		.Where(g => g != null && g.CurrentState == NPCState.TavernFloor)
		.ToList();

	// 🩹 Optional: Repair any legacy inconsistencies
	foreach (var guest in floorGuests)
	{
		guest.SetState(NPCState.TavernFloor);
	}

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

			default:
				GameLog.Debug($"⚠️ Unknown shop item purchased: {item.Name}");
				break;
		}

		// ✅ Grant Renown
		if (item.RenownValue > 0)
		{
			TavernStats.Instance.AddRenown(item.RenownValue);
			UpdateTavernStatsDisplay();
			GameLog.Info($"🏅 Gained {item.RenownValue} Renown from {item.Name}!");
		}
		// ✅ Track purchases for enforcement logic
if (!PurchasedItems.ContainsKey(item.Name))
	PurchasedItems[item.Name] = 1;
else
	PurchasedItems[item.Name]++;


	}


	#endregion

}
