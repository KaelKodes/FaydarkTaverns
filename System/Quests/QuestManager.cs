using Godot;
using System;
using System.Collections.Generic;

public class QuestManager
{
	// Singleton instance
	private static QuestManager _instance;
	public static QuestManager Instance => _instance ??= new QuestManager();

	private List<Quest> allQuests = new();
	private List<Quest> activeQuests = new();
	public List<Quest> ActiveQuests => activeQuests;
	private List<Quest> completedQuests = new();

	
	public event Action OnQuestsUpdated;


	public int MaxQuestSlots { get; private set; } = 2; // Can be increased via shop later
	private int nextQuestId = 1;
	private List<QuestReport> dailyReports = new();
	
	public bool CanAddQuest() => GetActiveQuestCount() < MaxQuestSlots;


public void AddQuest(Quest quest)
{
	if (quest == null) return;

	if (activeQuests.Count >= MaxQuestSlots)
	{
		GameLog.Debug("⚠️ Quest Board is full. Cannot add quest.");
		return;
	}

	activeQuests.Add(quest);
	GameLog.Debug($"📋 Quest added. Board now has {activeQuests.Count}/{MaxQuestSlots} quests.");

	OnQuestsUpdated?.Invoke(); // 🔁 Signal to refresh Quest UI
}
public List<Quest> GetDisplayableQuests()
{
	return activeQuests.FindAll(q =>
		!q.IsComplete || (q.IsComplete && q.Failed));
}
public List<Quest> GetSuccessfulQuests()
{
	return activeQuests.FindAll(q => q.IsComplete && !q.Failed);
}

public string GetQuestBoardStatusLabel()
{
	return $"Questboard: {GetActiveQuestCount()} / {MaxQuestSlots}";
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

public void RetryQuest(Quest quest)
{
	if (!quest.Failed || !quest.IsComplete) return;

	int retryCost = (int)Math.Floor(quest.Reward * 0.15f);
	if (!TavernManager.Instance.SpendGold(retryCost)) return;

	quest.IsComplete = false;
	quest.Failed = false;
	quest.AssignedAdventurers.Clear(); // Let the player re-assign

	GameLog.Info($"🔁 Quest '{quest.Title}' retried for {retryCost}g.");
	NotifyQuestStateChanged(quest);
	OnQuestsUpdated?.Invoke();
}
public void DismissQuest(Quest quest)
{
	if (!quest.Failed || !quest.IsComplete) return;

	activeQuests.Remove(quest);
	InformantManager.Instance.LowerHappiness(5);


	GameLog.Info($"🗑️ Quest '{quest.Title}' dismissed. Informant less happy.");
	OnQuestsUpdated?.Invoke();
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
	GameLog.Debug($"🔁 Quest state changed: {quest.Title}");

	// Full refresh to relocate quest card
	OnQuestsUpdated?.Invoke();
}

public int GetNextQuestId()
{
	return nextQuestId++;
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
	TavernManager.Instance.IncrementSuccessCombo();
	int tavernExp = CalculateTavernExp(quest, result);
	TavernManager.Instance.GainTavernExp(tavernExp);

	activeQuests.Remove(quest);
	completedQuests.Add(quest); // ✅ add here

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

	public int GetActiveQuestCount()
{
	return activeQuests.FindAll(q => !q.IsComplete || (q.IsComplete && q.Failed)).Count;
}
public List<Quest> GetDisplayableBoardQuests()
{
	return activeQuests.FindAll(q => !q.IsComplete || (q.IsComplete && q.Failed));
}

public List<Quest> GetCompletedQuests()
{
	return completedQuests;
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
