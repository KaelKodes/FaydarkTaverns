using Godot;
using System;

public partial class FurniturePanel : Panel
{
	private bool _dragging = false;
	private Vector2 _dragOffset;
	public VBoxContainer FurnitureVBox => GetNode<VBoxContainer>("FurnitureVBox");


	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				_dragging = true;
				_dragOffset = GetGlobalMousePosition() - GlobalPosition;
			}
			else if (!mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
			{
				_dragging = false;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (_dragging)
		{
			GlobalPosition = GetGlobalMousePosition() - _dragOffset;
		}
	}
}
