using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public enum ShopCategory
{
	Tables,
	Decorations,
	Supplies,
	Upgrades
}

public class ShopItem
{
	public string Name;
	public int Cost;
	public int LevelRequirement;
	public int MaxOwned; // -1 = unlimited
	public ShopCategory Category;
	public string Description;
	public int PurchasedQuantity = 0;
	public int RenownValue;

	public ShopItem(string name, int cost, int levelReq, int maxOwned, ShopCategory category, string description)
{
	Name = name;
	Cost = cost;
	LevelRequirement = levelReq;
	MaxOwned = maxOwned;
	Category = category;
	Description = description;

	// 💡 Renown Scaling
	RenownValue = levelReq;

	if (MaxOwned == 1)
		RenownValue += 2;
	else if (MaxOwned > 0 && MaxOwned <= 8)
		RenownValue += 1;
}
}
public static class ShopDatabase
{
	public static List<ShopItem> AllItems = new()
	{
		// 🪑 Tables
		// 🪑 Tables
new ShopItem("Starting Table", 0, 1, 1, ShopCategory.Tables, "Free 2-seat starter table"),
new ShopItem("Tiny Table", 50, 2, 2, ShopCategory.Tables, "2-seat cozy table for duos"),
new ShopItem("Small Table", 100, 4, 2, ShopCategory.Tables, "4-seat table for small parties"),
new ShopItem("Medium Table", 250, 6, 2, ShopCategory.Tables, "6-seat table for larger groups"),
new ShopItem("Large Table", 500, 8, 2, ShopCategory.Tables, "8-seat raid-ready table"),


		// 🎨 Decorations
		new ShopItem("Wall Banner", 25, 1, 8, ShopCategory.Decorations, "Decorative banner"),
		new ShopItem("Fancy Rug", 50, 2, 1, ShopCategory.Decorations, "Stylish rug"),
		new ShopItem("Mounted Trophy", 100, 4, 4, ShopCategory.Decorations, "Display your beast-slaying pride"),

		// 🍞 Supplies

		
		// 🔧 Upgrades
new ShopItem("Increase Floor Cap", 250, 2, -1, ShopCategory.Upgrades, "Adds +1 to max guests on the tavern floor."),
new ShopItem("Increase Quest Cap", 300, 3, -1, ShopCategory.Upgrades, "Allows more quests to be posted at once."),
new ShopItem("Upgrade Tavern Sign", 750, 6, 1, ShopCategory.Upgrades, "Boosts renown and visibility."),
new ShopItem("Unlock Oven", 500, 2, 1, ShopCategory.Upgrades, "Unlocks the ability to sell food."),
new ShopItem("Unlock Keg Stand", 500, 4, 1, ShopCategory.Upgrades, "Unlocks the ability to sell drinks."),
new ShopItem("Unlock Lodging", 1000, 10, 1, ShopCategory.Upgrades, "Prepares the tavern to house guests overnight."),

	};

public static void RefreshSupplyItems()
{
	AllItems.RemoveAll(i => i.Category == ShopCategory.Supplies);

	foreach (var food in FoodDrinkDatabase.AllFood)
	{
		if (food.Purchasable)
		{
			AllItems.Add(new ShopItem(
				$"{food.Name} x10",
				food.BasePrice * 10,
				1,
				-1,
				ShopCategory.Supplies,
				$"Bundle of {food.Name.ToLower()}"
			));
		}
	}

	foreach (var drink in FoodDrinkDatabase.AllDrinks)
	{
		if (drink.Purchasable)
		{
			AllItems.Add(new ShopItem(
				$"{drink.Name} x10",
				drink.BasePrice * 10,
				1,
				-1,
				ShopCategory.Supplies,
				$"Bundle of {drink.Name.ToLower()}"
			));
		}
	}
}



}
