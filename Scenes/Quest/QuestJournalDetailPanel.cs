using Godot;

public partial class QuestJournalDetailPanel : VBoxContainer
{
	public QuestJournalDetailPanel(Quest quest)
	{
		Label title = new Label { Text = quest.Title };
		Label region = new Label { Text = $"Region: {quest.Region}" };
		Label reward = new Label { Text = $"Reward: {quest.Reward}g" };
		Label success = new Label { Text = quest.Failed ? "❌ Failed" : "✔ Completed" };

		AddChild(title);
		AddChild(region);
		AddChild(reward);
		AddChild(success);
	}
}
