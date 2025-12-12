using Godot;
using System;

public partial class ClockManager : Node
{
	public static ClockManager Instance { get; private set; }
	public static DateTime CurrentTime { get; private set; } = new DateTime(2025, 1, 1, 6, 0, 0); // Start at Jan 1, 2025, 6:00 AM
	public static float TimeMultiplier { get; private set; } = 1f; // 0 = paused, 1 = normal, etc.

	private const double GameSecondsPerRealSecond = 60.0; // 1s real = 60s game (1m)
	public event Action<DateTime> OnTimeAdvanced;

	private TimeSpan realTimeAccumulator = TimeSpan.Zero;
	private const double SecondsPerTick = 1.0; // how often we simulate 1 second in game time
	private int lastDay = -1;

	public static DateTime GameStartTime { get; private set; }
	public static int CurrentDay => (CurrentTime - GameStartTime).Days;
	private bool autosavedToday = false;

	public override void _Ready()
	{
		// Safe singleton guard — should generally only be one (autoload)
		if (Instance != null && Instance != this && GodotObject.IsInstanceValid(Instance))
		{
			GD.PrintErr("❌ Duplicate ClockManager instance detected! Keeping existing autoload instance.");
			return;
		}

		Instance = this;

		// Initialize GameStartTime only once if uninitialized
		if (GameStartTime == default)
			GameStartTime = CurrentTime;
	}

	public override void _Process(double delta)
	{
		if (TimeMultiplier == 0f)
			return;

		double scaledDelta = delta * TimeMultiplier * GameSecondsPerRealSecond;
		realTimeAccumulator += TimeSpan.FromSeconds(scaledDelta);

		while (realTimeAccumulator.TotalSeconds >= SecondsPerTick)
		{
			AdvanceTime(TimeSpan.FromSeconds(1));
			realTimeAccumulator -= TimeSpan.FromSeconds(1);
		}

		// AUTOSAVE TRIGGER
		if (CurrentTime.Hour == 6 && CurrentTime.Minute == 0)
		{
			if (!autosavedToday)
			{
				autosavedToday = true;
				SaveManager.SaveAuto(GameStateBuilder.BuildSaveData());
				GameLog.Info("💾 Autosave completed for new day.");
			}
		}
		else
		{
			autosavedToday = false; // resets after leaving 06:00
		}
	}

	private void AdvanceTime(TimeSpan amount)
	{
		CurrentTime += amount;

		OnTimeAdvanced?.Invoke(CurrentTime); // For C# event subscribers

		if (CurrentTime.Day != lastDay)
		{
			lastDay = CurrentTime.Day;
			GameLog.Debug($"🌞 New Day: {CurrentTime:D}");
			GameLog.Info($"🌞 New Day: {CurrentTime:D}");
			OnNewDay?.Invoke(CurrentTime);
		}
	}

	public static event Action<DateTime> OnNewDay;

	public static void SetTimeMultiplier(float multiplier)
	{
		TimeMultiplier = Mathf.Clamp(multiplier, 0f, 10f);
		GameLog.Debug($"⏱️ Time speed set to {TimeMultiplier}x");
		GameLog.Info($"⏱️ Time speed set to {TimeMultiplier}x");
	}

	public static string GetFormattedTime()
	{
		return CurrentTime.ToString("dddd, MMMM dd - HH:mm");
	}

	public static int GetCurrentHour() => CurrentTime.Hour;
	public static int GetCurrentMinute() => CurrentTime.Minute;
	public static int GetCurrentDay() => (CurrentTime - new DateTime(2025, 1, 1)).Days + 1;

	// =============================
	//  Save / Load
	// =============================
	public WorldData ToData()
	{
		return new WorldData
		{
			CurrentTimeISO = CurrentTime.ToString("o"),
			TimeMultiplier = TimeMultiplier,
			LastKnownDay = GetCurrentDay()
		};
	}

	public void FromData(WorldData data)
	{
		if (data == null)
			return;

		if (DateTime.TryParse(data.CurrentTimeISO, out var loadedTime))
			CurrentTime = loadedTime;

		TimeMultiplier = data.TimeMultiplier;

		// Reset accumulator so time doesn't jump
		realTimeAccumulator = TimeSpan.Zero;

		GameLog.Info($"⏱️ Time restored to: {CurrentTime}");
	}
}
