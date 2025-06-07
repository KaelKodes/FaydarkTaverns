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
	[Export] public Panel[] PartySlotNodes;
	private List<Panel> PartySlots = new();   // Internal list of panels resolved in _Ready()




	private List<Label> partySlotLabels = new();
	private Quest quest;

	public override void _Ready()
{
	TitleLabel ??= GetNode<Label>("VBox/TitleLabel");
	RegionLabel ??= GetNode<Label>("VBox/RegionLabel");
	TypeLabel ??= GetNode<Label>("VBox/TypeLabel");
	RewardLabel ??= GetNode<Label>("VBox/RewardLabel");
	TimeLabel ??= GetNode<Label>("VBox/TimeLabel");

	// Convert exported paths to actual nodes
	foreach (var node in PartySlotNodes)
{
	if (node is Panel panel)
		PartySlots.Add(panel);
}


	// For slot text display
	for (int i = 1; i <= 3; i++)
	{
		var label = GetNodeOrNull<Label>($"PartySlotContainer/PartySlot{i}/AdventurerLabel");
		if (label != null)
			partySlotLabels.Add(label);
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
				: "";
		}
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		return data.AsGodotObject() is Adventurer && quest.AssignedAdventurers.Count < 3;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		if (data.AsGodotObject() is Adventurer adventurer)
		{
			AssignToSlot(adventurer);
		}
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
