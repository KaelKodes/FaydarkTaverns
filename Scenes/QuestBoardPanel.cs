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
	
	public void SortQuestCards()
{
	var container = GetNode<VBoxContainer>("ScrollContainer/CardHolder");

	var cards = new List<QuestCard>();
	foreach (var child in container.GetChildren())
	{
		if (child is QuestCard card)
			cards.Add(card);
	}

	int GetPriority(Quest q)
	{
		if (q.IsAccepted && !q.IsComplete) return 0; // Active
		if (!q.IsAccepted && !q.IsComplete) return 1; // Unaccepted
		if (q.IsComplete && !q.Failed) return 2; // Complete Success
		if (q.IsComplete && q.Failed) return 3; // Complete Failed
		return 4; // Fallback
	}

	cards.Sort((a, b) =>
	{
		int aPriority = GetPriority(a.Quest);
		int bPriority = GetPriority(b.Quest);
		if (aPriority != bPriority)
			return aPriority.CompareTo(bPriority);

		return a.Quest.QuestId.CompareTo(b.Quest.QuestId);
	});

	foreach (var card in cards)
		container.RemoveChild(card);

	foreach (var card in cards)
	{
		container.AddChild(card);
		card.UpdateDisplay();
	}

	container.QueueSort();
	GameLog.Debug($"✅ Re-sorted {cards.Count} quest cards.");
}


}
