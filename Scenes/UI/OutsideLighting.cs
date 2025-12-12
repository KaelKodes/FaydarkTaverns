using Godot;
using System;

public partial class OutsideLighting : Node
{
	[Export] public TextureRect Outside;

	private ClockManager clock;

	public override void _Ready()
	{
		clock = GetNodeOrNull<ClockManager>("/root/ClockManager");
		if (clock != null && GodotObject.IsInstanceValid(clock))
		{
			clock.OnTimeAdvanced += UpdateLighting;
		}

		// Use whatever the current game time is (restored or fresh)
		UpdateLighting(ClockManager.CurrentTime);
	}

	public override void _ExitTree()
	{
		// Unsubscribe so disposed instances don't keep receiving events
		if (clock != null && GodotObject.IsInstanceValid(clock))
		{
			clock.OnTimeAdvanced -= UpdateLighting;
		}
	}

	private void UpdateLighting(DateTime t)
	{
		// 🔒 Hard guard against disposed TextureRect
		if (Outside == null || !GodotObject.IsInstanceValid(Outside))
		{
			GD.PrintErr("[OutsideLighting] Outside TextureRect is invalid; skipping UpdateLighting.");
			return;
		}

		float hour = t.Hour + t.Minute / 60f;
		float brightness = CalculateBrightness(hour);

		// DO NOT clamp as high — let it go much darker.
		brightness = Mathf.Max(brightness, 0.06f);

		Outside.Modulate = new Color(brightness, brightness, brightness, 1f);
	}

	private float CalculateBrightness(float hour)
	{
		// DARKEST NIGHT: 00:00 - 04:00 (flat minimum)
		if (hour < 4f)
			return 0.06f; // Very dark, but not black

		// DAWN FADE: 04:00 - 06:00
		if (hour < 6f)
			return Mathf.Lerp(0.06f, 0.6f, Mathf.InverseLerp(4f, 6f, hour));

		// MORNING: 06:00 - 12:00
		if (hour < 12f)
			return Mathf.Lerp(0.6f, 1f, Mathf.InverseLerp(6f, 12f, hour));

		// FULL DAYLIGHT: 12:00 - 15:00
		if (hour < 15f)
			return 1f;

		// PRE-EVENING FADE: 15:00 - 17:00
		if (hour < 17f)
			return Mathf.Lerp(1f, 0.6f, Mathf.InverseLerp(15f, 17f, hour));

		// SUNSET → NIGHT: 17:00 - 24:00
		return Mathf.Lerp(0.6f, 0.06f, Mathf.InverseLerp(17f, 24f, hour));
	}
}
