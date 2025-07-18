using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerInventory : Control
{
	[Export] public PackedScene ItemSlotScene;
	private GridContainer _grid;

	public override void _Ready()
	{
		_grid = GetNode<GridContainer>("VBoxContainer/ScrollContainer/ItemGrid");
		RefreshInventory();
	}

	public void RefreshInventory()
	{
		// Clear previous entries
		foreach (Node child in _grid.GetChildren())
			child.QueueFree();

		// Show Ingredients
		foreach (var pair in PlayerPantry.Ingredients)
			AddItemSlot(pair.Key, pair.Value, "Ingredient");

		// Show Supplies
		foreach (var pair in PlayerPantry.Supplies)
			AddItemSlot(pair.Key, pair.Value, "Supply");
	}

	private void AddItemSlot(string itemName, int count, string category)
	{
		var slot = ItemSlotScene.Instantiate<Control>();

		// Assumes you added these nodes to your ItemSlot scene
		slot.GetNode<Label>("ItemName").Text = itemName;
		slot.GetNode<Label>("StackLabel").Text = $"x{count}";

		// Optional: category tag, tooltip, color
		slot.TooltipText = $"{category}: {itemName}";

		_grid.AddChild(slot);
	}
}
