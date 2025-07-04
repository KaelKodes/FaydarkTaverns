using Godot;
using System;
using System.Collections.Generic;

public static class QuestSimulator
{
	public static QuestResult Simulate(Quest quest)
	{
		if (quest == null || quest.AssignedAdventurers.Count == 0)
{
	GD.PrintErr("⚠️ QuestSimulator.Simulate called with null or empty quest.");
	return new QuestResult
	{
		Success = false,
		GoldEarned = 0,
		ExpGained = 0,
		ResolvedAt = ClockManager.CurrentTime
	};
}


		float matchScore = CalculatePartyMatch(quest);
		bool success = matchScore >= 0.5f;

		int baseReward = quest.Reward;
int baseXP = success ? 100 : 40;

// 🎯 Risk-Reward Bonus: +10% per adventurer not sent
int expected = quest.OptimalRoles.Count;
int actual = quest.AssignedAdventurers.Count;
int missing = Mathf.Clamp(expected - actual, 0, expected);

float bonusFactor = 1f + (missing * 0.10f);

int reward = success
	? Mathf.RoundToInt(baseReward * bonusFactor)
	: Mathf.RoundToInt(baseReward * 0.5f);

int xp = Mathf.RoundToInt(baseXP * bonusFactor);

if (missing > 0)
{
	GameLog.Info($"🎲 Risk bonus applied: {missing} adventurer(s) under optimal. Bonus x{bonusFactor:F2}");
}


		GD.Print($"🎲 Simulating quest '{quest.Title}' | Match Score: {matchScore:F2} | Success: {success}");

		return new QuestResult
		{
			Success = success,
			GoldEarned = reward,
			ExpGained = xp,
			ResolvedAt = ClockManager.CurrentTime
		};
	}

	private static float CalculatePartyMatch(Quest quest)
	{
		if (quest.OptimalRoles == null || quest.OptimalRoles.Count == 0)
			return 0f;

		int matchCount = 0;
		HashSet<int> usedRoles = new();

		foreach (var optimalRole in quest.OptimalRoles)
		{
			if (quest.AssignedAdventurers.Exists(a => a.RoleId == optimalRole && !usedRoles.Contains(a.RoleId)))
			{
				matchCount++;
				usedRoles.Add(optimalRole);
			}
		}

		return (float)matchCount / quest.OptimalRoles.Count;
	}
}

public class QuestResult
{
	public bool Success;
	public int GoldEarned;
	public int ExpGained;
	public DateTime ResolvedAt;
}
