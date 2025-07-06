using Godot;
using System;
using System.Collections.Generic;

public partial class QuestBoardPanel : PanelContainer
{
	[Export] public NodePath CardHolderPath;
	[Export] public PackedScene QuestCardScene;
	[Export] public NodePath ActiveQuestContainerPath;
[Export] public NodePath CompletedQuestContainerPath;

private VBoxContainer activeQuestContainer;
private VBoxContainer completedQuestContainer;



	private VBoxContainer cardHolder;

	public override void _Ready()
{
	// ScrollContainer > QuestBoard > Active/Completed
	activeQuestContainer = GetNode<VBoxContainer>("ScrollContainer/QuestBoard/ActiveQuestPanel/ActiveQuestContainer");
	completedQuestContainer = GetNode<VBoxContainer>("ScrollContainer/QuestBoard/CompletedQuestPanel/CompletedQuestContainer");

	QuestCardScene ??= GD.Load<PackedScene>("res://Scenes/UI/QuestCard.tscn");

	// Subscribe to QuestManager updates
	QuestManager.Instance.OnQuestsUpdated += RefreshQuestList;

	// Initial population
	RefreshQuestList();
}


	public void RefreshQuestList()
{
	if (activeQuestContainer == null || completedQuestContainer == null || QuestCardScene == null)
	{
		GD.PrintErr("❌ One or more quest containers are missing!");
		return;
	}

	// 🧼 Clear both containers fully — this is critical
	activeQuestContainer.QueueFreeChildren();
	completedQuestContainer.QueueFreeChildren();

	// 🟢 Active & Failed quests
	foreach (var quest in QuestManager.Instance.GetDisplayableBoardQuests())
	{
		var card = QuestCardScene.Instantiate<QuestCard>();
		card.Bind(quest);
		activeQuestContainer.AddChild(card);
	}

	// 🟡 Completed (Successful) quests
	foreach (var quest in QuestManager.Instance.GetCompletedQuests())
	{
		var card = QuestCardScene.Instantiate<QuestCard>();
		card.Bind(quest);
		completedQuestContainer.AddChild(card);
	}
}



}
