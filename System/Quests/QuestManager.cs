using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FaydarkTaverns.Objects;

public class QuestManager
{
	// Automatically load our quest-requirements JSON once
	static QuestManager()
	{
	   QuestRequirementsLoader.Load("res://System/Quests/QuestRequirements.json");
	}
	
	// Singleton instance
	private static QuestManager _instance;
	public static QuestManager Instance => _instance ??= new QuestManager();

	private List<Quest> allQuests = new();
	private List<Quest> activeQuests = new();
	public List<Quest> ActiveQuests => activeQuests;
	private List<Quest> completedQuests = new();
	private bool questJournalUnlocked = false;

	
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

	OnQuestsUpdated?.Invoke();

	// ✅ Let tavern recheck quest posting
	TavernManager.Instance?.RecheckQuestPosting();
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
	if (quest == null)
		return;

	// 🧹 Remove from active list
	ActiveQuests.Remove(quest);

	// 🗑️ Remove from board UI
	var tree = Godot.Engine.GetMainLoop() as SceneTree;
	if (tree != null)
	{
		foreach (var card in tree.GetNodesInGroup("QuestCard"))
		{
			if (card is QuestCard qc && qc.HasQuest(quest))
			{
				qc.QueueFree();
				break;
			}
		}
	}

	// 🧓 Tag NPC as unavailable to repost
	if (quest.PostedBy != null)
	{
		quest.PostedBy.PostedQuest = null;
		quest.PostedBy.HasPostedToday = true;
		quest.PostedBy.AdjustHappiness(-2);
		GameLog.Info($"📜😤 {quest.PostedBy.Name} will not post again until tomorrow.");
	}

	GameLog.Info($"🗑️ Dismissed quest: {quest.Title}");
}

public void UnassignAdventurer(Quest quest, NPCData npc)
{
	if (!quest.AssignedAdventurers.Contains(npc))
		return;

	quest.AssignedAdventurers.Remove(npc);

	var guest = TavernManager.Instance.AllVillagers
		.FirstOrDefault(g => g.BoundNPC != null && g.BoundNPC == npc);

	if (guest != null)
	{
		// ✅ Prevent duplication in queue
		if (GuestManager.GuestsOutside.Contains(guest) ||
			GuestManager.GuestsInside.Contains(guest))
		{
			GameLog.Debug($"⚠️ {guest.Name} already tracked. Skipping duplicate add.");
			return;
		}

		// 🧹 Clear tavern-specific assignments
		guest.AssignedTable = null;
		guest.SeatIndex = null;

		// 🚪 Update state
		guest.SetState(NPCState.StreetOutside);
		if (guest.BoundNPC != null)
			guest.BoundNPC.State = guest.CurrentState; // <-- ADD THIS LINE

		// ⏳ Ensure they’ll leave eventually
		int fallbackDuration = 3;
		guest.DepartureTime = ClockManager.CurrentTime.AddHours(guest.StayDuration > 0 ? guest.StayDuration : fallbackDuration);

		// 🧭 Queue for reintegration
		GuestManager.QueueGuest(guest);
		GameLog.Info($"👋 {guest.Name} was unassigned and returned to the street.");
	}
}




public void HandleQuestReturn(Quest quest)
{
	foreach (var adventurer in quest.AssignedAdventurers)
	{
		var guest = TavernManager.Instance.AllVillagers
			.FirstOrDefault(g => g.BoundNPC != null && g.BoundNPC == adventurer);

		if (guest == null)
		{
			GameLog.Debug($"⚠️ Could not find guest for returning adventurer: {adventurer.Name}");
			continue;
		}

		// ✅ Safety check: prevent re-adding if already tracked
		if (GuestManager.GuestsOutside.Contains(guest) ||
			GuestManager.GuestsInside.Contains(guest))
		{
			GameLog.Debug($"⚠️ {guest.Name} already tracked. Skipping duplicate add.");
			continue;
		}

		// ✅ Reset guest state
		guest.AssignedTable = null;
		guest.SeatIndex = null;

		guest.SetState(NPCState.StreetOutside);
		if (guest.BoundNPC != null)
			guest.BoundNPC.State = guest.CurrentState; // <-- ADD THIS LINE

		// ⏳ Set DepartureTime for auto-leave
		int fallbackDuration = 3;
		guest.DepartureTime = ClockManager.CurrentTime.AddHours(guest.StayDuration > 0 ? guest.StayDuration : fallbackDuration);

		// ✅ Add to outside queue
		GuestManager.QueueGuest(guest);
		GameLog.Info($"🧭 {guest.Name} has returned from '{quest.Title}' and waits outside.");
	}
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
	HandleQuestReturn(quest);

	if (result.Success)
	{
		TavernManager.Instance.AddGold(result.GoldEarned);
		TavernManager.Instance.IncrementSuccessCombo();
		int tavernExp = CalculateTavernExp(quest, result);
		TavernManager.Instance.GainTavernExp(tavernExp);

		activeQuests.Remove(quest);
		completedQuests.Add(quest);

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
	}
	TavernManager.Instance.DisplayAdventurers();
	LogQuestResult(quest, result);
	NotifyQuestStateChanged(quest);
	GameLog.Info($"🎉 Quest '{quest.Title}' completed. Success: {result.Success}");

	// Unlock Quest Journal on first completion (success OR fail)
	if (!questJournalUnlocked)
	{
		questJournalUnlocked = true;
		QuestJournalUnlockController.Instance?.TryUnlockJournal();
	}
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

	// Also unlock journal on a first-ever failure
	if (!questJournalUnlocked)
	{
		questJournalUnlocked = true;
		QuestJournalUnlockController.Instance?.TryUnlockJournal();
	}
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
public void IncreaseQuestLimit()
{
	MaxQuestSlots++;
	GameLog.Info($"📈 Quest board capacity increased to {MaxQuestSlots}.");
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
