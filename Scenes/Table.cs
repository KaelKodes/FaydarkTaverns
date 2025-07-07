using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Table : Panel
{
	[Export] public int SeatCount = 4;
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

			// ✅ Set location to TableBase + TableId (use unique ID if you add it later)
			guest.LocationCode = (int)GuestLocation.TableBase; // Can be expanded later to +TableId

			UpdateSeatVisual(i, guest);

			if (LinkedPanel != null)
				LinkedPanel.UpdateSeatSlots();

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

		// ✅ Set location back to limbo; use TavernFloor later if needed
		guest.LocationCode = (int)GuestLocation.InTown;

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
