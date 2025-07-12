using Godot;
using System;

public partial class MainMenuSlider : Control
{
	[Export] public HSlider MusicSlider;
	[Export] public Label VolumeLabel;
	[Export] public AudioStreamPlayer MusicPlayer;

	public override void _Ready()
	{
		MusicSlider.ValueChanged += OnVolumeChanged;
		MusicSlider.Value = 50; // default 50%
		OnVolumeChanged(50);
	}

	private void OnVolumeChanged(double value)
	{
		var db = Mathf.Lerp(-40, 0, (float)value / 100f); // Godot uses dB (-40 = silent, 0 = full)
		MusicPlayer.VolumeDb = db;
		VolumeLabel.Text = $"{value}%";
	}
}
