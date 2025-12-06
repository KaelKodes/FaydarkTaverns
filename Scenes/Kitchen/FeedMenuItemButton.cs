using Godot;

public partial class FeedMenuItemButton : Button
{
	[Export] public TextureRect Icon;
	[Export] public Label QuantityBadge;

	// Assigned by FeedMenu during setup
	public string ItemName;
	public bool IsDrink;
	public string ItemId;

	// No need to override _Ready just for Pressed;
	// FeedMenu connects to btn.Pressed directly.

	public void SetQuantity(int qty)
	{
		QuantityBadge.Text = $"x{qty}";
	}

	public void SetIcon(Texture2D tex)
	{
		Icon.Texture = tex;
		Icon.CustomMinimumSize = new Vector2(48, 48);
	}
}
