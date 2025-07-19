using Godot;
using System;
using FaydarkTaverns.Objects;

public partial class GuestCard : Panel
{
	[Export] public PackedScene NPCDetailPopupScene;

	public Guest BoundGuest { get; set; }
	public NPCData BoundNPC { get; set; }

	private TextureButton HungryBubble;
	private TextureButton ThirstyBubble;
	public event Action<GuestCard> ServeFoodRequested;
	public event Action<GuestCard> ServeDrinkRequested;

	private ColorRect background;


	// Mouse 
	private static GuestCard _currentlyDraggedCard = null;
	private NPCDetailPopup _currentPopup;

	public static bool IsDraggingAdventurer() => _currentlyDraggedCard != null;
	public static GuestCard GetCurrentlyDraggedCard() => _currentlyDraggedCard;
	private Vector2 _clickStartPos;
	private bool    _didDrag;
	private const float DragThreshold = 10f;


	

	public override void _Ready()
{
	background = GetNodeOrNull<ColorRect>("Background");
	SetMouseFilter(MouseFilterEnum.Stop);

	// Role-based background color
	if (BoundNPC != null)
	{
		if (BoundNPC.Role == NPCRole.Adventurer)
			SetDefaultColor();
		else if (BoundNPC.Role == NPCRole.QuestGiver)
			SetBackgroundColor(new Color(0.5f, 0.5f, 0.5f));
		else
			SetBackgroundColor(new Color(0.4f, 0.4f, 0.4f));
	}

	SetCardLabels();

	// Setup Kick Button
	var removeButton = GetNode<Button>("KickButton");
	removeButton.Text = "❌";
	removeButton.FocusMode = Control.FocusModeEnum.None;
	removeButton.Pressed += OnRemovePressed;

	var transparentStyle = new StyleBoxFlat { BgColor = new Color(0, 0, 0, 0) };
	removeButton.AddThemeStyleboxOverride("normal", transparentStyle);
	removeButton.AddThemeStyleboxOverride("hover", transparentStyle);
	removeButton.AddThemeStyleboxOverride("pressed", transparentStyle);

	removeButton.MouseEntered += () => removeButton.Text = "🥾";
	removeButton.MouseExited += () => removeButton.Text = "❌";

	// ✅ Setup Hunger/Thirst Bubbles (safe + null-checked)
	HungryBubble = GetNodeOrNull<TextureButton>("BubbleControl/HungryBubble");
	ThirstyBubble = GetNodeOrNull<TextureButton>("BubbleControl/ThirstyBubble");

	if (HungryBubble != null)
	{
		HungryBubble.Pressed += OnHungryBubblePressed;
		HungryBubble.Visible = false;
	}
	else
	{
		GD.PrintErr("❌ HungryBubble not found in GuestCard!");
	}

	if (ThirstyBubble != null)
	{
		ThirstyBubble.Pressed += OnThirstyBubblePressed;
		ThirstyBubble.Visible = false;
	}
	else
	{
		GD.PrintErr("❌ ThirstyBubble not found in GuestCard!");
	}

	// Portrait logic
	var portrait = GetNodeOrNull<TextureRect>("MarginContainer/Portrait");
	if (portrait != null)
	{
		portrait.Position += new Vector2(25, 0);

		Gender gender = BoundGuest?.Gender ?? Gender.Male;
		string className = BoundNPC?.ClassName ?? "Informant";
		int portraitId = BoundNPC?.PortraitId ?? 1;

		string initial = gender == Gender.Male ? "M" : "F";
		string assetPath = $"res://Assets/UI/ClassPortraits/{className}/{className}{initial}{portraitId}.jpg";
		portrait.Texture = ResourceLoader.Load<Texture2D>(assetPath);
	}

	// Mouse/hover/click feedback
	MouseEntered += () =>
	{
		if (!IsDraggingAdventurer())
		{
			UIAudio.Instance.PlayHover();
			UICursor.Instance.SetPoint();
		}
	};

	MouseExited += () =>
	{
		if (!IsDraggingAdventurer())
			UICursor.Instance.SetIdle();
	};

	FocusEntered += () => UIAudio.Instance.PlayHover();

	GuiInput += OnGuiInput;

}


