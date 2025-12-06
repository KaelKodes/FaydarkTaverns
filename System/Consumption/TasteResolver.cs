using Godot;
using System.Collections.Generic;
using System.Linq;
using FaydarkTaverns.Objects;

public static class TasteResolver
{
	/// <summary>
	/// Evaluates the guest's taste reaction to a food or drink.
	/// Uses NPCData.FavoriteFoodGroup / FavoriteDrinkGroup etc.
	/// </summary>
	/// <param name="guest">The Guest wrapper (must have BoundNPC).</param>
	/// <param name="isDrink">true for drink, false for food.</param>
	/// <param name="rawFlavors">Flavor strings from Dish/FoodItem/DrinkItem.</param>
	/// <param name="ingredients">Ingredient names (currently unused but kept for future depth).</param>
	public static ConsumptionReaction Evaluate(Guest guest, bool isDrink, List<string> rawFlavors, List<string> ingredients)
	{
		var npc = guest.BoundNPC;
		if (npc == null)
			return ConsumptionReaction.Neutral;

		// Expand "Smoky + Spicy" → ["Smoky", "Spicy"]
		List<string> tokens = ExpandFlavorTokens(rawFlavors);

		string favGroup = isDrink ? npc.FavoriteDrinkGroup : npc.FavoriteFoodGroup;
		string hateGroup = isDrink ? npc.HatedDrinkGroup : npc.HatedFoodGroup;

		bool hasFav = !string.IsNullOrEmpty(favGroup) && tokens.Contains(favGroup);
		bool hasHate = !string.IsNullOrEmpty(hateGroup) && tokens.Contains(hateGroup);

		// Exact favorite vs mixed flavors idea:
		// - Pure favorite (only one flavor and it's their fav) → Loved
		// - Favorite among others → Liked
		if (hasFav && !hasHate)
		{
			if (tokens.Count == 1)
				return ConsumptionReaction.Loved;
			else
				return ConsumptionReaction.Liked;
		}

		// Hated, without favorite present
		if (hasHate && !hasFav)
			return ConsumptionReaction.Disliked;

		// Both favorite and hated in the same profile = weird mix → call it Neutral for now
		if (hasFav && hasHate)
			return ConsumptionReaction.Neutral;

		// No explicit ties to their stated preferences → Neutral
		return ConsumptionReaction.Neutral;
	}

	/// <summary>
	/// Splits flavor strings like "Smoky + Spicy" into ["Smoky","Spicy"].
	/// Also splits on spaces so "Sweet + Fruity" → ["Sweet","Fruity"].
	/// </summary>
	private static List<string> ExpandFlavorTokens(List<string> input)
	{
		List<string> result = new List<string>();

		if (input == null)
			return result;

		foreach (var f in input)
		{
			if (string.IsNullOrWhiteSpace(f))
				continue;

			var chunks = f.Split(new char[] { '+', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

			foreach (var c in chunks)
				result.Add(c.Trim());
		}

		return result;
	}
}
