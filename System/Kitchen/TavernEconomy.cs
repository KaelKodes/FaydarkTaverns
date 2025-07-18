using Godot;
using System;

public partial class TavernEconomy : Node
{
	public static float GetGuestMaxPrice(float baseCost, float renownFactor = 0f, float loyaltyFactor = 0f, float flavorMatchBonus = 0f, float rarityFactor = 0f)
	{
		float multiplier = 1.25f + renownFactor + loyaltyFactor + flavorMatchBonus + rarityFactor;
		return baseCost * multiplier;
	}

	public static bool WillGuestBuy(float dishPrice, float baseCost, float renownFactor, float loyaltyFactor, float flavorMatchBonus, float rarityFactor)
	{
		float maxAcceptable = GetGuestMaxPrice(baseCost, renownFactor, loyaltyFactor, flavorMatchBonus, rarityFactor);
		return dishPrice <= maxAcceptable;
	}
}
