using Godot;

public partial class JournalQuestEntry : Button
{
	public Quest BoundQuest;

	public JournalQuestEntry(Quest quest)
	{
		BoundQuest = quest;
		Text = quest.Title;
		Pressed += OnPressed;
	}

	private void OnPressed()
	{
		QuestJournal.Instance?.ShowQuestDetails(BoundQuest);
	}
}
