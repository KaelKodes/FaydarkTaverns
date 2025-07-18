using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FaydarkTaverns.Objects;


public partial class QuestCard : Panel
{
	[Export] public Label TitleLabel;
	[Export] public Label RegionLabel;
	[Export] public Label TypeLabel;
	[Export] public Label RewardLabel;
	[Export] public Label TimeLabel;



	// Party slot containers
	private List<HBoxContainer> PartySlots = new();
	private List<Label> partySlotLabels = new();
	private Quest quest;
	public Quest Quest => quest;

	public bool HasQuest(Quest q)
	{
		return ReferenceEquals(quest, q);
	}

	public override void _Ready()
{
	GameLog.Debug("QuestCard: Running _Ready()");

	try
	{
		TitleLabel = GetNode<Label>("VBox/TitleLabel");
		RegionLabel = GetNode<Label>("VBox/RegionLabel");
		TypeLabel = GetNode<Label>("VBox/TypeLabel");
		RewardLabel = GetNode<Label>("VBox/RewardLabel");
		TimeLabel = GetNode<Label>("VBox/TimeLabel");

		for (int i = 1; i <= 3; i++)
		{
			var panel = GetNode<HBoxContainer>($"VBox/PartySlotContainer/PartySlot{i}");
			PartySlots.Add(panel);

			var label = panel.GetNode<Label>("AdventurerLabel");
			partySlotLabels.Add(label);
		}

		if (quest != null)
			UpdateDisplay();

		AddToGroup("QuestCard"); // ✅ Add this to ensure cards are trackable
	}
	catch (Exception e)
	{
		GD.PrintErr("❌ QuestCard _Ready() failed: ", e.Message);
	}

	SetMouseFilter(Control.MouseFilterEnum.Stop);

	// ─── UI SOUND HOOKS (safe to add after all init) ────────────────

	this.MouseEntered += () => UIAudio.Instance.PlayHover();
	this.FocusEntered += () => UIAudio.Instance.PlayHover();

	this.GuiInput += @event =>
	{
		if (@event is InputEventMouseButton mouseEvent &&
			mouseEvent.ButtonIndex == MouseButton.Left &&
			mouseEvent.Pressed)
		{
			UIAudio.Instance.PlayClick();
		}
	};

	// ────────────────────────────────────────────────────────────────
}

	public void Bind(Quest quest)
	{
		this.quest = quest;
		UpdateDisplay();
	}


	public override void _ExitTree()
	{
		RemoveFromGroup("QuestCard"); // ✅ Clean up when node is freed
	}

	public void SetQuestData(Quest q)
	{
		quest = q;
		GameLog.Debug($"Card bound to Quest ID: {quest.QuestId}");
		GameLog.Debug($"[CARD] Card node: {Name} | Bound quest ref: {q.GetHashCode()} | Title: {q.Title} | QuestId: {q.QuestId}");
		if (IsInsideTree()) UpdateDisplay();
	}
	

