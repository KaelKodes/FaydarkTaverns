
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class CookbookManager : Node
{
	public static HashSet<string> KnownIngredients = new();
	public static HashSet<string> KnownRecipes = new();

	public static void RegisterIngredient(string ingredient)
	{
		if (KnownIngredients.Add(ingredient))
			GD.Print($"> New Ingredient: {ingredient}");

		foreach (var recipe in DishDatabase.Dishes)
		{
			if (KnownRecipes.Contains(recipe.Name))
				continue;

			int matchCount = recipe.Ingredients.Count(i => KnownIngredients.Contains(i));
			if (matchCount >= recipe.Ingredients.Count)
			{
				RevealRecipe(recipe.Name);
			}
		}
	}

	public static void RevealRecipe(string name)
	{
		if (KnownRecipes.Add(name))
			GD.Print($"> New Recipe Learned: {name}");
	}

	public static List<Dish> GetUnknownRecipesFrom(string ingredient)
	{
		return DishDatabase.Dishes
			.Where(d => d.Ingredients.Contains(ingredient) && !KnownRecipes.Contains(d.Name))
			.ToList();
	}

	public static bool CanAttemptRecipe(List<string> ingredients)
	{
		return DishDatabase.Dishes.Any(d =>
			d.Ingredients.Count == ingredients.Count &&
			!KnownRecipes.Contains(d.Name) &&
			!d.Ingredients.Except(ingredients).Any());
	}
	public static bool TryExperiment(List<string> attemptedIngredients, out Dish discoveredDish)
	{
		discoveredDish = null;

		foreach (var dish in DishDatabase.Dishes)
		{
			if (dish.Ingredients.Count != attemptedIngredients.Count)
				continue;

			bool allMatch = dish.Ingredients.All(ing => attemptedIngredients.Contains(ing));
			if (allMatch)
			{
				if (GD.Randf() <= 0.10f)
				{
					RevealRecipe(dish.Name);
					discoveredDish = dish;
					return true; // success
				}
				else
				{
					discoveredDish = dish;
					return false; // failure: lost ingredients
				}
			}
		}

		return false; // no recipe match
	}

}
