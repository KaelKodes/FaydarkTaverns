using Godot;
using System;

public partial class AudioPanelManager : Panel
{
	[Export] public TextureButton AudioButton;
	[Export] public Panel AudioPanel;
	[Export] public HSlider MusicSlider;
	[Export] public HSlider InterfaceSlider;

	private int musicBusIndex;
	private int interfaceBusIndex;

	public override void _Ready()
	{
		AudioButton.Pressed += TogglePanel;
		AudioPanel.Visible = false;

		musicBusIndex = AudioServer.GetBusIndex("Music");
		interfaceBusIndex = AudioServer.GetBusIndex("Interface");

		MusicSlider.ValueChanged += v => SetBusVolume(musicBusIndex, v);
		InterfaceSlider.ValueChanged += v => SetBusVolume(interfaceBusIndex, v);

		// Apply initial values
		MusicSlider.Value = 50;
		InterfaceSlider.Value = 50;
	}

	private void TogglePanel()
	{
		AudioPanel.Visible = !AudioPanel.Visible;
		UIAudio.Instance?.PlayClick(); // Optional: click feedback
	}

	private void SetBusVolume(int index, double value)
	{
		float db = Mathf.Lerp(-40, 0, (float)value / 100f);
		AudioServer.SetBusVolumeDb(index, db);
	}
	public override void _UnhandledInput(InputEvent @event)
{
	if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
	{
		if (AudioPanel.Visible && !AudioPanel.GetGlobalRect().HasPoint(mouseEvent.GlobalPosition))
			AudioPanel.Visible = false;
	}
}

}
