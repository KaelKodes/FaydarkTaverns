using Godot;
using System;
using System.Linq;
using System.Collections.Generic;


public partial class TablePanel : VBoxContainer
{
	[Export] public Label TableNameLabel;
	[Export] public VBoxContainer SeatSlotContainer;
	[Export] public PackedScene AdventurerCardScene;


	public Table LinkedTable;

	public override void _Ready()
	{
		if (LinkedTable != null)
		{
			TableNameLabel.Text = LinkedTable.TableName;
			GenerateSeats(LinkedTable.SeatCount);
			UpdateSeats(LinkedTable.SeatedGuests);
		}
	}

	public void GenerateSeats(int count)
{
	// Clear existing slots
	foreach (Node child in SeatSlotContainer.GetChildren())
	{
		child.QueueFree();
	}

	// Create new slots
	for (int i = 0; i < count; i++)
	{
		var seatSlot = new Panel(); // Replace with a custom scene if needed
		seatSlot.Name = $"SeatSlot{i + 1}";
		seatSlot.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		seatSlot.MouseFilter = Control.MouseFilterEnum.Stop;

		SeatSlotContainer.AddChild(seatSlot);
	}
}
public void UpdateSeats(List<Guest> seatedGuests)
{
	if (SeatSlotContainer == null || LinkedTable == null)
		return;

	// Clear all UI slots
	foreach (var child in SeatSlotContainer.GetChildren())
		child.QueueFree();

	for (int i = 0; i < LinkedTable.SeatCount; i++)
	{
		Guest guest = (i < seatedGuests.Count) ? seatedGuests[i] : null;

		if (guest != null && guest.BoundAdventurer != null)
		{
			var card = AdventurerCardScene.Instantiate<AdventurerCard>();
			card.BoundAdventurer = guest.BoundAdventurer;

			var nameLabel = card.GetNode<Label>("VBoxContainer/NameLabel");
			var classLabel = card.GetNode<Label>("VBoxContainer/ClassLabel");
			var vitalsLabel = card.GetNode<Label>("VBoxContainer/VitalsLabel");

			nameLabel.Text = guest.BoundAdventurer.Name;
			classLabel.Text = $"{guest.BoundAdventurer.Level} {guest.BoundAdventurer.ClassName}";
			vitalsLabel.Text = $"HP: {guest.BoundAdventurer.GetHp()} | Mana: {guest.BoundAdventurer.GetMana()}";

			SeatSlotContainer.AddChild(card);
		}
		else
		{
			var emptySlot = new Panel();
			emptySlot.CustomMinimumSize = new Vector2(250, 50);
			emptySlot.AddThemeColorOverride("bg_color", new Color(0.15f, 0.15f, 0.15f));
			SeatSlotContainer.AddChild(emptySlot);
		}
	}
}




}
