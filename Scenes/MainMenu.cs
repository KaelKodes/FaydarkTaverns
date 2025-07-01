using Godot;
using System;

public partial class MainMenu : Control
{
	private CheckBox DebugToggle;
	
	public override void _Ready()
	{
		GetNode<Button>("VBoxContainer/NewGame").Pressed += OnNewGamePressed;
		GetNode<Button>("VBoxContainer/Exit").Pressed += OnExitPressed;
		DebugToggle = GetNode<CheckBox>("DebugToggle");
		DebugToggle.Toggled += OnDebugToggleChanged;
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
	
	private void OnDebugToggleChanged(bool toggled)
	{
		GameDebug.DebugMode = toggled;
		GD.Print($"🐞 Debug Mode: {(toggled ? "ON" : "OFF")}");
	}
	
}
