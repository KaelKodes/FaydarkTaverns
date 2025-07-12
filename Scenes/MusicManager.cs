using Godot;
using System;
using System.Collections.Generic;

public partial class MusicManager : Node
{
	[Export] public AudioStreamPlayer MusicPlayer;
	[Export] public float DelayBetweenTracks = 3f;

	private List<AudioStream> playlist = new();
	private int currentTrackIndex = 0;
	private Timer delayTimer;
	private AudioStreamPlayback playback;
	private bool wasPlayingBeforePause = false;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		SetProcess(true);

		delayTimer = new Timer();
		delayTimer.OneShot = true;
		AddChild(delayTimer);
		delayTimer.Timeout += PlayNextSong;

		// 🎵 Add your default songs here
		playlist.Add(GD.Load<AudioStream>("res://Assets/Audio/Music/We'reJustGettingStartedHere.mp3"));
		playlist.Add(GD.Load<AudioStream>("res://Assets/Audio/Music/ATavern'sDawn.mp3"));
		playlist.Add(GD.Load<AudioStream>("res://Assets/Audio/Music/ThisOl'DustyBar.mp3"));

		MusicPlayer.Finished += OnSongFinished;
		PlayNextSong();
	}

	private void OnSongFinished()
	{
		delayTimer.Start(DelayBetweenTracks); // wait before next song
	}

	private void PlayNextSong()
	{
		if (playlist.Count == 0)
			return;

		currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
		MusicPlayer.Stream = playlist[currentTrackIndex];
		MusicPlayer.Play();
		playback = MusicPlayer.GetStreamPlayback();
	}

	public override void _Notification(int what)
{
	if (what == NotificationPaused)
	{
		if (MusicPlayer.Playing)
		{
			MusicPlayer.StreamPaused = true;
		}
	}
	else if (what == NotificationUnpaused)
	{
		if (MusicPlayer.StreamPaused)
		{
			MusicPlayer.StreamPaused = false;
		}
	}
}
public void TogglePause(bool paused)
{
	if (paused)
	{
		if (MusicPlayer.Playing)
			MusicPlayer.StreamPaused = true;
	}
	else
	{
		if (MusicPlayer.StreamPaused)
			MusicPlayer.StreamPaused = false;
	}
}


}
