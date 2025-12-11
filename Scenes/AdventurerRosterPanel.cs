using Godot;
using System;
using System.Collections.Generic;
using FaydarkTaverns.Objects;

public partial class AdventurerRosterPanel : PanelContainer
{
	[Export] public PackedScene GuestCardScene;
	private VBoxContainer adventurerListContainer;

	public override void _Ready()
	{
		GuestCardScene ??= GD.Load<PackedScene>("res://Scenes/UI/GuestCard.tscn");
		adventurerListContainer = GetNode<VBoxContainer>("AdventurerListContainer");

		GuestManager.OnGuestLeft += HandleGuestLeft;

	}

	public void Populate(List<Guest> guests)
	{
		foreach (Node child in adventurerListContainer.GetChildren())
			child.QueueFree();

		foreach (var guest in guests)
		{
			if (guest.CurrentState != NPCState.TavernFloor)
				continue;

			var card = GuestCardScene.Instantiate<GuestCard>();
			card.BoundGuest = guest;

			if (guest.BoundNPC != null)
				card.BoundNPC = guest.BoundNPC;

			adventurerListContainer.AddChild(card);
		}
	}

	private void HandleGuestLeft(Guest guest)
	{
		// Panel is gone
		if (!IsInsideTree())
			return;

		// VBox is gone
		if (adventurerListContainer == null ||
			!GodotObject.IsInstanceValid(adventurerListContainer))
			return;

		// Safe removal
		foreach (var card in adventurerListContainer.GetChildren())
		{
			if (card is GuestCard gc && gc.BoundGuest == guest)
			{
				gc.QueueFree();
				break;
			}
		}
	}

	public override void _ExitTree()
	{
		GuestManager.OnGuestLeft -= HandleGuestLeft;
	}


}
