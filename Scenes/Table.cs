using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Table : Panel
{
	[Export] public int SeatCount = 4;
	private PackedScene seatSlotScene = GD.Load<PackedScene>("res://Scenes/SeatSlot.tscn");
	[Export] public PackedScene SeatSlotScene;
	[Export] public NodePath SeatRowPath;
	[Export] public NodePath NameLabelPath;
	private Label nameLabel;
	public string TableName = "Starting Table";
	private HBoxContainer seatRow;
	public TablePanel LinkedPanel;

	public List<Guest> SeatedGuests = new();
	public List<Guest> AssignedGuests = new();

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
	SeatedGuests.Add(null); // ✅ Add this line
	AssignedGuests.Add(null);
}

}


	

	public bool HasFreeSeat() => GetFreeSeatCount() > 0;

	public int GetFreeSeatCount() => AssignedGuests.Count(g => g == null);

	public int AssignGuest(Guest guest)
{
	for (int i = 0; i < SeatedGuests.Count; i++)
	{
		if (SeatedGuests[i] == null)
		{
			SeatedGuests[i] = guest;
			guest.SeatIndex = i;
			guest.AssignedTable = this;

			UpdateSeatVisual(i, guest); // ✅ optional visual

			// 🔁 NEW: Update TablePanel immediately
			if (LinkedPanel != null)
				LinkedPanel.UpdateSeats(TavernManager.Instance.GetGuestsInside());
				
				TavernManager.Instance.DisplayAdventurers();
				TavernManager.Instance.UpdateFloorLabel();

			return i;
		}
	}
	return -1;
}


	public void RemoveGuest(Guest guest)
{
	if (guest.SeatIndex != null && guest.SeatIndex >= 0 && guest.SeatIndex < SeatedGuests.Count)
	{
		SeatedGuests[(int)guest.SeatIndex] = null;
		GD.Print($"🪑 Removed {guest.Name} from seat {guest.SeatIndex}");
	}
	else
	{
		GD.PrintErr($"❌ Tried to remove guest from invalid seat index: {guest.SeatIndex}");
	}

	guest.AssignedTable = null;
	guest.SeatIndex = null;
}


	private void UpdateSeatVisual(int index, Guest guest)
	{
		// Find the seat node by linear index
		var allSeats = new List<SeatSlot>();
		allSeats.AddRange(GetNode<HBoxContainer>("HBoxContainer/SeatRow").GetChildren().OfType<SeatSlot>());

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
	public int GetFreeSeatIndex()
{
	for (int i = 0; i < SeatedGuests.Count; i++)
	{
		if (SeatedGuests[i] == null)
			return i;
	}
	return -1;
}


	
}
