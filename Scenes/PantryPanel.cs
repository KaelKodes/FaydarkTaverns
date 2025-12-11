using Godot;
using System.Linq;

public partial class PantryPanel : Control
{
    // ===============================
    // SINGLETON INSTANCE
    // ===============================
    public static PantryPanel Instance;

    [Export] public VBoxContainer ItemListContainer;         // Supplies / Complete Dishes
    [Export] public VBoxContainer IngredientListContainer;   // Ingredients

    public override void _Ready()
    {
        // SAFE SINGLETON GUARD
        if (Instance != null && Instance != this && GodotObject.IsInstanceValid(Instance))
        {
            GD.PrintErr("❌ Duplicate PantryPanel detected! Replacing old instance.");
        }

        Instance = this;

        // Listen to supply changes AFTER singleton is valid
        PlayerPantry.SuppliesChanged += OnSuppliesChanged;

        // Deferred refresh prevents race condition during load
        CallDeferred(nameof(RefreshPantry));
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;

        PlayerPantry.SuppliesChanged -= OnSuppliesChanged;
    }

    private void OnSuppliesChanged()
    {
        // UI updates must ALWAYS be deferred during load
        CallDeferred(nameof(RefreshPantry));
    }

    // ===============================
    // SAFE REFRESH
    // ===============================
    public void RefreshPantry()
    {
        // Prevent crashes if UI is not yet ready (save/load timing)
        if (ItemListContainer == null || 
            !GodotObject.IsInstanceValid(ItemListContainer) ||
            IngredientListContainer == null ||
            !GodotObject.IsInstanceValid(IngredientListContainer))
        {
            GD.PrintErr("❌ PantryPanel.RefreshPantry aborted — containers not ready.");
            return;
        }

        // Clear existing items
        foreach (var child in ItemListContainer.GetChildren())
            child.QueueFree();

        foreach (var child in IngredientListContainer.GetChildren())
            child.QueueFree();

        // ===============================
        // SUPPLIES (FOOD/DRINKS)
        // ===============================
        foreach (var entry in PlayerPantry.Supplies)
        {
            if (entry.Value > 0)
            {
                var label = new Label();

                var food = FoodDrinkDatabase.AllFood.FirstOrDefault(f => f.Id == entry.Key);
                var drink = FoodDrinkDatabase.AllDrinks.FirstOrDefault(d => d.Id == entry.Key);
                string displayName = food?.Name ?? drink?.Name ?? entry.Key;

                label.Text = $"{displayName} x{entry.Value}";
                ItemListContainer.AddChild(label);
            }
        }

        // ===============================
        // INGREDIENTS
        // ===============================
        foreach (var entry in PlayerPantry.Ingredients)
        {
            if (entry.Value > 0)
            {
                var label = new Label();

                var ingredient = IngredientDatabase.Ingredients.ContainsKey(entry.Key)
                    ? IngredientDatabase.Ingredients[entry.Key]
                    : null;

                string displayName = ingredient?.Name ?? entry.Key;

                label.Text = $"{displayName} x{entry.Value}";
                IngredientListContainer.AddChild(label);
            }
        }

        GD.Print("[PantryPanel] Pantry refreshed.");
    }
}
