using Godot;
using System;

public partial class QuestCard : Panel
{
	[Export] public Label TitleLabel;
	[Export] public Label RegionLabel;
	[Export] public Label TypeLabel;
	[Export] public Label RewardLabel;
	[Export] public Label TimeLabel;

	private Quest quest;

	public override void _Ready()
	{
		TitleLabel ??= GetNode<Label>("VBox/TitleLabel");
		RegionLabel ??= GetNode<Label>("VBox/RegionLabel");
		TypeLabel ??= GetNode<Label>("VBox/TypeLabel");
		RewardLabel ??= GetNode<Label>("VBox/RewardLabel");
		TimeLabel ??= GetNode<Label>("VBox/TimeLabel");
	}

	public void SetQuestData(Quest q)
	{
		quest = q;
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		TitleLabel.Text = quest.Title;
		RegionLabel.Text = $"Region: {quest.Region}";
		TypeLabel.Text = $"Type: {quest.Type}";
		RewardLabel.Text = $"Reward: {quest.Reward}g";
		TimeLabel.Text = $"Est: {quest.GetTotalExpectedTU()} TU / Due: {quest.DeadlineTU}";

		if (quest.IsOverdue)
			Modulate = new Color(1, 0.5f, 0.5f); // Red = overdue
		else if (quest.Assigned)
			Modulate = new Color(0.8f, 0.8f, 0.8f); // Grey = assigned
		else
			Modulate = new Color(1, 1, 1); // White = available
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
	}
}

}
