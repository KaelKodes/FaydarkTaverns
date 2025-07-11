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
	}

	public void Populate(List<Guest> guests)
{
	foreach (Node child in adventurerListContainer.GetChildren())
		child.QueueFree();

	foreach (var guest in guests)
	{
		var card = GuestCardScene.Instantiate<GuestCard>();
		card.BoundGuest = guest;

		if (guest.BoundNPC != null)
			card.BoundNPC = guest.BoundNPC;

		adventurerListContainer.AddChild(card);
	}
}


}
