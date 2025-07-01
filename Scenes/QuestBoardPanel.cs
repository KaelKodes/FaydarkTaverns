using Godot;
using System;
using System.Collections.Generic;

public partial class QuestBoardPanel : PanelContainer
{
	private VBoxContainer questHolder;
	[Export] public PackedScene QuestCardScene;
	[Export] public VBoxContainer questListContainer;

	public override void _Ready()
	{
		questHolder = GetNode<VBoxContainer>("ScrollContainer/CardHolder");
		QuestCardScene ??= GD.Load<PackedScene>("res://Scenes/UI/QuestCard.tscn");

		PopulateQuests();
	}

	public void PopulateQuests()
{
	if (QuestManager.Instance == null)
	{
		GD.PrintErr("❌ QuestManager.Instance is null!");
		return;
	}

	ClearContainerChildren(questHolder);

	foreach (var quest in QuestManager.Instance.GetAvailableQuests())
	{
		var card = QuestCardScene.Instantiate<QuestCard>();
		card.SetQuestData(quest); // ✅ Use real quest from QuestManager
		questHolder.AddChild(card);
		GameLog.Debug($"Adding Quest ID: {quest.QuestId}");

	}
}


	private void ClearContainerChildren(Container container)
	{
		foreach (Node child in container.GetChildren())
		{
			child.QueueFree();
		}
	}
}
