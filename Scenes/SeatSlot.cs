using Godot;
using System;

public partial class SeatSlot : Panel
{
	[Export] public Color EmptyColor = new Color(0.4f, 0.4f, 0.4f);       // Gray
	[Export] public Color AdventurerColor = new Color(0.2f, 0.4f, 0.9f);   // Blue
	[Export] public Color QuestGiverColor = new Color(0.9f, 0.8f, 0.2f);   // Yellow

	private ColorRect background;

	public override void _Ready()
	{
		background = GetNode<ColorRect>("Background");
		SetEmpty(); // Default state
	}

	public void SetEmpty() => background.Color = EmptyColor;

	public void SetAdventurer() => background.Color = AdventurerColor;

	public void SetQuestGiver() => background.Color = QuestGiverColor;
}
