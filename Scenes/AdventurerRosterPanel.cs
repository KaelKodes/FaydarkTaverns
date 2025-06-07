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

	public void Populate(List<Adventurer> adventurers)
	{
		// Clear existing cards
		foreach (Node child in adventurerListContainer.GetChildren())
		{
			child.QueueFree();
		}

		// Add new cards
		foreach (var adventurer in adventurers)
		{
			var card = AdventurerCardScene.Instantiate<AdventurerCard>();
card.BoundAdventurer = adventurer;
;
			adventurerListContainer.AddChild(card);
		}
	}
}
