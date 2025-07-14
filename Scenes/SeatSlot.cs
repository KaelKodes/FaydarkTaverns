using Godot;
using System;

public partial class SeatSlot : Panel
{
	[Export] public Color EmptyColor = new Color(0, 0, 0, 0.4f); // 40% black overlay
	[Export] public Color AdventurerColor = new Color(0.2f, 0.4f, 0.9f);   // Blue
	[Export] public Color QuestGiverColor = new Color(0.9f, 0.8f, 0.2f);   // Yellow

	private ColorRect background;
	private bool hasBackground = false;

	public override void _Ready()
	{
		background = GetNodeOrNull<ColorRect>("Background");
		hasBackground = background != null;

		if (!hasBackground)
			GameLog.Debug($"ℹ️ SeatSlot at {GetPath()} has no Background. Skipping color updates.");
		else
			SetEmpty();
	}

	public void SetEmpty()
	{
		if (hasBackground)
			background.Color = EmptyColor;
	}

	public void SetAdventurer()
	{
		if (hasBackground)
			background.Color = AdventurerColor;
	}

	public void SetQuestGiver()
	{
		if (hasBackground)
			background.Color = QuestGiverColor;
	}
}
