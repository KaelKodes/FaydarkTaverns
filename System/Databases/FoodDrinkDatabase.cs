using Godot;
using System;
using System.Text.Json;
using System.Collections.Generic;

public static class FoodDrinkDatabase
{
	public static List<FoodItem> AllFood = new();
	public static List<DrinkItem> AllDrinks = new();

	public static void LoadData()
	{
		var foodJson = FileAccess.Open("res://System/Databases/FoodMenuDB.json", FileAccess.ModeFlags.Read).GetAsText();
		var drinkJson = FileAccess.Open("res://System/Databases/DrinkMenuDB.json", FileAccess.ModeFlags.Read).GetAsText();

		AllFood = JsonSerializer.Deserialize<List<FoodItem>>(foodJson);
		AllDrinks = JsonSerializer.Deserialize<List<DrinkItem>>(drinkJson);
	}

	public static List<FoodItem> GetByFlavor(string flavor)
	{
		return AllFood.FindAll(item => item.flavor_profiles.Contains(flavor));
	}

	public static List<DrinkItem> GetByDrinkFlavor(string flavor)
	{
		return AllDrinks.FindAll(item => item.flavor_profiles.Contains(flavor));
	}
}

// Matching data structures
public class FoodItem
{
	public string id;
	public string name;
	public List<string> flavor_profiles;
	public string quality;
	public string category;
	public int base_price;
	public string description;
}

public class DrinkItem
{
	public string id;
	public string name;
	public List<string> flavor_profiles;
	public string quality;
	public string category;
	public int base_price;
	public string description;
}
