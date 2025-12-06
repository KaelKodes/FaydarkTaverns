using Godot;
using System.Collections.Generic;

public partial class FeedMenu : Control
{	
	public static FeedMenu Instance { get; private set; }
	[Export] public Panel BackgroundPanel;
	[Export] public GridContainer ItemsGrid;
	[Export] public PackedScene ItemButtonScene;
	[Export] public Button CloseButton;


	private Guest currentGuest;
	private Vector2 openPosition;

	private Texture2D placeholderIcon;
	private bool selectingDrink = false;


	public override void _Ready()
{
	Instance = this;
	Visible = false;

	placeholderIcon = GD.Load<Texture2D>("res://Assets/UI/DishUI/BoarfolkPotluck.png");

	if (CloseButton != null)
		CloseButton.Pressed += () => Visible = false;
}


	public void OpenAtMouse(Guest guest, bool drinkMode)
{
	currentGuest = guest;
	selectingDrink = drinkMode;

	openPosition = GetViewport().GetMousePosition();
	PopulateMenu();
	Position = ClampToViewport(openPosition);
	Visible = true;
}


	private Vector2 ClampToViewport(Vector2 pos)
{
	var vp = GetViewportRect().Size;
	var size = Size;

	float maxX = Mathf.Max(0, vp.X - size.X);
	float maxY = Mathf.Max(0, vp.Y - size.Y);

	return new Vector2(
		Mathf.Clamp(pos.X, 0, maxX),
		Mathf.Clamp(pos.Y, 0, maxY)
	);
}


	private void PopulateMenu()
{
	// Clear existing
	foreach (Node n in ItemsGrid.GetChildren())
		n.QueueFree();

	if (currentGuest == null)
		return;

	// --- SHOW FOOD ONLY ---
	if (!selectingDrink)
	{
		foreach (var kvp in PlayerPantry.Supplies)
		{
			string id = kvp.Key;
			int qty = kvp.Value;

			if (qty <= 0)
				continue;

			var food = FoodDrinkDatabase.AllFood.Find(f => f.Id == id);
			if (food != null)
			{
				var dish = DishDatabase.Dishes.Find(d => d.Name == food.Name);

				// Fallback for missing dishes
				var flavors = dish?.Flavors ?? food.FlavorProfiles;
				var ingredients = dish?.Ingredients ?? food.Ingredients;

				CreateButton(id, food.Name, flavors, ingredients, qty, isDrink: false);
			}
		}

		return; // stop here so drinks do NOT show
	}

	// --- SHOW DRINKS ONLY ---
	foreach (var kvp in PlayerPantry.Supplies)
	{
		string id = kvp.Key;
		int qty = kvp.Value;

		if (qty <= 0)
			continue;

		var drink = FoodDrinkDatabase.AllDrinks.Find(d => d.Id == id);
		if (drink != null)
		{
			CreateButton(
				id,
				drink.Name,
				drink.FlavorProfiles,
				drink.Ingredients,
				qty,
				isDrink: true
			);
		}
	}
}



	private void CreateButton(string itemId, string itemName, List<string> flavors, List<string> ingredients, int qty, bool isDrink)
{
	var btn = ItemButtonScene.Instantiate<FeedMenuItemButton>();
	ItemsGrid.AddChild(btn);

	btn.ItemId = itemId;
	btn.ItemName = itemName;
	btn.IsDrink = isDrink;

	btn.SetIcon(placeholderIcon);
	btn.SetQuantity(qty);
	btn.TooltipText = itemName;

	btn.Pressed += () => OnItemSelected(itemId, itemName, isDrink);
}


private void OnItemSelected(string itemId, string itemName, bool isDrink)
{
	if (currentGuest == null)
		return;

	if (isDrink)
	{
		// Look up by ID, not Name
		var drink = FoodDrinkDatabase.AllDrinks.Find(d => d.Id == itemId);

		if (drink == null)
		{
			GD.PrintErr($"[FeedMenu] ERROR: No DrinkItem found for Id '{itemId}' (Name='{itemName}')");
			return;
		}

		PlayerPantry.ConsumeSupplies(new List<string> { itemId });

		var result = ConsumptionManager.ConsumeDrink(currentGuest, drink);
		ApplyAftermath(currentGuest, result);
	}
	else
	{
		// Look up by ID, not Name
		var foodItem = FoodDrinkDatabase.AllFood.Find(f => f.Id == itemId);

		if (foodItem == null)
		{
			GD.PrintErr($"[FeedMenu] ERROR: No FoodItem found for Id '{itemId}' (Name='{itemName}')");
			return;
		}

		PlayerPantry.ConsumeSupplies(new List<string> { itemId });

		var result = ConsumptionManager.ConsumeFood(currentGuest, foodItem);
		ApplyAftermath(currentGuest, result);
	}

	Visible = false;
}



	private void ApplyAftermath(Guest guest, ConsumptionResult result)
	{
		guest.ShowRequestBubble(false);
		guest.ShowReaction(result.Reaction);

		if (result.WantsAnotherServing)
			guest.ScheduleAnotherOrder();
	}

	public void ConnectGuestCard(GuestCard card)
	{
		card.ServeFoodRequested += OnServeFoodRequestedCard;
		card.ServeDrinkRequested += OnServeDrinkRequestedCard;
	}

	private void OnServeFoodRequestedCard(GuestCard card)
{
	OpenAtMouse(card.BoundGuest, false); // false = food mode
}

private void OnServeDrinkRequestedCard(GuestCard card)
{
	OpenAtMouse(card.BoundGuest, true);  // true = drink mode
}

}
