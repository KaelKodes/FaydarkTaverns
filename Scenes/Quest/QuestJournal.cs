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
	
	
	// GeneralTab1
	[Export] public SpinBox QuestAttemptedSpinbox { get; set; }
	[Export] public SpinBox QuestsSucceededSpinbox { get; set; }
	[Export] public SpinBox QuestsFailedSpinbox { get; set; }
	[Export] public SpinBox SuccessRateSpinbox { get; set; }

	// GeneralTab2
	[Export] public Label FavoriteQuestTypeLabel { get; set; }
	[Export] public Label FavoriteQuestGiverLabel { get; set; }
	[Export] public SpinBox GPQSpinbox { get; set; }
	[Export] public SpinBox HighestPayoutSpinbox { get; set; }

	
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
	public void RefreshGeneralStats()
{
	var stats = QuestManager.Instance.Stats;

	QuestAttemptedSpinbox.Value = stats.Attempts;
	QuestsSucceededSpinbox.Value = stats.Successes;
	QuestsFailedSpinbox.Value = stats.Failures;
	SuccessRateSpinbox.Value = stats.SuccessRate;

	FavoriteQuestTypeLabel.Text = stats.FavoriteQuestType;
	FavoriteQuestGiverLabel.Text = stats.FavoriteQuestGiver;

	GPQSpinbox.Value = stats.GoldEarned;
	HighestPayoutSpinbox.Value = stats.HighestPayout;
}

public void OpenJournal()
{
	Visible = true;
	RefreshGeneralStats();
}


	
}