	public void UpdateDisplay()
{
	if (quest == null)
	{
		GD.PrintErr("UpdateDisplay called with null quest.");
		return;
	}

	GameLog.Debug($"[UpdateDisplay] Quest {quest.QuestId} | {quest.Title} | IsAccepted: {quest.IsAccepted} | IsLocked: {quest.IsLocked}");

	// 🟩 Set quest info labels
	TitleLabel.Text = quest.Title;
	RegionLabel.Text = quest.Region.ToString();
	TypeLabel.Text = quest.Type.ToString();
	RewardLabel.Text = $"{quest.Reward}g";
	TimeLabel.Text = $"{quest.GetTotalExpectedTU()} Hours";

	GameLog.Debug($"Assigned Adventurers: {quest.AssignedAdventurers.Count}");

	for (int i = 0; i < partySlotLabels.Count; i++)
	{
		var panel = PartySlots[i];
		var label = partySlotLabels[i];
		var removeButton = panel.GetNode<Button>("RemoveButton");

		panel.MouseFilter = Control.MouseFilterEnum.Stop;

		// Setup style
		removeButton.Visible = false;
		removeButton.Text = "❌";
		removeButton.CustomMinimumSize = new Vector2(12, 12);
		removeButton.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
		removeButton.AddThemeFontSizeOverride("font_size", 10);
		removeButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		removeButton.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
		removeButton.FocusMode = Control.FocusModeEnum.None;

		// 🧹 Disconnect prior signals (hover + press)
		foreach (var conn in removeButton.GetSignalConnectionList("pressed"))
		{
			if (conn is Godot.Collections.Dictionary dict && dict.ContainsKey("callable"))
				removeButton.Disconnect("pressed", (Callable)dict["callable"]);
		}

		foreach (var conn in panel.GetSignalConnectionList("mouse_entered"))
		{
			if (conn is Godot.Collections.Dictionary dict && dict.ContainsKey("callable"))
				panel.Disconnect("mouse_entered", (Callable)dict["callable"]);
		}

		foreach (var conn in panel.GetSignalConnectionList("mouse_exited"))
		{
			if (conn is Godot.Collections.Dictionary dict && dict.ContainsKey("callable"))
				panel.Disconnect("mouse_exited", (Callable)dict["callable"]);
		}

		// >>>>>>>>>>>>>> ADD THIS HOVER SOUND LOGIC <<<<<<<<<<<<<<
		panel.MouseEntered += () => UIAudio.Instance.PlayHover();
		// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

		if (i < quest.AssignedAdventurers.Count)
		{
			var adventurer = quest.AssignedAdventurers[i];
			var firstName = adventurer.Name.Split(' ')[0];
			label.Text = firstName;
			panel.Visible = true;

			// Press ❌ to unassign
			var thisAdventurer = adventurer; // 🔒 capture locally

			removeButton.Pressed += () =>
			{
				GameLog.Debug("✅ RemoveButton clicked");

				if (!quest.IsLocked)
				{
					int index = quest.AssignedAdventurers.IndexOf(thisAdventurer);
					if (index >= 0)
					{
						GameLog.Info($"❌ Removing {thisAdventurer.Name} from '{quest.Title}'");
						UnassignFromSlot(index);
					}
					else
					{
						GameLog.Info($"⚠️ Couldn't find {thisAdventurer.Name} in quest party list.");
					}
				}
			};

			// Hover to show/hide ❌
			panel.MouseEntered += () => { if (!quest.IsLocked) removeButton.Visible = true; };
			panel.MouseExited += () => removeButton.Visible = false;

			// Blue slot background
			//var slotStyle = new StyleBoxFlat { BgColor = new Color(0.2f, 0.4f, 0.9f) };
			//panel.AddThemeStyleboxOverride("panel", slotStyle);

			GameLog.Debug($"  -> Slot {i}: {firstName}");
		}
		else
		{
			label.Text = "";
			panel.Visible = false;
			removeButton.Visible = false;
			GameLog.Debug($"  -> Slot {i}: (empty)");
		}
	}

	// Card background style
	var styleBox = new StyleBoxFlat();
styleBox.ContentMarginTop = 0;
styleBox.ContentMarginBottom = 0;
styleBox.ContentMarginLeft = 0;
styleBox.ContentMarginRight = 0;

styleBox.BorderWidthTop = 0;
styleBox.BorderWidthBottom = 0;
styleBox.BorderWidthLeft = 0;
styleBox.BorderWidthRight = 0;

// Apply background color based on quest state
if (quest.IsComplete)
	styleBox.BgColor = new Color(0.1f, 0.1f, 0.1f); // Completed
else if (quest.IsAccepted && quest.IsLocked)
	styleBox.BgColor = new Color(0.2f, 0.6f, 0.2f); // Active
else
	styleBox.BgColor = new Color(0.2f, 0.4f, 0.6f); // Available

AddThemeStyleboxOverride("panel", styleBox);


	// 🏳️ Result banners
	var success = GetNode<TextureRect>("SuccessBanner");
	var failed = GetNode<TextureRect>("FailedBanner");

	if (quest.IsComplete)
	{
		success.Visible = !quest.Failed;
		failed.Visible = quest.Failed;
		GameLog.Debug($"[Banner] Showing {(quest.Failed ? "FailedBanner" : "SuccessBanner")} for quest: {quest.Title}");
	}
	else
	{
		success.Visible = false;
		failed.Visible = false;
	}
}






	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		// This must return TRUE if we are dragging an GuestCard
		return data.AsGodotObject() is GuestCard && quest != null && quest.AssignedAdventurers.Count < 3;
	}

