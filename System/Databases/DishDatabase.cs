using Godot;
using System.Collections.Generic;
using System.Linq;

public static class DishDatabase
{
	public static List<Dish> Dishes = new();

	public static void LoadFromFoodDB()
	{
		Dishes.Clear();
		foreach (var food in FoodDrinkDatabase.AllFood)
		{
			Dishes.Add(new Dish(food.Name, food.Ingredients, food.FlavorProfiles));
		}
	}

	public static Dish? GetByName(string name)
	{
		return Dishes.FirstOrDefault(d => d.Name == name);
	}
}
