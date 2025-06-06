using Godot;
using System;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("VBoxContainer/NewGame").Pressed += OnNewGamePressed;
		GetNode<Button>("VBoxContainer/Exit").Pressed += OnExitPressed;
	}

	private void OnNewGamePressed()
	{
		GD.Print("Starting new game...");
		GetTree().ChangeSceneToFile("res://Scenes/TavernMain.tscn");
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}
}
