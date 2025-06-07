using Godot;
using System;

public partial class AdventurerCard : PanelContainer
{
public Adventurer BoundAdventurer;

	public override Variant _GetDragData(Vector2 atPosition)
{
	if (BoundAdventurer == null)
		return new Variant(); // or: return default;

	var preview = new Label
	{
		Text = BoundAdventurer.Name,
		Modulate = new Color(1, 1, 1, 0.8f)
	};
	SetDragPreview(preview);

	// Godot auto-boxes reference types into Variant
	return BoundAdventurer;
}

}
