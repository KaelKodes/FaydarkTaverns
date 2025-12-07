using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ShopPanel : Window
{
	[Export] public VBoxContainer ItemListContainer;
	[Export] public VBoxContainer CartContainer;
	[Export] public Label TotalLabel;
	[Export] public TextureButton ConfirmButton;
	[Export] public TextureButton CloseButton;

	[Export] public TextureButton TabTables;
	[Export] public TextureButton TabDecorations;
	[Export] public TextureButton TabSupplies;
	[Export] public TextureButton TabUpgrade;
	[Export] public Texture2D activeTexture;
	[Export] public Texture2D inactiveTexture;
	public PantryPanel PantryPanel { get; set; }





	private ShopCategory activeCategory = ShopCategory.Tables;
	private TextureButton activeTab;

	private Dictionary<string, int> cart = new();

	public override void _Ready()
	{
		FoodDrinkDatabase.LoadData();
		ShopDatabase.RefreshSupplyItems();

		TabTables.Pressed += () => SwitchCategory(ShopCategory.Tables);
		TabDecorations.Pressed += () => SwitchCategory(ShopCategory.Decorations);
		TabSupplies.Pressed += () => SwitchCategory(ShopCategory.Supplies);
		TabUpgrade.Pressed += () => SwitchCategory(ShopCategory.Upgrades);


		ConfirmButton.Pressed += ConfirmPurchase;
		CloseButton.Pressed += Hide;
		this.CloseRequested += () => Hide();

		RefreshShop();
	}

	private void SwitchCategory(ShopCategory category)
	{
		activeCategory = category;
		RefreshShop();
	}
	private void SetActiveTab(TextureButton tab)
{
	// Reset previous tab to normal texture
	if (activeTab != null)
	{
		activeTab.TextureNormal = inactiveTexture;
		activeTab.TexturePressed = inactiveTexture;
	}

	// Set new active tab
	activeTab = tab;
	activeTab.TextureNormal = activeTexture;
	activeTab.TexturePressed = activeTexture;
}



	private void RefreshShop()
{
	ShopDatabase.RefreshSupplyItems();

	foreach (var child in ItemListContainer.GetChildren())
		child.QueueFree();

	foreach (var item in ShopDatabase.AllItems.Where(i => i.Category == activeCategory))
	{
		var label = new Label();
		label.Text = $"{item.Name} - {item.Cost}g (Lv {item.LevelRequirement})";

		bool isTable = item.Category == ShopCategory.Tables;

		// 🔢 Determine current owned and cap
		int current = TavernManager.Instance.GetPurchasedCount(item.Name);
		int cap = item.MaxOwned; // Use proper item-defined cap for tables


		bool isUnderCap = (cap == -1 || current < cap);
		bool levelUnlocked = TavernStats.Instance.Level >= item.LevelRequirement;

		if (levelUnlocked && isUnderCap)
		{
			label.MouseFilter = Control.MouseFilterEnum.Stop;

			label.MouseEntered += () => UIAudio.Instance.PlayHover();
			label.GuiInput += @event =>
			{
				if (@event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
				{
					UIAudio.Instance.PlayClick();
					AddToCart(item);
				}
			};
		}
		else
		{
			label.Modulate = new Color(0.5f, 0.5f, 0.5f);

			// 🛠 Optional tooltip explaining why item is locked
			if (!levelUnlocked)
				label.TooltipText = $"Requires Tavern Level {item.LevelRequirement}";
			else if (!isUnderCap)
				label.TooltipText = $"Limit reached ({current}/{cap})";
		}

		ItemListContainer.AddChild(label);
	}

	RefreshCartDisplay();
}





	private void AddToCart(ShopItem item)
{
	if (!cart.ContainsKey(item.Name))
		cart[item.Name] = 0;

	bool isTable = item.Category == ShopCategory.Tables;

	// 🧠 Determine cap and how many are already owned
	int currentOwned = TavernManager.Instance.GetPurchasedCount(item.Name);
	int cap = item.MaxOwned;


	int totalAfterAdd = cart[item.Name] + currentOwned;

	if (cap == -1 || totalAfterAdd < cap)
	{
		cart[item.Name]++;
		RefreshCartDisplay();
	}
	else
	{
		GameLog.Debug($"⛔ Cannot add {item.Name} — reached cap ({totalAfterAdd}/{cap})");
	}
}



	private void RefreshCartDisplay()
{
	// 🔄 Clear previous UI elements
	foreach (var child in CartContainer.GetChildren())
		child.QueueFree();

	int total = 0;

	foreach (var entry in cart)
	{
		var item = ShopDatabase.AllItems.FirstOrDefault(i => i.Name == entry.Key);
		if (item == null)
		{
			GD.PrintErr($"❌ Item not found in ShopDatabase: {entry.Key}");
			continue;
		}

		int qty = entry.Value;

		// 📦 HBox: [❌] [Label]
		var hbox = new HBoxContainer();

		var removeButton = new Button
		{
			Text = "❌",
			CustomMinimumSize = new Vector2(24, 24),
			Modulate = new Color(1, 0.3f, 0.3f),
			TooltipText = "Remove one"
		};

		// 🧠 Capture item name in local scope for lambda
		string itemName = item.Name;

		removeButton.Pressed += () =>
		{
			if (cart.TryGetValue(itemName, out int count))
			{
				if (count > 1)
					cart[itemName]--;
				else
					cart.Remove(itemName);
			}

			RefreshCartDisplay();
		};

		var label = new Label
		{
			Text = $"{item.Name} x{qty} ({item.Cost * qty}g)",
			VerticalAlignment = VerticalAlignment.Center
		};

		hbox.AddChild(removeButton);
		hbox.AddChild(label);
		CartContainer.AddChild(hbox);

		total += item.Cost * qty;
	}

	TotalLabel.Text = $"Total: {total}g";
}



	private void ConfirmPurchase()
{
	int total = cart.Sum(entry => ShopDatabase.AllItems.First(i => i.Name == entry.Key).Cost * entry.Value);

	if (TavernManager.Instance.SpendGold(total))
	{
		foreach (var entry in cart)
		{
			var item = ShopDatabase.AllItems.First(i => i.Name == entry.Key);
			for (int i = 0; i < entry.Value; i++)
			{
				if (item.Category == ShopCategory.Upgrades)
				{
					UpgradeManager.Instance.ApplyUpgrade(item);
				}
				else if (item.Category == ShopCategory.Supplies)
				{
					// Supplies: Add 10 of the correct food/drink to pantry
					var nameOnly = item.Name.Replace(" x10", "");
					var food = FoodDrinkDatabase.AllFood.FirstOrDefault(f => f.Name == nameOnly);
					if (food != null)
					{
						PlayerPantry.AddSupply(food.Id, 10);
						PantryPanel?.RefreshPantry();

					}
					else
					{
						var drink = FoodDrinkDatabase.AllDrinks.FirstOrDefault(d => d.Name == nameOnly);
						if (drink != null)
						{
							PlayerPantry.AddSupply(drink.Id, 10);
							PantryPanel?.RefreshPantry();
						}
					}
				}
				else
				{
					TavernManager.Instance.PurchaseItem(item);
				}

				item.PurchasedQuantity++;
			}
		}

		GameLog.Info($"🛒 Purchased items for {total}g");
		cart.Clear();
		RefreshCartDisplay();
		RefreshShop();
	}
	else
	{
		GameLog.Info("❌ Not enough gold!");
	}
}




}
