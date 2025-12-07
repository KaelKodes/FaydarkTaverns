using Godot;
using System;
using FaydarkTaverns.Objects;

public partial class GuestCard : Panel
{
	// ============================================================
	//  EXPORTED / BOUND DATA
	// ============================================================
	[Export] public PackedScene NPCDetailPopupScene;

	public Guest BoundGuest { get; set; }
	public NPCData BoundNPC { get; set; }

	// Bubble nodes
	private TextureButton HungryBubble;
	private TextureButton ThirstyBubble;

	// Feed signals
	public event Action<GuestCard> ServeFoodRequested;
	public event Action<GuestCard> ServeDrinkRequested;

	private ColorRect background;

	// ============================================================
	//  DRAGGING / POPUP
	// ============================================================
	private static GuestCard _currentlyDraggedCard = null;
	private NPCDetailPopup _currentPopup;

	public static bool IsDraggingAdventurer() => _currentlyDraggedCard != null;
	public static GuestCard GetCurrentlyDraggedCard() => _currentlyDraggedCard;

	private Vector2 _clickStartPos;
	private bool _didDrag;
	private const float DragThreshold = 10f;


	// ============================================================
	//  READY — Initialize visuals & interactions
	// ============================================================
	public override void _Ready()
	{
		background = GetNodeOrNull<ColorRect>("Background");
		SetMouseFilter(MouseFilterEnum.Stop);

		InitializeBackgroundColor();
		SetCardLabels();
		InitializeKickButton();
		InitializeBubbles();
		InitializePortrait();
		InitializeMouseFeedback();

		GuiInput += OnGuiInput;
	}

	private void InitializeBackgroundColor()
	{
		if (BoundNPC == null)
			return;

		switch (BoundNPC.Role)
		{
			case NPCRole.Adventurer:
				SetDefaultColor();
				break;

			case NPCRole.QuestGiver:
				SetBackgroundColor(new Color(0.5f, 0.5f, 0.5f));
				break;

			default:
				SetBackgroundColor(new Color(0.4f, 0.4f, 0.4f));
				break;
		}
	}

	private void InitializeKickButton()
	{
		var button = GetNode<Button>("KickButton");

		button.Text = "❌";
		button.FocusMode = Control.FocusModeEnum.None;

		button.Pressed += OnRemovePressed;

		var transparent = new StyleBoxFlat { BgColor = new Color(0, 0, 0, 0) };
		button.AddThemeStyleboxOverride("normal", transparent);
		button.AddThemeStyleboxOverride("hover", transparent);
		button.AddThemeStyleboxOverride("pressed", transparent);

		// Button emoji feedback
		button.MouseEntered += () => button.Text = "🥾";
		button.MouseExited += () => button.Text = "❌";
	}

	private void InitializeBubbles()
	{
		HungryBubble  = GetNodeOrNull<TextureButton>("BubbleControl/HungryBubble");
		ThirstyBubble = GetNodeOrNull<TextureButton>("BubbleControl/ThirstyBubble");

		if (HungryBubble != null)
		{
			HungryBubble.Pressed += OnHungryBubblePressed;
			HungryBubble.Visible = false;
		}

		if (ThirstyBubble != null)
		{
			ThirstyBubble.Pressed += OnThirstyBubblePressed;
			ThirstyBubble.Visible = false;
		}
	}

	private void InitializePortrait()
	{
		var portrait = GetNodeOrNull<TextureRect>("MarginContainer/Portrait");
		if (portrait == null || BoundNPC == null)
			return;

		portrait.Position += new Vector2(25, 0);

		Gender gender = BoundGuest?.Gender ?? Gender.Male;
		string className = BoundNPC.ClassName ?? "Informant";
		int portraitId = BoundNPC.PortraitId;

		string initial = gender == Gender.Male ? "M" : "F";
		string path = $"res://Assets/UI/ClassPortraits/{className}/{className}{initial}{portraitId}.jpg";

		portrait.Texture = ResourceLoader.Load<Texture2D>(path);
	}

	private void InitializeMouseFeedback()
	{
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
	}


	// ============================================================
	//  GENERAL UI SUPPORT
	// ============================================================
	public void SetBackgroundColor(Color color)
	{
		if (background != null)
			background.Color = color;
	}

	public void SetDefaultColor()
	{
		SetBackgroundColor(new Color(0.3f, 0.4f, 0.6f));
	}

	public void SetCardLabels()
	{
		var nameLabel  = GetNode<Label>("VBoxContainer/NameLabel");
		var classLabel = GetNode<Label>("VBoxContainer/ClassLabel");

		if (BoundNPC != null)
		{
			var lastInitial = string.IsNullOrEmpty(BoundNPC.LastName) ? "" : $"{BoundNPC.LastName[0]}.";
			nameLabel.Text  = $"{BoundNPC.FirstName} {lastInitial}";
			classLabel.Text = $"{BoundNPC.Level} {BoundNPC.ClassName}";
		}
	}


	// ============================================================
	//  EMPTY SLOT MODE
	// ============================================================
	public void SetEmptySlot()
	{
		GetNode<Control>("VBoxContainer").Visible = false;

		var bg = GetNode<ColorRect>("Background");
		bg.Color = new Color(0f, 0f, 0f, 0.3f);

		var portrait = GetNode<TextureRect>("MarginContainer/Portrait");
		portrait.Texture = null;
		portrait.Visible = false;

		GetNode<Button>("KickButton").Visible = false;
	}


