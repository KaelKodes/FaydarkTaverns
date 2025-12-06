using Godot;
using System;

public class ConsumptionResult
{
	public ConsumptionReaction Reaction;
	public int LoyaltyChange;
	public int GoldEarned;
	public bool SatisfiedNeed; // Hunger or thirst resolved
	public float StayDurationBonus; // Minutes to extend guest visit
	public bool WantsAnotherServing; // True for loved/liked reactions
}
