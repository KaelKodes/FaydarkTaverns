using Godot;
using System;

public partial class FurniturePanel : Panel
{
	private bool _dragging = false;
	private Vector2 _dragOffset;

	public VBoxContainer FurnitureVBox => GetNode<VBoxContainer>("FurnitureVBox");

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			if (mouseEvent.Pressed)
			{
				_dragging = true;
				_dragOffset = GetGlobalMousePosition() - GlobalPosition;
			}
			else
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
