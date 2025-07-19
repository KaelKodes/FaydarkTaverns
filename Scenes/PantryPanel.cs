using Godot;
using System.Linq;

public partial class PantryPanel : Control
{
	[Export]
	public VBoxContainer ItemListContainer;         // Supplies / Complete Dishes container

	[Export]
	public VBoxContainer IngredientListContainer;   // Ingredients container

	public override void _Ready()
	{
		RefreshPantry();
	}

	public void RefreshPantry()
	{
		// Clear existing children in both containers
		foreach (var child in ItemListContainer.GetChildren())
			child.QueueFree();

		foreach (var child in IngredientListContainer.GetChildren())
			child.QueueFree();

		// Show all supplies (complete dishes) with quantity > 0
		foreach (var entry in PlayerPantry.Supplies)
		{
			if (entry.Value > 0)
			{
				var label = new Label();

				// Lookup name in Food/Drink DB
				var food = FoodDrinkDatabase.AllFood.FirstOrDefault(f => f.Id == entry.Key);
				var drink = FoodDrinkDatabase.AllDrinks.FirstOrDefault(d => d.Id == entry.Key);
				string displayName = food?.Name ?? drink?.Name ?? entry.Key;

				label.Text = $"{displayName} x{entry.Value}";
				ItemListContainer.AddChild(label);
			}
		}

		// Show all ingredients with quantity > 0
		foreach (var entry in PlayerPantry.Ingredients)
		{
			if (entry.Value > 0)
			{
				var label = new Label();

				// Lookup ingredient name in Ingredient DB
				var ingredient = IngredientDatabase.Ingredients.ContainsKey(entry.Key)
					? IngredientDatabase.Ingredients[entry.Key]
					: null;

				string displayName = ingredient?.Name ?? entry.Key;

				label.Text = $"{displayName} x{entry.Value}";
				IngredientListContainer.AddChild(label);
			}
		}
	}
}
