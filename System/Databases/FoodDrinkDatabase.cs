using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

public static class FoodDrinkDatabase
{
	public static List<FoodItem> AllFood = new();
	public static List<DrinkItem> AllDrinks = new();

	public static void LoadData()
	{
		string foodPath = "res://System/Databases/FoodMenuDB.json";
		string drinkPath = "res://System/Databases/DrinkMenuDB.json";

		GameLog.Debug($"[FDB] Loading food from: {foodPath}");
		GameLog.Debug($"[FDB] Loading drinks from: {drinkPath}");

		string foodJson = "";
		string drinkJson = "";

		try
		{
			foodJson = FileAccess.Open(foodPath, FileAccess.ModeFlags.Read).GetAsText();
			drinkJson = FileAccess.Open(drinkPath, FileAccess.ModeFlags.Read).GetAsText();
		}
		catch (Exception e)
		{
			GD.PrintErr($"[FDB] ERROR opening JSON files: {e.Message}");
		}

		try
		{
			AllFood = JsonSerializer.Deserialize<List<FoodItem>>(foodJson);
			AllDrinks = JsonSerializer.Deserialize<List<DrinkItem>>(drinkJson);
		}
		catch (Exception e)
		{
			GD.PrintErr($"[FDB] ERROR deserializing JSON: {e.Message}");
		}

		// Debug outputs
		GameLog.Debug($"[FDB] Loaded AllFood count: {(AllFood == null ? "NULL" : AllFood.Count.ToString())}");
		if (AllFood != null)
			GameLog.Debug($"[FDB] AllFood names: {string.Join(", ", AllFood.Select(f => f.Name))}");
		if (AllFood != null)
			GameLog.Debug($"[FDB] Purchasable Foods: {string.Join(", ", AllFood.Where(f => f.Purchasable).Select(f => f.Name))}");

		GameLog.Debug($"[FDB] Loaded AllDrinks count: {(AllDrinks == null ? "NULL" : AllDrinks.Count.ToString())}");
		if (AllDrinks != null)
			GameLog.Debug($"[FDB] AllDrinks names: {string.Join(", ", AllDrinks.Select(d => d.Name))}");
		if (AllDrinks != null)
			GameLog.Debug($"[FDB] Purchasable Drinks: {string.Join(", ", AllDrinks.Where(d => d.Purchasable).Select(d => d.Name))}");
	}

	public static List<FoodItem> GetByFlavor(string flavor)
	{
		return AllFood.FindAll(item => item.FlavorProfiles.Contains(flavor));
	}

	public static List<DrinkItem> GetByDrinkFlavor(string flavor)
	{
		return AllDrinks.FindAll(item => item.FlavorProfiles.Contains(flavor));
	}
}

// Matching data structures
public class FoodItem
{
	[JsonPropertyName("id")]
	public string Id { get; set; }
	[JsonPropertyName("name")]
	public string Name { get; set; }
	[JsonPropertyName("flavor_profiles")]
	public List<string> FlavorProfiles { get; set; }
	[JsonPropertyName("ingredients")]
	public List<string> Ingredients { get; set; }
	[JsonPropertyName("quality")]
	public string Quality { get; set; }
	[JsonPropertyName("category")]
	public string Category { get; set; }
	[JsonPropertyName("base_price")]
	public int BasePrice { get; set; }
	[JsonPropertyName("description")]
	public string Description { get; set; }
	[JsonPropertyName("learned")]
	public bool Learned { get; set; }
	[JsonPropertyName("purchasable")]
	public bool Purchasable { get; set; }
}

public class DrinkItem
{
	[JsonPropertyName("id")]
	public string Id { get; set; }

	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("flavor_profiles")]
	public List<string> FlavorProfiles { get; set; }

	[JsonPropertyName("ingredients")]
	public List<string> Ingredients { get; set; }

	[JsonPropertyName("quality")]
	public string Quality { get; set; }

	[JsonPropertyName("category")]
	public string Category { get; set; }

	[JsonPropertyName("base_price")]
	public int BasePrice { get; set; }

	[JsonPropertyName("description")]
	public string Description { get; set; }

	[JsonPropertyName("learned")]
	public bool Learned { get; set; }

	[JsonPropertyName("purchasable")]
	public bool Purchasable { get; set; }
}
