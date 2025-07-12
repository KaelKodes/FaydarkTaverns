using Godot;
using System;

public partial class MainMenu : Control
{
	private CheckBox DebugToggle;
	private HSlider MusicSlider;
	private Label VolumeLabel;
	private AudioStreamPlayer MusicPlayer;

	public override void _Ready()
	{
		// 🎮 Core buttons
		GetNode<Button>("VBoxContainer/NewGame").Pressed += OnNewGamePressed;
		GetNode<Button>("VBoxContainer/Exit").Pressed += OnExitPressed;

		// 🐞 Debug toggle
		DebugToggle = GetNode<CheckBox>("DebugToggle");
		DebugToggle.Toggled += OnDebugToggleChanged;

		// 🎵 Music and volume
		MusicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
		MusicSlider = GetNode<HSlider>("HBoxContainer/HSlider");
		VolumeLabel = GetNode<Label>("HBoxContainer/MusicVolumeLabel");

		MusicSlider.ValueChanged += OnVolumeChanged;
		MusicSlider.Value = 50; // default 50%
		OnVolumeChanged(50);    // apply on load

		MusicPlayer.Finished += OnMusicFinished;
		MusicPlayer.Play();
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

	private void OnVolumeChanged(double value)
	{
		// VolumeDb expects dB: -40 is silent, 0 is max
		var db = Mathf.Lerp(-40, 0, (float)value / 100f);
		MusicPlayer.VolumeDb = db;
		VolumeLabel.Text = $"{value}%";
	}

	private async void OnMusicFinished()
	{
		GD.Print("🎵 Music finished. Looping in 3 seconds...");
		await ToSignal(GetTree().CreateTimer(3f), "timeout");
		MusicPlayer.Play();
	}
}
