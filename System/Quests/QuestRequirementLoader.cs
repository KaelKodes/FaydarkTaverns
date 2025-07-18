// res://System/Quests/QuestRequirementsLoader.cs
using Godot;
using System;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;

public static class QuestRequirementsLoader
{
	// [QuestType][Level] → requirements
	private static Dictionary<QuestType, Dictionary<int, QuestStatRequirement>> _cache;

	public static void Load(string jsonPath)
	{
		var file = FileAccess.Open(jsonPath, FileAccess.ModeFlags.Read);
		if (file == null)
			throw new Exception($"Couldn't open {jsonPath}");

		var sb = new StringBuilder();
		while (!file.EofReached())
			sb.AppendLine(file.GetLine());

		var root = JsonDocument.Parse(sb.ToString()).RootElement;
		_cache = new();

		foreach (var questTypeProp in root.EnumerateObject())
		{
			var qType = Enum.Parse<QuestType>(questTypeProp.Name);
			var levelDict = new Dictionary<int, QuestStatRequirement>();

			foreach (var levelProp in questTypeProp.Value.EnumerateObject())
			{
				int lvl = int.Parse(levelProp.Name);
				var req = new QuestStatRequirement();

				foreach (var stat in levelProp.Value.GetProperty("requiredStats").EnumerateObject())
				{
					var skill = Enum.Parse<Skill>(stat.Name);
					req.RequiredStats[skill] = stat.Value.GetInt32();
				}

				foreach (var stat in levelProp.Value.GetProperty("bonusStats").EnumerateObject())
				{
					var skill = Enum.Parse<Skill>(stat.Name);
					req.BonusStats[skill] = stat.Value.GetInt32();
				}

				levelDict[lvl] = req;
			}
			_cache[qType] = levelDict;
		}
	}

	public static QuestStatRequirement Get(QuestType type, int level)
	{
		if (_cache.TryGetValue(type, out var levels) && levels.TryGetValue(level, out var req))
			return req;

		// Auto-generate higher levels by “+1” rules
		if (level > 1)
		{
			var prev = Get(type, level - 1);
			var gen  = new QuestStatRequirement();
			foreach (var kv in prev.RequiredStats)
				gen.RequiredStats[kv.Key] = kv.Value + 1;
			foreach (var kv in prev.BonusStats)
				gen.BonusStats[kv.Key] = kv.Value + 1;

			_cache[type][level] = gen;
			return gen;
		}

		throw new KeyNotFoundException($"No requirements for {type} level {level}");
	}
}