public override void _DropData(Vector2 atPosition, Variant data)
{
	var card = data.AsGodotObject() as GuestCard;
	if (card == null || quest == null)
		return;

	if (quest.IsLocked)
		return;

	var adventurer = card.BoundNPC;
	if (adventurer == null || quest.AssignedAdventurers.Contains(adventurer))
		return;

	var guest = card.BoundGuest;

	if (guest != null)
	{
		// 🪑 Remove from table if seated
		if (guest.AssignedTable != null)
		{
			guest.AssignedTable.RemoveGuest(guest);
			guest.AssignedTable = null;
			guest.SeatIndex = null;
		}

		// 🚶 Remove from floor if standing
		if (TavernManager.Instance.GetGuestsInside().Contains(guest))
		{
			TavernManager.Instance.OnGuestRemoved(guest);
		}

		// 🎯 Update guest quest assignment state
		guest.SetState(NPCState.StagingArea);
		guest.DepartureTime = null;

		GameLog.Info($"🧭 {guest.Name} has been assigned to '{quest.Title}' and awaits departure.");
	}

	// 📜 Assign adventurer to quest
	quest.AssignedAdventurers.Add(adventurer);

	// 💨 Clean up and refresh UI
	card.QueueFree();
	UpdateDisplay();
	TavernManager.Instance?.DisplayAdventurers();
	TavernManager.Instance?.UpdateFloorLabel();
}

private void UnassignFromSlot(int index)
{
	if (quest.IsLocked)
	{
		GameLog.Info("⛔ Cannot unassign adventurers — quest is locked.");
		return;
	}

	if (index < quest.AssignedAdventurers.Count)
	{
		var adventurer = quest.AssignedAdventurers[index];
		quest.AssignedAdventurers.RemoveAt(index);

		// 🔍 Look for the guest anywhere (floor or staging)
		var guest = TavernManager.Instance.AllVillagers
			.FirstOrDefault(g => g.BoundNPC != null && g.BoundNPC == adventurer);

		// 👷 Rebuild guest if somehow lost
		if (guest == null)
		{
			guest = new Guest
			{
				Name = adventurer.Name,
				Gender = (Gender)(new Random().Next(0, 2)),
				PortraitId = adventurer.PortraitId,
				BoundNPC = adventurer,
				VisitDay = ClockManager.CurrentDay,
				VisitHour = ClockManager.CurrentTime.Hour,
				WaitDuration = 1,
				StayDuration = 4
			};

			GameLog.Debug($"⚠️ Reconstructed guest for unassigned adventurer: {guest.Name}");
		}

		// 🔄 Reset state and remove seating
		guest.SetState(NPCState.StreetOutside);
		guest.AssignedTable = null;
		guest.SeatIndex = null;
		guest.DepartureTime = null;

		// 🏠 Try to re-admit or return to street
		if (TavernManager.Instance.GetGuestsInside().Count < TavernStats.Instance.MaxFloorGuests)
		{
			TavernManager.Instance.AdmitGuestToTavern(guest);
			GameLog.Info($"↩️ {guest.Name} re-entered the tavern.");
		}
		else
		{
			GuestManager.QueueGuest(guest);
			GameLog.Info($"🚶 {guest.Name} returned to the street.");
		}

		UpdateDisplay();
		TavernManager.Instance.DisplayAdventurers();
	}
}


	public override void _GuiInput(InputEvent @event)
{
	if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
	{
		if (mouseEvent.ButtonIndex == MouseButton.Left)
		{
			var popup = GetTree().Root.GetNode<QuestDetailPopup>("TavernMain/QuestDetailPopup");
			popup.SetQuest(quest);
			popup.Show();
		}

		// 🧹 Removed right-click unassign fallback
	}
}






}
