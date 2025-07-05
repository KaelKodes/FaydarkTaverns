using Godot;
using System;
using System.Collections.Generic;

public partial class QuestBoardPanel : PanelContainer
{
	[Export] public NodePath CardHolderPath;
	[Export] public PackedScene QuestCardScene;

	private VBoxContainer cardHolder;

	public override void _Ready()
	{
		cardHolder = GetNode<VBoxContainer>("ScrollContainer/CardHolder");
		QuestCardScene ??= GD.Load<PackedScene>("res://Scenes/UI/QuestCard.tscn");

		// Subscribe to updates
		QuestManager.Instance.OnQuestsUpdated += RefreshQuestList;

		// Initial load
		RefreshQuestList();
	}

	public void RefreshQuestList()
	{
		if (cardHolder == null || QuestCardScene == null)
		{
			GD.PrintErr("❌ CardHolder or QuestCardScene is missing!");
			return;
		}

		cardHolder.QueueFreeChildren();

		var quests = QuestManager.Instance.GetActiveQuests();
		foreach (var quest in quests)
		{
			var card = QuestCardScene.Instantiate<QuestCard>();
			card.Bind(quest);
			cardHolder.AddChild(card);
		}

		SortQuestCards();
	}

	public void SortQuestCards()
	{
		var cards = new List<QuestCard>();
		foreach (var child in cardHolder.GetChildren())
		{
			if (child is QuestCard card)
				cards.Add(card);
		}

		int GetPriority(Quest q)
		{
			if (q.IsAccepted && !q.IsComplete) return 0;
			if (!q.IsAccepted && !q.IsComplete) return 1;
			if (q.IsComplete && !q.Failed) return 2;
			if (q.IsComplete && q.Failed) return 3;
			return 4;
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
		{
			cardHolder.RemoveChild(card);
			cardHolder.AddChild(card);
			card.UpdateDisplay();
		}

		cardHolder.QueueSort();
	}
}