	// ============================================================
	//  DRAG & DROP SUPPORT
	// ============================================================
	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (BoundNPC == null || BoundNPC.Role != NPCRole.Adventurer)
			return new Variant();

		_currentlyDraggedCard = this;
		UICursor.Instance.SetGrab();

		var preview = new Label
		{
			Text = BoundNPC.Name,
			Modulate = new Color(1,1,1,0.9f),
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

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb)
		{
			if (!mb.Pressed)
			{
				_currentlyDraggedCard = null;
				UICursor.Instance.SetIdle();
			}
			else if (mb.ButtonIndex == MouseButton.Left)
			{
				GetViewport().SetInputAsHandled();
			}
		}
		else if (@event is InputEventMouseMotion mm)
		{
			if (mm.ButtonMask.HasFlag(MouseButtonMask.Left) &&
				!_didDrag &&
				mm.Position.DistanceTo(_clickStartPos) > DragThreshold)
			{
				_didDrag = true;
			}
		}
	}


	// ============================================================
	//  CARD FACTORY
	// ============================================================
	public static GuestCard CreateCardFor(Guest guest)
	{
		var card = GD.Load<PackedScene>("res://Scenes/UI/GuestCard.tscn").Instantiate<GuestCard>();

		card.BoundGuest = guest;
		card.BoundNPC = guest.BoundNPC;
		card.SetMouseFilter(Control.MouseFilterEnum.Stop);

		card.SetCardLabels();

		if (card.BoundNPC == null)
			card.SetEmptySlot();

		// Hook into FeedMenu
		FeedMenu.Instance?.ConnectGuestCard(card);
		return card;
	}


	// ============================================================
	//  REQUEST BUBBLE LOGIC
	// ============================================================
	public void ShowRequestBubble(bool visible)
	{
		if (BoundNPC == null)
			return;

		if (HungryBubble != null)
			HungryBubble.Visible = visible && BoundNPC.IsHungry;

		if (ThirstyBubble != null)
			ThirstyBubble.Visible = visible && BoundNPC.IsThirsty;
	}

	public void UpdateBubbleDisplay()
	{
		if (BoundNPC == null)
			return;

		bool isSeated = BoundGuest?.CurrentState == NPCState.Seats;

		if (HungryBubble != null)
			HungryBubble.Visible = BoundNPC.IsHungry && isSeated;

		if (ThirstyBubble != null)
			ThirstyBubble.Visible = BoundNPC.IsThirsty;
	}

	private void OnHungryBubblePressed()
	{
		ServeFoodRequested?.Invoke(this);
	}

	private void OnThirstyBubblePressed()
	{
		ServeDrinkRequested?.Invoke(this);
	}


	// ============================================================
	//  REACTION DISPLAY
	// ============================================================
	public void ShowReaction(ConsumptionReaction reaction)
	{
		GD.Print($"Guest {BoundNPC?.Name} reaction: {reaction}");

		switch (reaction)
		{
			case ConsumptionReaction.Loved:
				ShowFloatingEmote("❤️ Loved it!");
				break;
			case ConsumptionReaction.Liked:
				ShowFloatingEmote("🙂 Liked it.");
				break;
			case ConsumptionReaction.Neutral:
				ShowFloatingEmote("😐 Neutral.");
				break;
			case ConsumptionReaction.Disliked:
				ShowFloatingEmote("💀 Disliked it.");
				break;
		}
	}

	private void ShowFloatingEmote(string text)
	{
		GD.Print($"[EMOTE] {text}");
	}


	// ============================================================
	//  REMOVE GUEST
	// ============================================================
	private void OnRemovePressed()
	{
		if (BoundGuest == null)
			return;

		// Remove from table
		if (BoundGuest.AssignedTable != null)
		{
			BoundGuest.AssignedTable.RemoveGuest(BoundGuest);
			BoundGuest.AssignedTable = null;
			BoundGuest.SeatIndex = null;
		}

		// Remove from tavern
		if (TavernManager.Instance.GetGuestsInside().Contains(BoundGuest))
			TavernManager.Instance.OnGuestRemoved(BoundGuest);

		GuestManager.QueueGuest(BoundGuest);
		GameLog.Info($"🚶 {BoundGuest.Name} left to rejoin the queue.");

		UICursor.Instance.SetIdle();
		TavernManager.Instance.DisplayAdventurers();
		TavernManager.Instance.UpdateFloorLabel();

		QueueFree();
	}


	// ============================================================
	//  POPUP (NPC DETAILS)
	// ============================================================
	private void ToggleNpcDetails()
	{
		if (_currentPopup != null && _currentPopup.IsInsideTree())
		{
			_currentPopup.QueueFree();
			_currentPopup = null;
			return;
		}

		if (BoundNPC == null || NPCDetailPopupScene == null)
			return;

		_currentPopup = NPCDetailPopupScene.Instantiate<NPCDetailPopup>();
		GetTree().Root.AddChild(_currentPopup);
		_currentPopup.SetNPC(BoundNPC);
		_currentPopup.Show();
	}

	private void OnGuiInput(InputEvent @event)
{
	if (@event is InputEventMouseButton e && e.ButtonIndex == MouseButton.Left)
	{
		if (e.Pressed)
		{
			UIAudio.Instance.PlayClick();
			_didDrag = false;
			_clickStartPos = e.Position;
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
