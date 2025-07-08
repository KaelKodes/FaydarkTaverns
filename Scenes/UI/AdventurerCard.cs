using Godot;
using System;

public partial class AdventurerCard : PanelContainer
{
	public Adventurer BoundAdventurer { get; set; }
	public Guest BoundGuest { get; set; }
	public QuestGiver BoundGiver { get; set; }
	private static AdventurerCard _currentlyDraggedCard = null;
	public static bool IsDraggingAdventurer() => _currentlyDraggedCard != null;
	public static AdventurerCard GetCurrentlyDraggedCard() => _currentlyDraggedCard;



	private ColorRect background;

	public override void _Ready()
{
	background = GetNodeOrNull<ColorRect>("Background");
	SetMouseFilter(MouseFilterEnum.Stop);

	// Set background color based on type
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

	// Configure remove button
	var removeButton = GetNode<Button>("KickButton");
	removeButton.Text = "❌";
	removeButton.FocusMode = Control.FocusModeEnum.None;
	removeButton.Pressed += OnRemovePressed;

	// Make button background transparent
	var transparentStyle = new StyleBoxFlat();
	transparentStyle.BgColor = new Color(0, 0, 0, 0);
	removeButton.AddThemeStyleboxOverride("normal", transparentStyle);
	removeButton.AddThemeStyleboxOverride("hover", transparentStyle);
	removeButton.AddThemeStyleboxOverride("pressed", transparentStyle);

	// Hover icon swap
	removeButton.MouseEntered += () => removeButton.Text = "🥾";
	removeButton.MouseExited += () => removeButton.Text = "❌";

	// ─── Portrait loading ──────────────────────────────────────────
var portrait = GetNodeOrNull<TextureRect>("MarginContainer/Portrait");
if (portrait != null)
{
	portrait.Position += new Vector2(25, 0);

	Gender gender;
	string className;
	int portraitId;

	if (BoundAdventurer != null)
	{
		gender = BoundAdventurer.Gender;
		className = BoundAdventurer.ClassName;
		portraitId = BoundAdventurer.PortraitId;
	}
	else if (BoundGuest != null)
	{
		gender = BoundGuest.Gender;
		className = BoundGuest.IsAdventurer && BoundGuest.BoundAdventurer != null
			? BoundGuest.BoundAdventurer.ClassName
			: "Informant";
		portraitId = BoundGuest.PortraitId;
	}
	else
	{
		gender = Gender.Male;
		className = "Informant";
		portraitId = 1;
	}

	string initial = gender == Gender.Male ? "M" : "F";
	string assetPath = $"res://assets/ui/ClassPortraits/{className}/{className}{initial}{portraitId}.jpg";
	var tex2D = ResourceLoader.Load<Texture2D>(assetPath);
	portrait.Texture = tex2D;
}


	// ─── UI SOUND AND CURSOR HOOKS (add after all UI is initialized) ──────────

	this.MouseEntered += () =>
	{
		if (!AdventurerCard.IsDraggingAdventurer())
		{
			UIAudio.Instance.PlayHover();
			UICursor.Instance.SetPoint();
		}
	};
	this.MouseExited += () =>
	{
		if (!AdventurerCard.IsDraggingAdventurer())
			UICursor.Instance.SetIdle();
	};

	this.FocusEntered += () => UIAudio.Instance.PlayHover();

	this.GuiInput += @event =>
	{
		if (@event is InputEventMouseButton mouseEvent &&
			mouseEvent.ButtonIndex == MouseButton.Left &&
			mouseEvent.Pressed)
		{
			UIAudio.Instance.PlayClick();
		}
	};

	// ───────────────────────────────────────────────────────────────
}




	private void OnRemovePressed()
{
	if (BoundGuest == null)
		return;

	// Remove from table if seated
	if (BoundGuest.AssignedTable != null)
	{
		BoundGuest.AssignedTable.RemoveGuest(BoundGuest);
		BoundGuest.AssignedTable = null;
		BoundGuest.SeatIndex = null;
	}

	// Remove from floor if standing
	if (TavernManager.Instance.GetGuestsInside().Contains(BoundGuest))
	{
		TavernManager.Instance.OnGuestRemoved(BoundGuest);
	}

	// Send back to street queue
	GuestManager.QueueGuest(BoundGuest);
	GameLog.Info($"🚶 {BoundGuest.Name} left to rejoin the queue.");

	// ─── Cursor logic: Ensure cursor resets to idle on removal ───
	UICursor.Instance.SetIdle();
	// ────────────────────────────────────────────────────────────

	// Cleanup UI
	TavernManager.Instance.DisplayAdventurers();
	TavernManager.Instance.UpdateFloorLabel();
	QueueFree();
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
	
	
		public override void _GuiInput(InputEvent @event)
{
	if (@event is InputEventMouseButton mouseEvent)
	{
		if (!mouseEvent.Pressed)
		{
			// Mouse button released (end/cancel drag)
			_currentlyDraggedCard = null;

			// ─── Cursor logic: Always return to idle on release ───
			UICursor.Instance.SetIdle();
			// ─────────────────────────────────────────────────────
		}
		else if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
		{
			GetViewport().SetInputAsHandled();
		}
	}
}


	
	public override Variant _GetDragData(Vector2 atPosition)
{
	if (BoundAdventurer == null || BoundGuest == null)
	{
		GD.Print($"❌ Invalid drag: BoundAdventurer or BoundGuest is null.");
		return new Variant();
	}

	// Mark this card as being dragged
	_currentlyDraggedCard = this;

	// ─── Cursor logic: Switch to grab ───
	UICursor.Instance.SetGrab();
	// ───────────────────────────────────

	var preview = new Label
	{
		Text = BoundAdventurer.Name,
		Modulate = new Color(1, 1, 1, 0.9f),
		SizeFlagsHorizontal = SizeFlags.ExpandFill,
		HorizontalAlignment = HorizontalAlignment.Center
	};

	SetDragPreview(preview);
	return this;
}

// Clear reference on drag end (add this override)
public override void _DropData(Vector2 atPosition, Variant data)
{
	_currentlyDraggedCard = null;

	// ─── Cursor logic: Always return to idle after drop ───
	UICursor.Instance.SetIdle();
	// ─────────────────────────────────────────────────────

	base._DropData(atPosition, data);
}




}
