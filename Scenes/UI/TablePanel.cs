using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using FaydarkTaverns.Objects;

public partial class TablePanel : VBoxContainer
{
	[Export] public Label TableNameLabel;
	[Export] public VBoxContainer SeatSlotContainer;
	[Export] public PackedScene GuestCardScene;

	public Table LinkedTable;

	public override void _Ready()
	{
		if (LinkedTable != null)
		{
			TableNameLabel.Text = LinkedTable.TableName;
			GenerateSeats(LinkedTable.SeatCount);
			CallDeferred(nameof(UpdateSeatSlots));
		}
	}

	public void GenerateSeats(int count)
	{
		if (SeatSlotContainer == null)
			return;

		foreach (Node child in SeatSlotContainer.GetChildren())
			child.QueueFree();

		for (int i = 0; i < count; i++)
		{
			var seatSlot = new SeatSlot
			{
				Name = $"SeatSlot{i + 1}",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				MouseFilter = Control.MouseFilterEnum.Stop
			};

			SeatSlotContainer.AddChild(seatSlot);
		}
	}

public void UpdateSeatSlots()
{
	if (LinkedTable == null || SeatSlotContainer == null)
	{
		GD.PrintErr("⛔ TablePanel: UpdateSeatSlots called with null LinkedTable or SeatSlotContainer.");
		return;
	}

	SeatSlotContainer.ClearChildren();

	for (int i = 0; i < LinkedTable.SeatCount; i++)
	{
		var guest = LinkedTable.SeatedGuests.ElementAtOrDefault(i);

		// 🎛️ Furniture mode: show basic slot visuals only
		if (Owner is FurniturePanel)
		{
			var slot = new SeatSlot();

			if (guest == null)
				slot.SetEmpty();
			else if (guest.IsAdventurer)
				slot.SetAdventurer();
			else if (guest.IsQuestGiver)
				slot.SetQuestGiver();
			else
				slot.SetEmpty();

			SeatSlotContainer.AddChild(slot);
		}
		else // 👥 Guest mode: show full GuestCards
		{
			var card = GuestCardScene.Instantiate<GuestCard>();
			card.SetMouseFilter(Control.MouseFilterEnum.Stop);

			if (guest != null && !guest.IsOnQuest && guest.BoundNPC != null)
			{
				card.BoundGuest = guest;
				card.BoundNPC = guest.BoundNPC;

				var lastInitial = string.IsNullOrEmpty(guest.BoundNPC.LastName) ? "" : $"{guest.BoundNPC.LastName[0]}.";
				card.GetNode<Label>("VBoxContainer/NameLabel").Text = $"{guest.BoundNPC.FirstName} {lastInitial}";
				card.GetNode<Label>("VBoxContainer/ClassLabel").Text = $"{guest.BoundNPC.Level} {guest.BoundNPC.ClassName}";

				// ✅ Hook up event handlers for bubble click actions
				card.ServeFoodRequested += TavernManager.Instance.OnServeFoodRequestedFromCard;
				card.ServeDrinkRequested += TavernManager.Instance.OnServeDrinkRequestedFromCard;

				// ✅ Ensure bubbles are evaluated after scene readiness
				card.CallDeferred(nameof(card.UpdateBubbleDisplay));
			}
			else
			{
				card.SetEmptySlot();
			}

			SeatSlotContainer.AddChild(card);
		}
	}
}

}
