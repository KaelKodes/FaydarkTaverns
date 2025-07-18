using Godot;
using System.Collections.Generic;

public partial class PlayerPantry : Node
{
	public static Dictionary<string, int> Ingredients = new();
	public static Dictionary<string, int> Supplies = new();

	// ========== INGREDIENTS ==========

	public static void AddIngredient(string ingredient, int amount = 1)
	{
		if (Ingredients.ContainsKey(ingredient))
			Ingredients[ingredient] += amount;
		else
			Ingredients[ingredient] = amount;

		GD.Print($"> Gained {amount}x {ingredient}");
		CookbookManager.RegisterIngredient(ingredient);
	}

	public static bool HasIngredients(List<string> needed)
	{
		var temp = new Dictionary<string, int>(Ingredients);
		foreach (var item in needed)
		{
			if (!temp.ContainsKey(item) || temp[item] <= 0)
				return false;
			temp[item]--;
		}
		return true;
	}

	public static void ConsumeIngredients(List<string> used)
	{
		foreach (var item in used)
		{
			if (Ingredients.ContainsKey(item))
			{
				Ingredients[item] = Mathf.Max(Ingredients[item] - 1, 0);
				GD.Print($"> Used 1x {item}");
			}
		}
	}

	// ========== SUPPLIES ==========

	public static void AddSupply(string supply, int amount = 1)
	{
		if (Supplies.ContainsKey(supply))
			Supplies[supply] += amount;
		else
			Supplies[supply] = amount;

		GD.Print($"> Gained {amount}x {supply}");
	}

	public static bool HasSupplies(List<string> needed)
	{
		var temp = new Dictionary<string, int>(Supplies);
		foreach (var item in needed)
		{
			if (!temp.ContainsKey(item) || temp[item] <= 0)
				return false;
			temp[item]--;
		}
		return true;
	}

	public static void ConsumeSupplies(List<string> used)
	{
		foreach (var item in used)
		{
			if (Supplies.ContainsKey(item))
			{
				Supplies[item] = Mathf.Max(Supplies[item] - 1, 0);
				GD.Print($"> Used 1x {item}");
			}
		}
	}

	// ========== DEBUG DISPLAY ==========

	public static void PrintInventory()
	{
		GD.Print("--- Pantry Inventory ---");
		foreach (var kvp in Ingredients)
			GD.Print($"[Ingredient] {kvp.Key}: {kvp.Value}");
		foreach (var kvp in Supplies)
			GD.Print($"[Supply]     {kvp.Key}: {kvp.Value}");
	}
}
