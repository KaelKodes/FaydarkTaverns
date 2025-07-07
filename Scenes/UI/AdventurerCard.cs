using Godot;
using System;

public partial class AdventurerCard : PanelContainer
{
	public Adventurer BoundAdventurer { get; set; }
	public Guest BoundGuest { get; set; }
	public QuestGiver BoundGiver { get; set; }

	private ColorRect background;

public override void _Ready()
{
	background = GetNodeOrNull<ColorRect>("Background");
	SetMouseFilter(MouseFilterEnum.Stop);

	if (BoundAdventurer != null)
	{
		SetDefaultColor();
	}
	else if (BoundGiver != null)
	{
		SetBackgroundColor(new Color(0.9f, 0.8f, 0.2f));
	}
	else
	{
		SetBackgroundColor(new Color(0.4f, 0.4f, 0.4f));
	}

	var removeButton = GetNode<Button>("KickButton");
	removeButton.Text = "❌";
	removeButton.FocusMode = Control.FocusModeEnum.None;
	removeButton.Pressed += OnRemovePressed;

	// Remove background
	var transparentStyle = new StyleBoxFlat();
	transparentStyle.BgColor = new Color(0, 0, 0, 0);
	removeButton.AddThemeStyleboxOverride("normal", transparentStyle);
	removeButton.AddThemeStyleboxOverride("hover", transparentStyle);
	removeButton.AddThemeStyleboxOverride("pressed", transparentStyle);

	// 👢 Hover behavior to change icon
	removeButton.MouseEntered += () => removeButton.Text = "🥾";
	removeButton.MouseExited += () => removeButton.Text = "❌";
}


private void OnRemovePressed()
{
	if (BoundGuest == null)
		return;

	// 🪑 Remove from table if seated
	if (BoundGuest.AssignedTable != null)
	{
		BoundGuest.AssignedTable.RemoveGuest(BoundGuest);
		BoundGuest.AssignedTable = null;
		BoundGuest.SeatIndex = null;
	}

	// 🚶 Remove from floor if standing
	if (TavernManager.Instance.GetGuestsInside().Contains(BoundGuest))
	{
		TavernManager.Instance.OnGuestRemoved(BoundGuest);
	}

	// 🎯 Send back to street queue
	GuestManager.QueueGuest(BoundGuest);
	GameLog.Info($"🚶 {BoundGuest.Name} left to rejoin the queue.");

	// 🧼 Cleanup UI
	TavernManager.Instance.DisplayAdventurers();
	TavernManager.Instance.UpdateFloorLabel();
	QueueFree();
}



	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent &&
			mouseEvent.ButtonIndex == MouseButton.Left &&
			mouseEvent.Pressed)
		{
			// This triggers _GetDragData automatically
			GetViewport().SetInputAsHandled();
		}
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (BoundAdventurer == null || BoundGuest == null)
		{
			GD.Print($"❌ Invalid drag: BoundAdventurer or BoundGuest is null.");
			return new Variant();
		}

		var preview = new Label
		{
			Text = BoundAdventurer.Name,
			Modulate = new Color(1, 1, 1, 0.9f),
			SizeFlagsHorizontal = SizeFlags.ExpandFill,
			HorizontalAlignment = HorizontalAlignment.Center
		};

		SetDragPreview(preview);
		return this; // Return the card itself
	}

	public void SetBackgroundColor(Color color)
	{
		if (background != null)
			background.Color = color;
	}

	public void SetDefaultColor()
	{
		SetBackgroundColor(new Color(0.3f, 0.4f, 0.6f));
	}
}
