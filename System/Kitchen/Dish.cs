using Godot;
using System;
using System.Collections.Generic;

public partial class Dish
{
	public string Name;
	public List<string> Ingredients;
	public List<string> Flavors;

	public Dish(string name, List<string> ingredients, List<string> flavors)
	{
		Name = name;
		Ingredients = ingredients;
		Flavors = flavors;
	}

	public int CalculateBaseCost()
	{
		int total = 0;
		if (Ingredients == null)
			return total;

		foreach (var ingredientName in Ingredients)
		{
			if (IngredientDatabase.Ingredients.TryGetValue(ingredientName, out var ing))
				total += (int)ing.Value;  // Or use Mathf.RoundToInt(ing.Value) if preferred
		}
		return total;
	}
}
