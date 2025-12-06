using Godot;
using System;
using FaydarkTaverns.Objects;
using System.Collections.Generic;

public static class ConsumptionManager
{
	// =======================================================
	//  PUBLIC ENTRY POINTS
	// =======================================================

	public static ConsumptionResult ConsumeFood(Guest guest, FoodItem foodItem, bool fastService = false)
	{
		if (guest == null || guest.BoundNPC == null || foodItem == null)
			return new ConsumptionResult { Reaction = ConsumptionReaction.Neutral };

		var npc = guest.BoundNPC;

		// 1) Taste evaluation
		var reaction = TasteResolver.Evaluate(
			guest,
			isDrink: false,
			rawFlavors: foodItem.FlavorProfiles,
			ingredients: foodItem.Ingredients
		);

		// 2) Loyalty update
		int loyaltyDelta = GetLoyaltyDelta(reaction);
		if (fastService && reaction != ConsumptionReaction.Disliked)
			loyaltyDelta += 1;

		npc.LoyaltyRating = Mathf.Clamp(npc.LoyaltyRating + loyaltyDelta, -100f, 100f);

		// 3) Calculate gold
		int goldEarned = CalculateGoldEarned(npc, foodItem.BasePrice, reaction);
		if (goldEarned > 0 && TavernManager.Instance != null)
			TavernManager.Instance.AddGold(goldEarned);

		// 4) Resolve hunger
		npc.IsHungry = false;
		npc.HasEatenToday = true;

		// Record type for repeat requests
		npc.LastConsumedWasFood = true;
		npc.LastConsumedWasDrink = false;

		// 5) Stay duration bonus
		float stayBonus = GetStayDurationBonus(reaction);
		guest.StayDuration += (int)stayBonus;

		// 6) Chance to order more
		bool wantsMore = GetWantsAnotherServing(reaction);

		return new ConsumptionResult
		{
			Reaction = reaction,
			LoyaltyChange = loyaltyDelta,
			GoldEarned = goldEarned,
			SatisfiedNeed = true,
			StayDurationBonus = stayBonus,
			WantsAnotherServing = wantsMore
		};
	}

	public static ConsumptionResult ConsumeDrink(Guest guest, DrinkItem drinkItem, bool fastService = false)
	{
		if (guest == null || guest.BoundNPC == null || drinkItem == null)
			return new ConsumptionResult { Reaction = ConsumptionReaction.Neutral };

		var npc = guest.BoundNPC;

		// 1) Taste evaluation
		var reaction = TasteResolver.Evaluate(
			guest,
			isDrink: true,
			rawFlavors: drinkItem.FlavorProfiles,
			ingredients: drinkItem.Ingredients
		);

		// 2) Loyalty update
		int loyaltyDelta = GetLoyaltyDelta(reaction);
		if (fastService && reaction != ConsumptionReaction.Disliked)
			loyaltyDelta += 1;

		npc.LoyaltyRating = Mathf.Clamp(npc.LoyaltyRating + loyaltyDelta, -100f, 100f);

		// 3) Calculate gold
		int goldEarned = CalculateGoldEarned(npc, drinkItem.BasePrice, reaction);
		if (goldEarned > 0 && TavernManager.Instance != null)
			TavernManager.Instance.AddGold(goldEarned);

		// 4) Resolve thirst
		npc.IsThirsty = false;
		npc.HasDrankToday = true;

		// Record type for repeat requests
		npc.LastConsumedWasFood = false;
		npc.LastConsumedWasDrink = true;

		// 5) Stay duration bonus
		float stayBonus = GetStayDurationBonus(reaction);
		guest.StayDuration += (int)stayBonus;

		// 6) Chance to order more
		bool wantsMore = GetWantsAnotherServing(reaction);

		return new ConsumptionResult
		{
			Reaction = reaction,
			LoyaltyChange = loyaltyDelta,
			GoldEarned = goldEarned,
			SatisfiedNeed = true,
			StayDurationBonus = stayBonus,
			WantsAnotherServing = wantsMore
		};
	}


	// =======================================================
	//  INTERNAL HELPERS
	// =======================================================

	private static int GetLoyaltyDelta(ConsumptionReaction reaction)
	{
		return reaction switch
		{
			ConsumptionReaction.Loved => 6,
			ConsumptionReaction.Liked => 3,
			ConsumptionReaction.Neutral => 2,
			ConsumptionReaction.Disliked => -6,
			_ => 0
		};
	}

	private static float GetStayDurationBonus(ConsumptionReaction reaction)
	{
		return reaction switch
		{
			ConsumptionReaction.Loved => 5f,
			ConsumptionReaction.Liked => 3f,
			ConsumptionReaction.Neutral => 1f,
			ConsumptionReaction.Disliked => 0f,
			_ => 0f
		};
	}

	private static bool GetWantsAnotherServing(ConsumptionReaction reaction)
	{
		float roll = GD.Randf();

		return reaction switch
		{
			ConsumptionReaction.Loved => roll < 0.20f, // 20%
			ConsumptionReaction.Liked => roll < 0.10f, // 10%
			_ => false
		};
	}

	private static int CalculateGoldEarned(NPCData npc, int basePrice, ConsumptionReaction reaction)
	{
		if (basePrice <= 0)
			return 0;

		// Taste multiplier
		float tasteMult = reaction switch
		{
			ConsumptionReaction.Loved => 1.20f,
			ConsumptionReaction.Liked => 1.10f,
			ConsumptionReaction.Neutral => 1.00f,
			ConsumptionReaction.Disliked => 0.80f,
			_ => 1.00f
		};

		// Loyalty multiplier  (-100 → +100) = up to ±25%
		float loyaltyMult = 1.0f + (npc.LoyaltyRating / 100.0f) * 0.25f;

		// Renown multiplier (0–100 → up to +10%)
		int renown = TavernStats.Instance != null ? TavernStats.Instance.Renown : 0;
		float renownMult = 1.0f + (renown / 100.0f) * 0.10f;

		float goldFloat = basePrice * tasteMult * loyaltyMult * renownMult;
		int gold = Mathf.RoundToInt(goldFloat);

		return Math.Max(gold, 1);
	}
}
