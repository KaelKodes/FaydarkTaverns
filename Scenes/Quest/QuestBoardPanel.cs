using Godot;
using System;
using System.Collections.Generic;

public partial class QuestBoardPanel : PanelContainer
{
	public static QuestBoardPanel Instance;

	[Export] public NodePath CardHolderPath;
	[Export] public PackedScene QuestCardScene;
	[Export] public NodePath ActiveQuestContainerPath;
	[Export] public NodePath CompletedQuestContainerPath;

	private VBoxContainer activeQuestContainer;
	private VBoxContainer completedQuestContainer;

	public override void _Ready()
	{
		Instance = this;

		// Prefer explicit paths if exported, otherwise fall back to hard-coded tree paths
		activeQuestContainer = !ActiveQuestContainerPath.IsEmpty
			? GetNodeOrNull<VBoxContainer>(ActiveQuestContainerPath)
			: GetNodeOrNull<VBoxContainer>("ScrollContainer/QuestBoard/ActiveQuestPanel/ActiveQuestContainer");

		completedQuestContainer = !CompletedQuestContainerPath.IsEmpty
			? GetNodeOrNull<VBoxContainer>(CompletedQuestContainerPath)
			: GetNodeOrNull<VBoxContainer>("ScrollContainer/QuestBoard/CompletedQuestPanel/CompletedQuestContainer");

		if (activeQuestContainer == null || completedQuestContainer == null)
		{
			GD.PrintErr("[QuestBoardPanel] ❌ Quest containers not found; check node paths.");
			return;
		}

		QuestCardScene ??= GD.Load<PackedScene>("res://Scenes/UI/QuestCard.tscn");

		if (QuestManager.Instance != null)
		{
			QuestManager.Instance.OnQuestsUpdated += RefreshQuestList;
		}

		// Defer initial refresh until the whole scene tree is ready
		CallDeferred(nameof(RefreshQuestList));
	}

	public override void _ExitTree()
	{
		if (Instance == this)
		{
			Instance = null;
		}

		if (QuestManager.Instance != null)
		{
			QuestManager.Instance.OnQuestsUpdated -= RefreshQuestList;
		}
	}

	public void RefreshQuestList()
	{
		// If panel or containers aren't valid / in tree, bail out safely
		if (!IsInsideTree() || !GodotObject.IsInstanceValid(this))
			return;

		if (activeQuestContainer == null ||
			!GodotObject.IsInstanceValid(activeQuestContainer) ||
			completedQuestContainer == null ||
			!GodotObject.IsInstanceValid(completedQuestContainer) ||
			QuestCardScene == null)
		{
			GD.PrintErr("[QuestBoardPanel] ❌ Containers or QuestCardScene missing; aborting RefreshQuestList.");
			return;
		}

		// Clear both containers fully
		activeQuestContainer.QueueFreeChildren();
		completedQuestContainer.QueueFreeChildren();

		var qm = QuestManager.Instance;
		if (qm == null)
		{
			GD.PrintErr("[QuestBoardPanel] QuestManager.Instance is null in RefreshQuestList.");
			return;
		}

		// Active & Failed quests
		foreach (var quest in qm.GetDisplayableBoardQuests())
		{
			var card = QuestCardScene.Instantiate<QuestCard>();
			card.Bind(quest);
			activeQuestContainer.AddChild(card);
		}

		// Completed (Successful) quests
		foreach (var quest in qm.GetCompletedQuests())
		{
			var card = QuestCardScene.Instantiate<QuestCard>();
			card.Bind(quest);
			completedQuestContainer.AddChild(card);
		}
	}
}
