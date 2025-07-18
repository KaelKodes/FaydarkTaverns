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
		foreach (var ingredientName in Ingredients)
		{
			if (IngredientDatabase.Ingredients.TryGetValue(ingredientName, out var ing))
				total += ing.Value;
		}
		return total;
	}
}
