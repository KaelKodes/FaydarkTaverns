using Godot;
using System;

public partial class DayNightTint : CanvasModulate
{
	private ClockManager clock;

	public override void _Ready()
	{
		// Safely grab the autoloaded ClockManager
		clock = GetNodeOrNull<ClockManager>("/root/ClockManager");

		if (clock != null && GodotObject.IsInstanceValid(clock))
		{
			clock.OnTimeAdvanced += UpdateTint;
		}
		else
		{
			GD.PrintErr("[DayNightTint] ClockManager not found; tint will not update with time.");
		}

		// Initialize tint to whatever time we’re currently at
		UpdateTint(ClockManager.CurrentTime);
	}

	public override void _ExitTree()
	{
		// IMPORTANT: unsubscribe so dead tints don’t keep getting updates
		if (clock != null && GodotObject.IsInstanceValid(clock))
		{
			clock.OnTimeAdvanced -= UpdateTint;
		}
	}

	private void UpdateTint(DateTime t)
	{
		// Extra safety: if this node has been freed, do nothing.
		if (!GodotObject.IsInstanceValid(this))
			return;

		float hour = t.Hour + t.Minute / 60f;
		float brightness = CalculateBrightness(hour);

		// Warm tint scaling with brightness
		Color = new Color(
			Mathf.Lerp(0.55f, 1f, brightness),   // Red
			Mathf.Lerp(0.45f, 1f, brightness),   // Green
			Mathf.Lerp(0.60f, 1f, brightness)    // Blue (slightly cool when dark)
		);
	}

	private float CalculateBrightness(float hour)
	{
		// --- DARKEST NIGHT: 00:00 - 04:00 ---
		if (hour < 4f)
			return 0.25f; // darkest

		// --- DAWN FADE: 04:00 - 06:00 ---
		if (hour < 6f)
			return Mathf.Lerp(0.25f, 0.6f, Mathf.InverseLerp(4f, 6f, hour));

		// --- MORNING BRIGHTENING: 06:00 - 12:00 ---
		if (hour < 12f)
			return Mathf.Lerp(0.6f, 1f, Mathf.InverseLerp(6f, 12f, hour));

		// --- FULL DAYLIGHT: 12:00 - 15:00 ---
		if (hour < 15f)
			return 1f;

		// --- EVENING FADE: 15:00 - 17:00 ---
		if (hour < 17f)
			return Mathf.Lerp(1f, 0.6f, Mathf.InverseLerp(15f, 17f, hour));

		// --- TWILIGHT TO NIGHT: 17:00 - 24:00 ---
		return Mathf.Lerp(0.6f, 0.25f, Mathf.InverseLerp(17f, 24f, hour));
	}
}
