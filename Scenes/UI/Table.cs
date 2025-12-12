using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using FaydarkTaverns.Objects;

public partial class Table : Panel
{
	[Export] public int SeatCount = 2;
	private PackedScene seatSlotScene = GD.Load<PackedScene>("res://Scenes/SeatSlot.tscn");

	private Label nameLabel;
	private HBoxContainer seatRow;

	public string TableName = "Starting Table";
	public TablePanel LinkedPanel;

	public List<Guest> SeatedGuests { get; private set; } = new();
	public List<Guest> AssignedGuests { get; private set; } = new();

	public override void _Ready()
	{
		seatRow = GetNode<HBoxContainer>("HBoxContainer/SeatRow");
		nameLabel = GetNode<Label>("HBoxContainer/NameLabel");

		nameLabel.Text = TableName;

		seatRow.ClearChildren();

		for (int i = 0; i < SeatCount; i++)
		{
			var slot = seatSlotScene.Instantiate<SeatSlot>();
			seatRow.AddChild(slot);
			SeatedGuests.Add(null);
			AssignedGuests.Add(null);
		}
	}

	public bool HasFreeSeat() => GetFreeSeatCount() > 0;

	public int GetFreeSeatCount() => AssignedGuests.Count(g => g == null);

	public int GetFreeSeatIndex()
	{
		for (int i = 0; i < SeatedGuests.Count; i++)
		{
			if (SeatedGuests[i] == null)
				return i;
		}
		return -1;
	}

	public int AssignGuest(Guest guest)
{
	for (int i = 0; i < SeatedGuests.Count; i++)
	{
		if (SeatedGuests[i] == null)
		{
			SeatedGuests[i] = guest;
			guest.SeatIndex = i;
			guest.AssignedTable = this;

			// ✅ Set state to seated
			guest.SetState(NPCState.Seats);
			if (guest.BoundNPC != null)
			guest.BoundNPC.State = guest.CurrentState;


			UpdateSeatVisual(i, guest);

			// ✅ Refresh the GuestCard (if exists) after sitting
			LinkedPanel?.UpdateSeatSlots();

			// 🔥 Update bubble for seated guest card
			if (LinkedPanel != null)
			{
				foreach (var child in LinkedPanel.GetChildren())
				{
					if (child is GuestCard card && card.BoundGuest == guest)
						card.UpdateBubbleDisplay();
				}
			}

			TavernManager.Instance.DisplayAdventurers();
			TavernManager.Instance.UpdateFloorLabel();

			return i;
		}
	}
	return -1;
}


	public void RemoveGuest(Guest guest)
	{
		int index = SeatedGuests.IndexOf(guest);
		if (index >= 0)
		{
			SeatedGuests[index] = null;
			guest.AssignedTable = null;
			guest.SeatIndex = -1;

			// ✅ Reset to idle state on tavern floor
			guest.SetState(NPCState.TavernFloor);
			if (guest.BoundNPC != null)
			guest.BoundNPC.State = guest.CurrentState;


			UpdateSeatVisual(index, null);
			LinkedPanel?.UpdateSeatSlots();
		}
	}

	private void UpdateSeatVisual(int index, Guest guest)
	{
		var allSeats = GetNode<HBoxContainer>("HBoxContainer/SeatRow")
			.GetChildren()
			.OfType<SeatSlot>()
			.ToList();

		if (index >= 0 && index < allSeats.Count)
		{
			var seat = allSeats[index];
			if (guest == null)
				seat.SetEmpty();
			else if (guest.IsAdventurer)
				seat.SetAdventurer();
			else
				seat.SetQuestGiver();
		}
	}
}
