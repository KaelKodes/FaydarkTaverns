using Godot;
using System;
using System.Collections.Generic;

public partial class QuestJournal : Control
{
	public static QuestJournal Instance { get; private set; }

	[Export] public Label PageLabel;

	[Export] public Control GeneralTab1;
	[Export] public Control GeneralTab2;
	[Export] public Control CompletedQuests1;
	[Export] public Control CompletedQuests2;
	[Export] public Control FailedQuests1;
	[Export] public Control FailedQuests2;

	[Export] public TextureButton GeneralButton;
	[Export] public TextureButton CompletedButton;
	[Export] public TextureButton FailedButton;
	[Export] public Button CloseButton;

	// NEW
	[Export] public VBoxContainer CompletedQuestList;
	[Export] public VBoxContainer FailedQuestList;
	[Export] public Control QuestDetailContainer;

	private List<Control> allTabs = new();

	public override void _Ready()
	{
		Instance = this;

		allTabs = new List<Control>
		{
			GeneralTab1, GeneralTab2,
			CompletedQuests1, CompletedQuests2,
			FailedQuests1, FailedQuests2
		};

		ShowGeneral();

		GeneralButton.Pressed += ShowGeneral;
		CompletedButton.Pressed += ShowCompleted;
		FailedButton.Pressed += ShowFailed;
		if (CloseButton != null)
			CloseButton.Pressed += OnClosePressed;
	}

	private void OnClosePressed()
	{
		Visible = false;
	}

	private void HideAll()
	{
		foreach (var tab in allTabs)
			if (tab != null)
				tab.Visible = false;
	}

	private void ShowGeneral()
	{
		PageLabel.Text = "General";
		HideAll();
		GeneralTab1.Visible = true;
		GeneralTab2.Visible = true;
	}

	private void ShowCompleted()
	{
		PageLabel.Text = "Completed Quests";
		HideAll();
		CompletedQuests1.Visible = true;
		CompletedQuests2.Visible = true;
	}

	private void ShowFailed()
	{
		PageLabel.Text = "Failed Quests";
		HideAll();
		FailedQuests1.Visible = true;
		FailedQuests2.Visible = true;
	}

	public void AddCompletedEntry(Quest quest)
	{
		if (CompletedQuestList == null) return;

		var entry = new JournalQuestEntry(quest);
		CompletedQuestList.AddChild(entry);
	}

	public void AddFailedEntry(Quest quest)
	{
		if (FailedQuestList == null) return;

		var entry = new JournalQuestEntry(quest);
		FailedQuestList.AddChild(entry);
	}

	public void ShowQuestDetails(Quest quest)
	{
		if (QuestDetailContainer == null) return;

		foreach (var child in QuestDetailContainer.GetChildren())
			child.QueueFree();

		var detail = new QuestJournalDetailPanel(quest);
		QuestDetailContainer.AddChild(detail);
	}
}
