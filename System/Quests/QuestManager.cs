using Godot;
using System;
using System.Collections.Generic;

public class QuestManager
{
	// Singleton instance
	private static QuestManager _instance;
	public static QuestManager Instance => _instance ??= new QuestManager();

	private List<Quest> allQuests = new();
	private int nextQuestId = 1;
	private List<QuestReport> dailyReports = new();

	private QuestManager()
	{
		GenerateDailyQuests(); // Optionally preload quests here
	}

	public void GenerateDailyQuests()
	{
		allQuests.Clear();

		for (int i = 0; i < 16; i++)
		{
			var quest = QuestGenerator.GenerateQuest(nextQuestId++);
			allQuests.Add(quest);
		}
	}

	public List<Quest> GetAvailableQuests()
	{
		// Only return unassigned quests
		return allQuests.FindAll(q => !q.Assigned);
	}

	public Quest GetQuestById(int id)
	{
		return allQuests.Find(q => q.QuestId == id);
	}

	public void ClearUnclaimedQuests()
	{
		allQuests.RemoveAll(q => !q.Assigned);
	}
	
	public List<Quest> GetAcceptedQuests()
{
	return allQuests.FindAll(q => q.IsAccepted && !q.IsComplete);
}

public void LogQuestResult(Quest quest, QuestResult result)
{
	var report = new QuestReport
	{
		Title = quest.Title,
		Success = result.Success,
		Gold = result.GoldEarned,
		ExpEach = result.ExpGained,
		AdventurerNames = quest.AssignedAdventurers.ConvertAll(a => a.Name)
	};

	dailyReports.Add(report);

	GD.Print($"📋 Logged Quest Report: {report.Title} | Success: {report.Success}");
}
public void NotifyQuestStateChanged(Quest quest)
{
	foreach (var node in TavernManager.Instance.GetTree().GetNodesInGroup("QuestCard"))
	{
		if (node is QuestCard qc && qc.HasQuest(quest))
		{
			qc.UpdateDisplay();
			GameLog.Debug($"🔄 Updated QuestCard for accepted quest: {quest.Title}");
		}
	}

	// ✅ Trigger reordering of the cards
	TavernManager.Instance.SortQuestCards();
}

public void CompleteQuest(Quest quest)
{
	if (quest.IsComplete)
	{
		GameLog.Debug($"⚠️ Tried to complete Quest {quest.QuestId}, but it's already complete.");
		return;
	}

	var result = QuestSimulator.Simulate(quest);
	quest.IsComplete = true;
	quest.Failed = !result.Success;

	if (result.Success)
	{
		TavernManager.Instance.AddGold(result.GoldEarned);

		// ✅ Combo bonus!
		TavernManager.Instance.IncrementSuccessCombo();
		int tavernExp = CalculateTavernExp(quest, result);
		TavernManager.Instance.GainTavernExp(tavernExp);

		GameLog.Info($"💰 Player earned {result.GoldEarned}g!");
		GameLog.Info($"✨ Success Combo: {TavernManager.Instance.SuccessComboCount} → +{TavernManager.Instance.SuccessComboCount} EXP bonus");
	}
	else
	{
		TavernManager.Instance.ResetSuccessCombo();
	}

	foreach (var adventurer in quest.AssignedAdventurers)
	{
		adventurer.GainXP(result.ExpGained);
		TavernManager.Instance.DisplayAdventurers();
	}

	LogQuestResult(quest, result);
	NotifyQuestStateChanged(quest);
	GameLog.Info($"🎉 Quest '{quest.Title}' completed. Success: {result.Success}");
}



// 💡 You can define this however you like — basic example:
private int CalculateTavernExp(Quest quest, QuestResult result)
{
	int baseExp = 10;
	int adventurerCount = quest.AssignedAdventurers.Count;
	int comboBonus = TavernManager.Instance.SuccessComboCount;

	return baseExp + (2 * adventurerCount) + comboBonus;
}

public void EnforceDeadline(Quest quest)
{
	if (quest.IsComplete)
	{
		GameLog.Debug($"✅ Quest {quest.QuestId} completed before deadline. No action needed.");
		return;
	}

	quest.IsComplete = true;
	quest.Failed = true;

	LogQuestResult(quest, new QuestResult
	{
		Success = false,
		GoldEarned = 0,
		ExpGained = 0,
		ResolvedAt = ClockManager.CurrentTime
	});

	foreach (var adventurer in quest.AssignedAdventurers)
	{
		TavernManager.Instance.DisplayAdventurers();
	}

	NotifyQuestStateChanged(quest);
	GameLog.Info($"❌ Quest '{quest.Title}' failed due to missed deadline.");
}

}


public class QuestReport
{
	public string Title;
	public bool Success;
	public int Gold;
	public int ExpEach;
	public List<string> AdventurerNames;
}
