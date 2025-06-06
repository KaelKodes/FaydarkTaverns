using System;
using System.Collections.Generic;

public class QuestManager
{
	// Singleton instance
	private static QuestManager _instance;
	public static QuestManager Instance => _instance ??= new QuestManager();

	private List<Quest> allQuests = new();
	private int nextQuestId = 1;

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
}
