using Godot;
using System;

public partial class TorchLight : PointLight2D
{
	[Export] public float DayEnergy = 0.2f;
	[Export] public float NightEnergy = 1.0f;

	// Much smaller flicker strength
	[Export] public float FlickerStrength = 0.01f;

	// Flicker speed (lower = slower and safer)
	[Export] public float FlickerSpeed = 0.5f;

	private ClockManager clock;
	private float flickerTime = 0f;

	public override void _Ready()
	{
		// Find the autoloaded ClockManager safely
		clock = GetNodeOrNull<ClockManager>("/root/ClockManager");
		if (clock != null && GodotObject.IsInstanceValid(clock))
		{
			clock.OnTimeAdvanced += UpdateLight;
		}
		else
		{
			GD.PrintErr("[TorchLight] ClockManager not found; torch will not react to time.");
		}

		// Initialize to current time (fresh game or restored)
		UpdateLight(ClockManager.CurrentTime);
	}

	public override void _ExitTree()
	{
		// Make sure we don't keep receiving events after being freed
		if (clock != null && GodotObject.IsInstanceValid(clock))
		{
			clock.OnTimeAdvanced -= UpdateLight;
		}
	}

	private void UpdateLight(DateTime time)
	{
		// 🔒 Hard guard: if this light has been disposed, do nothing
		if (!GodotObject.IsInstanceValid(this))
			return;

		float hour = time.Hour + time.Minute / 60f;

		// Night factor: 0 at noon (brightest), 1 at midnight (darkest)
		float nightFactor = Mathf.Abs(hour - 12f) / 12f;

		float baseEnergy = Mathf.Lerp(DayEnergy, NightEnergy, nightFactor);

		// Smooth flicker using sine wave (very gentle)
		flickerTime += FlickerSpeed * 0.016f; // approximate delta
		float flicker = Mathf.Sin(flickerTime) * FlickerStrength;

		Energy = baseEnergy + flicker;
	}
}
