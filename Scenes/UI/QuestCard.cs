using Godot;
using System;
using System.Collections.Generic;

public partial class QuestCard : Panel
{
	[Export] public Label TitleLabel;
	[Export] public Label RegionLabel;
	[Export] public Label TypeLabel;
	[Export] public Label RewardLabel;
	[Export] public Label TimeLabel;

	// Party slot containers
	private List<Panel> PartySlots = new();
	private List<Label> partySlotLabels = new();
	private Quest quest;
	public bool HasQuest(Quest q)
{
	return ReferenceEquals(quest, q);
}






	public override void _Ready()
{
	GD.Print("QuestCard: Running _Ready()");

	try
	{
		TitleLabel = GetNode<Label>("VBox/TitleLabel");
		RegionLabel = GetNode<Label>("VBox/RegionLabel");
		TypeLabel = GetNode<Label>("VBox/TypeLabel");
		RewardLabel = GetNode<Label>("VBox/RewardLabel");
		TimeLabel = GetNode<Label>("VBox/TimeLabel");

		for (int i = 1; i <= 3; i++)
		{
			var panel = GetNode<Panel>($"VBox/PartySlotContainer/PartySlot{i}");
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
}

public override void _ExitTree()
{
	RemoveFromGroup("QuestCard"); // ✅ Clean up when node is freed
}





	public void SetQuestData(Quest q)
{
	quest = q;
	GD.Print($"Card bound to Quest ID: {quest.QuestId}");
	GD.Print($"[CARD] Card node: {Name} | Bound quest ref: {q.GetHashCode()} | Title: {q.Title} | QuestId: {q.QuestId}");
	if (IsInsideTree()) UpdateDisplay();
}



	public void UpdateDisplay()
{
	if (quest == null)
	{
		GD.PrintErr("UpdateDisplay called with null quest.");
		return;
	}
	GD.Print($"[UpdateDisplay] Quest {quest.QuestId} | {quest.Title} | IsAccepted: {quest.IsAccepted} | IsLocked: {quest.IsLocked}");

	// 🟩 Update label values
	TitleLabel.Text = quest.Title;
	RegionLabel.Text = quest.Region.ToString();
	TypeLabel.Text = quest.Type.ToString();
	RewardLabel.Text = $"{quest.Reward}g";
	TimeLabel.Text = $"{quest.GetTotalExpectedTU()} TU";

	GD.Print($"Updating display for quest: {quest.Title}");
	GD.Print($"Assigned Adventurers: {quest.AssignedAdventurers.Count}");

	for (int i = 0; i < partySlotLabels.Count; i++)
	{
		if (i < quest.AssignedAdventurers.Count)
		{
			var fullName = quest.AssignedAdventurers[i].Name;
			var firstName = fullName.Split(' ')[0];
			partySlotLabels[i].Text = firstName;

			GD.Print($"  -> Slot {i}: {firstName}");
		}
		else
		{
			partySlotLabels[i].Text = "";
			GD.Print($"  -> Slot {i}: (empty)");
		}
	}

	// 🎨 Background color logic
var styleBox = new StyleBoxFlat(); // ⛔️ don't fetch from theme

// Failure state
if (quest.IsComplete && quest.Failed)
{
	styleBox.BgColor = new Color(0.1f, 0.1f, 0.1f); // deep grey/black
}
// Active quest (ONLY accepted + locked)

else if (quest.IsAccepted && quest.IsLocked)
{
	GD.Print($"[STYLE] Quest {quest.QuestId} | IsAccepted: {quest.IsAccepted} | IsLocked: {quest.IsLocked}");

	float progress = (float)quest.GetElapsedTU() / quest.GetTotalExpectedTU();

	if (progress < 0.5f)
		styleBox.BgColor = new Color(0.2f, 0.6f, 0.2f); // green
	else
	{
		float redIntensity = Mathf.Clamp(progress, 0.5f, 1.0f);
		styleBox.BgColor = new Color(redIntensity, 0.2f, 0.2f); // more red over time
	}
}
// Default, unaccepted quest
else
{
	styleBox.BgColor = new Color(0.2f, 0.4f, 0.6f); // blue default
}

AddThemeStyleboxOverride("panel", styleBox);
}





	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		// This must return TRUE if we are dragging an AdventurerCard
		return data.AsGodotObject() is AdventurerCard && quest != null && quest.AssignedAdventurers.Count < 3;
	}



	public override void _DropData(Vector2 atPosition, Variant data)
{
	var card = data.AsGodotObject() as AdventurerCard;
	if (card == null || quest == null)
		return;

	// 🛑 Prevent dropping if quest is locked
	if (quest.IsLocked)
		return;

	var adventurer = card.BoundAdventurer;
	if (adventurer == null || quest.AssignedAdventurers.Contains(adventurer))
		return;

	// Assign the adventurer
	quest.AssignedAdventurers.Add(adventurer);

	// Remove the visual card
	card.QueueFree();

	// ✅ Refresh the slot labels
	UpdateDisplay();
}





	private void AssignToSlot(Adventurer adventurer)
	{
		if (quest.AssignedAdventurers.Contains(adventurer) || quest.AssignedAdventurers.Count >= 3)
			return;

		quest.AssignedAdventurers.Add(adventurer);
		UpdateDisplay();
	}

	private void UnassignFromSlot(int index)
{
	if (quest.IsLocked)
	{
		GD.Print("⛔ Cannot unassign adventurers — quest is locked.");
		return;
	}

	if (index < quest.AssignedAdventurers.Count)
	{
		var adventurer = quest.AssignedAdventurers[index];
		quest.AssignedAdventurers.RemoveAt(index);
		UpdateDisplay();
		TavernManager.Instance?.RestoreAdventurerToRoster(adventurer);
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
		else if (mouseEvent.ButtonIndex == MouseButton.Right)
		{
			// ✅ Just remove the last adventurer from the party
			for (int i = 0; i < partySlotLabels.Count; i++)
			{
				if (!string.IsNullOrEmpty(partySlotLabels[i].Text))
				{
					UnassignFromSlot(i);
					break;
				}
			}
		}
	}
}


}
