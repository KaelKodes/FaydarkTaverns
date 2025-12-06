using Godot;
using System;
using System.Collections.Generic;
using FaydarkTaverns.Objects;




public static class GuestDishEvaluator
{
	public static bool WillBuy(NPCData guest, Dish dish, float askingPrice)
	{
		int baseCost = dish.CalculateBaseCost();

		// Check flavor match
		float flavorBonus = dish.Flavors.Contains(guest.FavoriteFoodGroup) ? 0.15f : 0f;
		float penalty = dish.Flavors.Contains(guest.HatedFoodGroup) ? -0.20f : 0f;

		float maxAcceptable = TavernEconomy.GetGuestMaxPrice(
			baseCost,
			renownFactor: 0f,
			loyaltyFactor: (guest.LoyaltyRating / 100f) * 0.25f,
			flavorMatchBonus: flavorBonus,
			rarityFactor: 0f
		) + (penalty * baseCost);

		return askingPrice <= maxAcceptable;
	}
}
