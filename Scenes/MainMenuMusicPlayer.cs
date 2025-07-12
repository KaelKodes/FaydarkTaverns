using Godot;
using System;

public partial class MainMenuMusicPlayer : AudioStreamPlayer
{
	[Export] public float DelayBeforeLoop = 3f;

	public override void _Ready()
	{
		Finished += OnMusicFinished;
		Play();
	}

	private async void OnMusicFinished()
	{
		GD.Print("🎵 Song finished. Waiting to loop...");
		await ToSignal(GetTree().CreateTimer(DelayBeforeLoop), "timeout");
		Play();
	}
}
