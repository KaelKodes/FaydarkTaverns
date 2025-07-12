using System;
using System.Collections.Generic;
using FaydarkTaverns.Objects;

public static class QuestGenerator
{
	private static readonly Random rng = new();
	private static readonly List<string> SampleDescriptions = new()
	{
		"Monsters are threatening a village",
		"A noble seeks a stealthy hand",
		"Mysterious ruins need exploring",
		"A wild beast has been terrorizing livestock",
		"An ancient artifact lies hidden in a cave",
		"A merchant has gone missing",
		"Arcane anomalies swirl around the ruins",
		"A dragon's hoard may hold untold riches"
	};

	private static readonly List<string> SampleQuirks = new()
	{
		"Urgent request",
		"Only wants women in the party",
		"Hates rogues",
		"Wants the job done quietly",
		"Loves Bards",
		"Will pay extra if completed before sundown"
	};

	public static Quest GenerateQuest(int id)
	{
		QuestType type = GetRandomEnum<QuestType>();
		Region region = GetRandomEnum<Region>();

		int travelTime = GetTravelTimeFromElderstone(region);
		int taskTime = rng.Next(60, 180); // Task Time: 1–3 hours
		int deadline = travelTime * 2 + taskTime + rng.Next(30, 120); // Add padding for deadline

		List<int> optimalRoles = GetOptimalRoles(type);

		return new Quest
		{
			QuestId = id,
			Title = GenerateQuestTitle(type, region),
			Type = type,
			Region = region,
			TravelHours = travelTime,
			TaskHours = taskTime / 60,
			Description = SampleDescriptions[rng.Next(SampleDescriptions.Count)],
			Quirk = rng.NextDouble() < 0.3 ? SampleQuirks[rng.Next(SampleQuirks.Count)] : null,
			OptimalRoles = optimalRoles,
			Reward = rng.Next(30, 80)
		};
	}

	private static string GenerateQuestTitle(QuestType type, Region region)
	{
		return type switch
		{
			QuestType.Slay => $"Cull Beasts in {region}",
			QuestType.Escort => $"Escort to {region}",
			QuestType.Heist => $"Silent Operation in {region}",
			QuestType.Explore => $"Chart the Depths of {region}",
			QuestType.Tame => $"Tame the Wilds of {region}",
			QuestType.Rescue => $"Rescue in {region}",
			QuestType.Research => $"Research Oddities in {region}",
			QuestType.Treasure => $"Treasure Hunt in {region}",
			_ => $"Quest in {region}"
		};
	}

	private static List<int> GetOptimalRoles(QuestType type)
	{
		return type switch
		{
			QuestType.Slay => new() { 1, 2, 4 },
			QuestType.Escort => new() { 1, 4, 5 },
			QuestType.Heist => new() { 2, 5 },
			QuestType.Explore => new() { 5 },
			QuestType.Tame => new() { 1, 2 },
			QuestType.Rescue => new() { 1, 3, 4 },
			QuestType.Research => new() { 3, 5 },
			QuestType.Treasure => new() { 2, 3 },
			_ => new() { 1, 2, 3 }
		};
	}

	private static T GetRandomEnum<T>() where T : Enum
	{
		Array values = Enum.GetValues(typeof(T));
		return (T)values.GetValue(rng.Next(values.Length));
	}

	public static int GetTravelTimeFromElderstone(Region region)
	{
		return region switch
		{
			Region.Frostmere => 6,
			Region.Ironhold => 4,
			Region.Willowbank => 2,
			Region.Mossvale => 2,
			Region.Brinemarsh => 4,
			Region.Ravenmoor => 4,
			Region.Sundrift => 6,
			Region.AshenRuins => 6,
			Region.HowlingPlains => 10,
			_ => 4
		};
	}

	public static Quest GenerateFromGiver(NPCData giver, Guest guest)
	{
		if (giver == null || guest == null)
		{
			GameLog.Debug("⚠️ Tried to generate a quest from a null giver or guest.");
			return null;
		}

		// ✅ New logic: only generate if the guest is actively standing on the tavern floor
		bool validState = guest.CurrentState == NPCState.TavernFloor &&
						  guest.AssignedTable == null;

		if (!validState)
		{
			GameLog.Debug($"⛔ {guest.Name} tried to post a quest from invalid state: {guest.CurrentState}");
			return null;
		}

		if (giver.PostedQuest != null)
		{
			GameLog.Debug($"⛔ {giver.Name} already has a posted quest.");
			return giver.PostedQuest;
		}

		var quest = GenerateQuest(QuestManager.Instance.GetNextQuestId());
		quest.PostedBy = giver;
		giver.PostedQuest = quest;
		giver.QuestsPosted++;

		GameLog.Debug($"🧾 [SAFE-GEN] {giver.Name} generated a new quest: '{quest.Title}'");
		return quest;
	}
}