	private void OnRemovePressed()
	{
		if (BoundGuest == null)
			return;

		if (BoundGuest.AssignedTable != null)
		{
			BoundGuest.AssignedTable.RemoveGuest(BoundGuest);
			BoundGuest.AssignedTable = null;
			BoundGuest.SeatIndex = null;
		}

		if (TavernManager.Instance.GetGuestsInside().Contains(BoundGuest))
		{
			TavernManager.Instance.OnGuestRemoved(BoundGuest);
		}

		GuestManager.QueueGuest(BoundGuest);
		GameLog.Info($"🚶 {BoundGuest.Name} left to rejoin the queue.");

		UICursor.Instance.SetIdle();
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
				_currentlyDraggedCard = null;
				UICursor.Instance.SetIdle();
			}
			else if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				GetViewport().SetInputAsHandled();
			}
		}
	}

	public void SetEmptySlot()
	{
		GetNode<Control>("VBoxContainer").Visible = false;

		var background = GetNode<ColorRect>("Background");
		background.Color = new Color(0f, 0f, 0f, 0.3f);

		var portrait = GetNode<TextureRect>("MarginContainer/Portrait");
		portrait.Texture = null;
		portrait.Visible = false;

		GetNode<Button>("KickButton").Visible = false;
	}

	public override Variant _GetDragData(Vector2 atPosition)
{
	// Only allow dragging if this card is an Adventurer
	if (BoundNPC == null || BoundNPC.Role != NPCRole.Adventurer)
		return new Variant(); 

	// ── Existing drag logic ──
	_currentlyDraggedCard = this;
	UICursor.Instance.SetGrab();

	var preview = new Label
	{
		Text                = BoundNPC.Name,
		Modulate            = new Color(1,1,1,0.9f),
		SizeFlagsHorizontal = SizeFlags.ExpandFill,
		HorizontalAlignment = HorizontalAlignment.Center
	};

	SetDragPreview(preview);
	return this;
}


	public override void _DropData(Vector2 atPosition, Variant data)
	{
		_currentlyDraggedCard = null;
		UICursor.Instance.SetIdle();
		base._DropData(atPosition, data);
	}
	
	public void SetCardLabels()
{
	var nameLabel = GetNode<Label>("VBoxContainer/NameLabel");
	var classLabel = GetNode<Label>("VBoxContainer/ClassLabel");

	if (BoundNPC != null)
	{
		var lastInitial = string.IsNullOrEmpty(BoundNPC.LastName) ? "" : $"{BoundNPC.LastName[0]}.";
		nameLabel.Text = $"{BoundNPC.FirstName} {lastInitial}";
		classLabel.Text = $"{BoundNPC.Level} {BoundNPC.ClassName}";
	}
}


	public static GuestCard CreateCardFor(Guest guest)
	{
		var card = GD.Load<PackedScene>("res://Scenes/UI/GuestCard.tscn").Instantiate<GuestCard>();

		card.BoundGuest = guest;
		card.BoundNPC = guest.BoundNPC;
		card.SetMouseFilter(Control.MouseFilterEnum.Stop);

		var nameLabel = card.GetNode<Label>("VBoxContainer/NameLabel");
		var classLabel = card.GetNode<Label>("VBoxContainer/ClassLabel");

		if (card.BoundNPC != null)
		{
			var lastInitial = string.IsNullOrEmpty(card.BoundNPC.LastName) ? "" : $"{card.BoundNPC.LastName[0]}.";
			nameLabel.Text = $"{card.BoundNPC.FirstName} {lastInitial}";
			classLabel.Text = $"{card.BoundNPC.Level} {card.BoundNPC.ClassName}";
			GameLog.Debug($"🧪 Card Name: {card.BoundNPC.FirstName} {card.BoundNPC.LastName}");

		}
		else
		{
			card.SetEmptySlot();
		}

		return card;
	}

// Hunger Display
public void UpdateBubbleDisplay()
{
	if (BoundNPC == null)
		return;

	GameLog.Debug($"🧪 {BoundNPC.FirstName} UpdateBubbleDisplay → Hungry={BoundNPC.IsHungry}, Thirsty={BoundNPC.IsThirsty}");

	if (HungryBubble == null)
		GD.PrintErr("❌ HungryBubble node is null.");
	if (ThirstyBubble == null)
		GD.PrintErr("❌ ThirstyBubble node is null.");

	// Use enum state instead of seating boolean
	bool isSeated = BoundGuest?.CurrentState == NPCState.Seats;

	// Show food bubble only if seated
	if (HungryBubble != null)
		HungryBubble.Visible = BoundNPC.IsHungry && isSeated;

	// Show drink bubble always if thirsty
	if (ThirstyBubble != null)
		ThirstyBubble.Visible = BoundNPC.IsThirsty;
}





private void OnHungryBubblePressed()
{
	ServeFoodRequested?.Invoke(this);
	HungryBubble.Visible = false;
}

private void OnThirstyBubblePressed()
{
	ServeDrinkRequested?.Invoke(this);
	ThirstyBubble.Visible = false;
}

 private void ToggleNpcDetails()
{
	// If it’s already open, close it
	if (_currentPopup != null && _currentPopup.IsInsideTree())
	{
		_currentPopup.QueueFree();
		_currentPopup = null;
		return;
	}

	// Otherwise, instantiate and show
	if (BoundNPC == null || NPCDetailPopupScene == null)
		return;

	_currentPopup = NPCDetailPopupScene.Instantiate<NPCDetailPopup>();
	GetTree().Root.AddChild(_currentPopup);
	_currentPopup.SetNPC(BoundNPC);
	_currentPopup.Show();
}

	
	private void OnGuiInput(InputEvent @event)
{
	if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
	{
		if (mb.Pressed)
		{
			UIAudio.Instance.PlayClick();
			_didDrag = false;
			_clickStartPos = mb.Position;
		}
		else
		{
			// on release: only open popup if no drag happened
			if (!_didDrag)
				ToggleNpcDetails();
			_didDrag = false;
		}
		GetViewport().SetInputAsHandled();
	}
	else if (@event is InputEventMouseMotion mm)
{
	// while left is held, if moved enough, mark as drag
	if (mm.ButtonMask.HasFlag(MouseButtonMask.Left) &&
		!_didDrag &&
		mm.Position.DistanceTo(_clickStartPos) > DragThreshold)
	{
		_didDrag = true;
	}
}

}






}
