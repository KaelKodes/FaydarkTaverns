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

		// Required for drag to register
		SetMouseFilter(MouseFilterEnum.Stop);

		// Optional: background tinting logic
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
