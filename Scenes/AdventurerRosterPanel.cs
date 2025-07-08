using Godot;
using System;
using System.Collections.Generic;

public partial class AdventurerRosterPanel : PanelContainer
{
	[Export] public PackedScene AdventurerCardScene;
	private VBoxContainer adventurerListContainer;

	public override void _Ready()
	{
		AdventurerCardScene ??= GD.Load<PackedScene>("res://Scenes/UI/AdventurerCard.tscn");
		adventurerListContainer = GetNode<VBoxContainer>("ScrollContainer/AdventurerListContainer");
	}

	public void Populate(List<Guest> guests)
{
	foreach (Node child in adventurerListContainer.GetChildren())
		child.QueueFree();

	foreach (var guest in guests)
	{
		var card = AdventurerCardScene.Instantiate<AdventurerCard>();
		card.BoundGuest = guest;
		if (guest.BoundAdventurer != null)
			card.BoundAdventurer = guest.BoundAdventurer;
		if (guest.BoundGiver != null)
			card.BoundGiver = guest.BoundGiver;

		adventurerListContainer.AddChild(card);
	}
}

}
