using System.Collections.Generic;

public enum ShopCategory
{
	Tables,
	Decorations,
	Supplies
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
		new ShopItem("Starting Table", 0, 1, 1, ShopCategory.Tables, "Free 4-seat starter table"),
		new ShopItem("Tiny Table", 50, 2, -1, ShopCategory.Tables, "2-seat cozy table for duos"),
		new ShopItem("Small Table", 100, 4, -1, ShopCategory.Tables, "4-seat table for small parties"),
		new ShopItem("Medium Table", 250, 6, -1, ShopCategory.Tables, "6-seat table for larger groups"),
		new ShopItem("Large Table", 500, 8, -1, ShopCategory.Tables, "8-seat raid-ready table"),

		// 🎨 Decorations
		new ShopItem("Wall Banner", 25, 1, 8, ShopCategory.Decorations, "Decorative banner"),
		new ShopItem("Fancy Rug", 50, 2, 1, ShopCategory.Decorations, "Stylish rug"),
		new ShopItem("Mounted Trophy", 100, 4, 4, ShopCategory.Decorations, "Display your beast-slaying pride"),
		new ShopItem("Upgrade Tavern Sign", 500, 6, 1, ShopCategory.Decorations, "Draws elite guests"),

		// 🍞 Supplies (x10 bundles)
		new ShopItem("Bread Loaf x10", 20, 1, -1, ShopCategory.Supplies, "Bundle of bread"),
		new ShopItem("Mug of Ale x10", 30, 1, -1, ShopCategory.Supplies, "Bundle of ale"),
		new ShopItem("Hearty Stew x10", 60, 5, -1, ShopCategory.Supplies, "Extends guest stay"),
		new ShopItem("Rare Wine x10", 120, 7, -1, ShopCategory.Supplies, "Treat for VIP guests")
	};
}
