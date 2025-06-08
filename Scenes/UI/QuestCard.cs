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

	public override void _Ready()
	{
		TitleLabel ??= GetNode<Label>("VBox/TitleLabel");
		RegionLabel ??= GetNode<Label>("VBox/RegionLabel");
		TypeLabel ??= GetNode<Label>("VBox/TypeLabel");
		RewardLabel ??= GetNode<Label>("VBox/RewardLabel");
		TimeLabel ??= GetNode<Label>("VBox/TimeLabel");

		// Locate party slot panels and their corresponding labels
		for (int i = 1; i <= 3; i++)
		{
			var panel = GetNodeOrNull<Panel>($"PartySlotContainer/PartySlot{i}");
			if (panel != null)
			{
				PartySlots.Add(panel);
				var label = panel.GetNodeOrNull<Label>("AdventurerLabel");
				if (label != null)
					partySlotLabels.Add(label);
			}
		}
	}

	public void SetQuestData(Quest q)
	{
		quest = q;
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		if (quest == null) return;

		TitleLabel.Text = quest.Title;
		RegionLabel.Text = $"Region: {quest.Region}";
		TypeLabel.Text = $"Type: {quest.Type}";
		RewardLabel.Text = $"Reward: {quest.Reward}g";
		TimeLabel.Text = $"Est: {quest.GetTotalExpectedTU()} TU / Due: {quest.DeadlineTU}";

		Modulate = quest.IsOverdue ? new Color(1, 0.5f, 0.5f) :
				   quest.Assigned ? new Color(0.8f, 0.8f, 0.8f) :
				   new Color(1, 1, 1);

		for (int i = 0; i < partySlotLabels.Count; i++)
{
	partySlotLabels[i].Text = i < quest.AssignedAdventurers.Count
		? quest.AssignedAdventurers[i].Name
		: "[ Empty ]"; // Optional placeholder for clarity
}

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
		if (index < quest.AssignedAdventurers.Count)
		{
			quest.AssignedAdventurers.RemoveAt(index);
			UpdateDisplay();
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
				Vector2 clickPos = GetLocalMousePosition();
				for (int i = 0; i < partySlotLabels.Count; i++)
				{
					if (partySlotLabels[i].GetGlobalRect().HasPoint(GetGlobalMousePosition()) &&
						!string.IsNullOrEmpty(partySlotLabels[i].Text))
					{
						UnassignFromSlot(i);
						break;
					}
				}
			}
		}
	}
}
