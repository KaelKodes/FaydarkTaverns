using Godot;
using System;

public partial class AdventurerCard : PanelContainer
{
public Adventurer BoundAdventurer;

	public override Variant _GetDragData(Vector2 atPosition)
{
	var label = new Label
	{
		Text = BoundAdventurer.Name,
		Modulate = new Color(1, 1, 1, 0.8f)
	};
	SetDragPreview(label);

	// Return this node — the drag source
	return this;
}


public Adventurer GetAdventurer() => BoundAdventurer;

}
