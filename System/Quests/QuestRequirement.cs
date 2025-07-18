// res://System/Quests/QuestStatRequirement.cs
using System.Collections.Generic;

public class QuestStatRequirement
{
	public Dictionary<Skill,int> RequiredStats { get; } = new();
	public Dictionary<Skill,int> BonusStats    { get; } = new();
}
