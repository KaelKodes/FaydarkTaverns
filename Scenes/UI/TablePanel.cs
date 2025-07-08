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
			UpdateSeatSlots();
		}
	}

	public void GenerateSeats(int count)
	{
		// 🔁 Clear existing
		foreach (Node child in SeatSlotContainer.GetChildren())
			child.QueueFree();

		// ➕ Add empty SeatSlot nodes
		for (int i = 0; i < count; i++)
		{
			var seatSlot = new SeatSlot(); // Custom control
			seatSlot.Name = $"SeatSlot{i + 1}";
			seatSlot.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
			seatSlot.MouseFilter = Control.MouseFilterEnum.Stop;

			SeatSlotContainer.AddChild(seatSlot);
		}
	}

	public void UpdateSeatSlots()
{
	if (LinkedTable == null || SeatSlotContainer == null)
		return;

	SeatSlotContainer.ClearChildren();

	for (int i = 0; i < LinkedTable.SeatCount; i++)
	{
		var guest = LinkedTable.SeatedGuests[i];

		// 🪑 Furniture Panel: simplified display
		if (Owner is FurniturePanel)
		{
			var slot = new SeatSlot();

			if (guest == null)
				slot.SetEmpty();
			else if (guest.IsAdventurer)
				slot.SetAdventurer();
			else
				slot.SetQuestGiver();

			SeatSlotContainer.AddChild(slot);
		}
		else // 🎴 ALC TablePanel: full draggable card display
		{
			if (guest != null && !guest.IsOnQuest)
			{
				var card = AdventurerCardScene.Instantiate<AdventurerCard>();

				// ✅ Bind the exact guest and adventurer
				card.BoundGuest = guest;
				card.BoundAdventurer = guest.BoundAdventurer;

				// ✅ Required to receive input events for drag
				card.SetMouseFilter(Control.MouseFilterEnum.Stop);

				// ✅ Populate display
				if (guest.BoundAdventurer != null)
				{
					card.GetNode<Label>("MarginContainer2/VBoxContainer/NameLabel").Text = guest.BoundAdventurer.Name;
					card.GetNode<Label>("MarginContainer2/VBoxContainer/ClassLabel").Text = $"{guest.BoundAdventurer.Level} {guest.BoundAdventurer.ClassName}";
					card.GetNode<Label>("MarginContainer2/VBoxContainer/VitalsLabel").Text = $"HP: {guest.BoundAdventurer.GetHp()} | Mana: {guest.BoundAdventurer.GetMana()}";
				}
				else if (guest.BoundGiver != null)
				{
					card.GetNode<Label>("MarginContainer2/VBoxContainer/NameLabel").Text = guest.BoundGiver.Name;
					card.GetNode<Label>("MarginContainer2/VBoxContainer/ClassLabel").Text = $"Lv {guest.BoundGiver.Level} Informant";
					card.GetNode<Label>("MarginContainer2/VBoxContainer/VitalsLabel").Text = $"Mood: {guest.BoundGiver.GetMoodStatus()}";
				}

				SeatSlotContainer.AddChild(card);
			}
			else
			{
				// 🕳️ Empty or quest-assigned slot
				var emptyPanel = new Panel();
				emptyPanel.CustomMinimumSize = new Vector2(250, 50);
				emptyPanel.AddThemeColorOverride("bg_color", new Color(0.15f, 0.15f, 0.15f));
				SeatSlotContainer.AddChild(emptyPanel);
			}
		}
	}
}


}
