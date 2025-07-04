using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ShopPanel : Window
{
	[Export] public VBoxContainer ItemListContainer;
	[Export] public VBoxContainer CartContainer;
	[Export] public Label TotalLabel;
	[Export] public Button ConfirmButton;
	[Export] public Button CloseButton;

	[Export] public Button TabTables;
	[Export] public Button TabDecorations;
	[Export] public Button TabSupplies;

	private ShopCategory activeCategory = ShopCategory.Tables;
	private Dictionary<string, int> cart = new();

	public override void _Ready()
	{
		TabTables.Pressed += () => SwitchCategory(ShopCategory.Tables);
		TabDecorations.Pressed += () => SwitchCategory(ShopCategory.Decorations);
		TabSupplies.Pressed += () => SwitchCategory(ShopCategory.Supplies);

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

	private void RefreshShop()
{
	foreach (var child in ItemListContainer.GetChildren())
		child.QueueFree();

	foreach (var item in ShopDatabase.AllItems.Where(i => i.Category == activeCategory))
	{
		var label = new Label();
		label.Text = $"{item.Name} - {item.Cost}g (Lv {item.LevelRequirement})";

		if (TavernManager.TavernLevel >= item.LevelRequirement && item.PurchasedQuantity < item.MaxOwned)
		{
			label.MouseFilter = Control.MouseFilterEnum.Stop;
			label.GuiInput += @event =>
			{
				if (@event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
					AddToCart(item);
			};
		}
		else
		{
			label.Modulate = new Color(0.5f, 0.5f, 0.5f);
		}

		ItemListContainer.AddChild(label);
	}

	RefreshCartDisplay();
}


	private void AddToCart(ShopItem item)
	{
		if (!cart.ContainsKey(item.Name))
			cart[item.Name] = 0;

		if (cart[item.Name] + item.PurchasedQuantity < item.MaxOwned)
		{
			cart[item.Name]++;
			RefreshCartDisplay();
		}
	}

	private void RefreshCartDisplay()
{
	// Clear old UI
	foreach (var child in CartContainer.GetChildren())
		child.QueueFree();

	int total = 0;

	foreach (var entry in cart)
	{
		var item = ShopDatabase.AllItems.First(i => i.Name == entry.Key);
		int qty = entry.Value;

		// HBox: [❌] [Label]
		var hbox = new HBoxContainer();

		var removeButton = new Button
		{
			Text = "❌",
			CustomMinimumSize = new Vector2(24, 24),
			Modulate = new Color(1, 0.3f, 0.3f),
			TooltipText = "Remove one"
		};

		// Capture name to use in lambda
		string itemName = item.Name;

		removeButton.Pressed += () =>
		{
			if (cart[itemName] > 1)
				cart[itemName]--;
			else
				cart.Remove(itemName);

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
				TavernManager.Instance.PurchaseItem(item);
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
