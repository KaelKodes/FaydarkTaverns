using Godot;
using System;

public partial class ItemSlot : Control
{
	public void SetItem(Texture2D icon, int count)
	{
		GetNode<TextureRect>("ItemIcon").Texture = icon;
		GetNode<Label>("StackLabel").Text = $"x{count}";
	}
}
