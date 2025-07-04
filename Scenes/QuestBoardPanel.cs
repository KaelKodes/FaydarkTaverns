using Godot;
using System;
using System.Collections.Generic;

public partial class QuestBoardPanel : PanelContainer
{
	private VBoxContainer questHolder;
	[Export] public PackedScene QuestCardScene;
	private VBoxContainer questListContainer;


	public override void _Ready()
	{
		questHolder = GetNode<VBoxContainer>("ScrollContainer/CardHolder");
		questListContainer = GetNode<VBoxContainer>("ScrollContainer/CardHolder"); 
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

	questHolder.QueueFreeChildren();

	foreach (var quest in QuestManager.Instance.GetAvailableQuests())
	{
		var card = QuestCardScene.Instantiate<QuestCard>();
		card.SetQuestData(quest);
		questHolder.AddChild(card);
		GameLog.Debug($"Adding Quest ID: {quest.QuestId}");
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
