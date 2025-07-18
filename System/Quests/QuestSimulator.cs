using Godot;
using System;
using System.Collections.Generic;
using FaydarkTaverns.Objects;

public static class QuestSimulator
{
	public static QuestResult Simulate(Quest quest)
{
	if (quest == null || quest.AssignedAdventurers.Count == 0)
	{
		GD.PrintErr("⚠️ QuestSimulator.Simulate called with null or empty quest.");
		return new QuestResult
		{
			Success    = false,
			GoldEarned = 0,
			ExpGained  = 0,
			ResolvedAt = ClockManager.CurrentTime
		};
	}

	// 1) Determine success
	float matchScore = CalculatePartyMatch(quest);
	bool success     = matchScore >= 0.5f;

	// 2) If failure, immediate zero payout
	if (!success)
	{
		GD.Print($"❌ Quest '{quest.Title}' failed (Match Score: {matchScore:F2}). No reward.");
		return new QuestResult
		{
			Success    = false,
			GoldEarned = 0,
			ExpGained  = 0,
			ResolvedAt = ClockManager.CurrentTime
		};
	}

	// 3) Compute base rewards for success
	int baseReward = quest.Reward;
	int baseXP     = 100;

	// 4) Party‐size bonus (max party is 3)
	const int MaxPartySize = 3;
	int actual        = quest.AssignedAdventurers.Count;
	int missing       = Mathf.Clamp(MaxPartySize - actual, 0, MaxPartySize);
	float bonusFactor = 1f + (missing * 0.10f);

	// 5) Final payout
	int reward = Mathf.RoundToInt(baseReward * bonusFactor);
	int xp     = Mathf.RoundToInt(baseXP     * bonusFactor);

	// 6) Log risk bonus if applied
	if (missing > 0)
	{
		GameLog.Info($"🎲 Risk bonus applied: {missing} fewer adventurer(s). Bonus x{bonusFactor:F2}");
	}

	GD.Print($"✅ Quest '{quest.Title}' succeeded (Match Score: {matchScore:F2}). Reward: {reward}g, {xp} XP.");

	return new QuestResult
	{
		Success    = true,
		GoldEarned = reward,
		ExpGained  = xp,
		ResolvedAt = ClockManager.CurrentTime
	};
}


	private static float CalculatePartyMatch(Quest quest)
{
	var req = quest.Requirements;
	if (req == null)
		return 0f;

	// 1) Check that the *pooled* total of each required stat meets its threshold
	int requiredCount = req.RequiredStats.Count;
	int metRequired   = 0;
	foreach (var kv in req.RequiredStats)
	{
		// Sum this stat across the whole party
		int total = 0;
		foreach (var a in quest.AssignedAdventurers)
			total += GetStatValue(a, kv.Key);

		if (total >= kv.Value)
			metRequired++;
		else
			return 0f;     // pooled total failed → total failure
	}

	// 2) Count how many bonus thresholds you *also* meet (pooled)
	int bonusCount = 0;
	foreach (var kv in req.BonusStats)
	{
		int total = 0;
		foreach (var a in quest.AssignedAdventurers)
			total += GetStatValue(a, kv.Key);

		if (total >= kv.Value)
			bonusCount++;
	}

	// 3) Compute and return normalized score
	int matchCount = metRequired + bonusCount;
	int totalCount = requiredCount + req.BonusStats.Count;
	return totalCount > 0
		? (float)matchCount / totalCount
		: 0f;
}


// Helper to read a Skill value from NPCData
public static int GetStatValue(NPCData data, Skill skill)
{
	return skill switch
	{
		Skill.Tank          => data.Tank,
		Skill.pDPS          => data.pDPS,
		Skill.mDPS          => data.mDPS,
		Skill.Healer        => data.Healer,
		Skill.Athletics     => data.Athletics,
		Skill.Tracking      => data.Tracking,
		Skill.LockPicking   => data.LockPicking,
		Skill.Buffing       => data.Buffing,
		Skill.Debuffing     => data.Debuffing,
		Skill.Transport     => data.Transport,
		Skill.Taming        => data.Taming,
		Skill.SpellResearch => data.SpellResearch,
		Skill.Investigation => data.Investigation,
		_                   => 0
	};
}
}

public class QuestResult
{
	public bool Success;
	public int GoldEarned;
	public int ExpGained;
	public DateTime ResolvedAt;
}
