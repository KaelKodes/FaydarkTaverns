using Godot;
using System;

public partial class EscMenu : Control
{
	[Export] public Button ResumeButton;
	[Export] public Button SaveButton;
	[Export] public Button LoadButton;
	[Export] public Button QuitButton;

	[Export] public PlayThroughSelectWindow PlaythroughWindow;
	[Export] public ConfirmationWindow ConfirmationWindow;

	public override void _Ready()
	{
		Visible = false;

		ResumeButton.Pressed += () => HideMenu();
		QuitButton.Pressed   += ShowQuitConfirmation;

		SaveButton.Pressed += () =>
		{
			HideMenu();
			PlaythroughWindow.Open(PlaythroughSelectMode.SaveGame);
		};

		LoadButton.Pressed += () =>
		{
			HideMenu();
			PlaythroughWindow.Open(PlaythroughSelectMode.LoadGame);
		};
	}

	public void ShowMenu()
	{
		Visible = true;
		GetTree().Paused = true;
	}

	public void HideMenu()
	{
		Visible = false;
		GetTree().Paused = false;
	}

	private void ShowQuitConfirmation()
{
	HideMenu();

	ConfirmationWindow.ShowWindow(
		"Are you sure you want to return to the main menu?",
		() => 
		{
			GetTree().Paused = false; // ensure time unfreezes
			GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
		},
		() => ShowMenu()
	);
}

}
