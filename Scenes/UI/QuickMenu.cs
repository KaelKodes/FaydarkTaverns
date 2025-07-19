using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FaydarkTaverns.Objects;

public partial class QuickMenu : Window
{
	[Export] VBoxContainer MainVBox;
	[Export] VBoxContainer ItemVBox;
	[Export] TextureButton ConfirmButton;
	[Export] TextureButton CancelButton;
	[Export] Label TitleLabel;

	private string selectedItemId = null;
	private NPCData currentNPC;
	private string currentRequestType; // e.g. "food" or "drink"
	private Button selectedButton = null;

	public override void _Ready()
{
	ConfirmButton.Pressed += OnConfirmPressed;
	CancelButton.Pressed += () => Hide();
	ConfirmButton.Disabled = true;  // Start disabled
}

	public void ShowMenu(string requestType, NPCData npc)
{
	currentRequestType = requestType.ToLower();
	currentNPC = npc;
	selectedItemId = null;

	// Reset button highlight if any
	if (selectedButton != null)
	{
		selectedButton.Modulate = new Color(1, 1, 1);
		selectedButton = null;
	}

	TitleLabel.Text = $"Select a {currentRequestType} to feed {npc.Name}";

	PopulateItems();

	Show();
	GrabFocus();
}


	private void PopulateItems()
{
	ItemVBox.ClearChildren();

	List<(string Id, string DisplayName)> items = new();

	if (currentRequestType == "food")
	{
		items = PlayerPantry.Supplies
			.Where(kvp => kvp.Value > 0)
			.Select(kvp =>
			{
				var food = FoodDrinkDatabase.AllFood.FirstOrDefault(f => f.Id == kvp.Key);
				var drink = FoodDrinkDatabase.AllDrinks.FirstOrDefault(d => d.Id == kvp.Key);
				string name = food?.Name ?? drink?.Name ?? kvp.Key;
				return (kvp.Key, name);
			})
			.ToList();
	}
	else if (currentRequestType == "drink")
	{
		items = PlayerPantry.Supplies
			.Where(kvp => kvp.Value > 0)
			.Select(kvp =>
			{
				var drink = FoodDrinkDatabase.AllDrinks.FirstOrDefault(d => d.Id == kvp.Key);
				var food = FoodDrinkDatabase.AllFood.FirstOrDefault(f => f.Id == kvp.Key);
				string name = drink?.Name ?? food?.Name ?? kvp.Key;
				return (kvp.Key, name);
			})
			.ToList();
	}
	else
	{
		GD.PrintErr($"Unknown request type: {currentRequestType}");
		return;
	}

	foreach (var item in items)
	{
		var btn = new Button
		{
			Text = item.DisplayName,
			SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
		};

		// Correct closure capture for event:
		string capturedId = item.Id;
		Button capturedBtn = btn;
		btn.Pressed += () => OnItemSelected(capturedId, capturedBtn);

		ItemVBox.AddChild(btn);
	}
}




private void OnItemSelected(string itemId, Button btn)
{
	selectedItemId = itemId;

	if (selectedButton != null && selectedButton != btn)
		selectedButton.Modulate = new Color(1, 1, 1);  // Reset old button color

	btn.Modulate = new Color(0.6f, 0.8f, 1);  // Highlight new button
	selectedButton = btn;
}



	private void OnConfirmPressed()
	{
		if (string.IsNullOrEmpty(selectedItemId))
		{
			GD.Print("Please select an item to feed.");
			return;
		}

		GD.Print($"Feeding NPC {currentNPC.Name} with {selectedItemId}.");

		// TODO: call feeding logic here, e.g.:
		// GameManager.Instance.FeedNPC(currentNPC, selectedItemId);

		Hide();
	}
}
