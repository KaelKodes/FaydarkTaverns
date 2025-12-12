using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class TimerManager : Node
{
	public static TimerManager Instance { get; private set; }

	private List<ScheduledEvent> eventQueue = new();

	public override void _Ready()
	{
		// FIXED SINGLETON LOGIC
		if (Instance != null && Instance != this && GodotObject.IsInstanceValid(Instance))
		{
			GD.PrintErr("❌ Duplicate TimerManager instance! Replacing old instance.");
		}

		Instance = this;

		var clock = GetNode<ClockManager>("/root/ClockManager");
		clock.OnTimeAdvanced += CheckEvents;
	}

	public override void _ExitTree()
	{
		if (Instance == this)
			Instance = null;
	}


	// ★ CORRECT PLACE FOR WaitSeconds ★
	public static async Task WaitSeconds(double seconds)
	{
		var sceneTree = Engine.GetMainLoop() as SceneTree;

		if (sceneTree == null)
		{
			GD.PrintErr("TimerManager.WaitSeconds: SceneTree unavailable!");
			return;
		}

		var timer = sceneTree.CreateTimer(seconds);
		await timer.ToSignal(timer, Timer.SignalName.Timeout);
	}

	// Scheduling system (unchanged)
	public void ScheduleEvent(DateTime triggerTime, Action callback)
	{
		eventQueue.Add(new ScheduledEvent(triggerTime, callback));
		eventQueue.Sort((a, b) => a.TriggerTime.CompareTo(b.TriggerTime));
	}

	private void CheckEvents(DateTime currentTime)
	{
		if (GameStateLoader.IsRestoring)
			return;
		List<ScheduledEvent> toFire = new();

		foreach (var evt in eventQueue)
		{
			if (evt.TriggerTime <= currentTime)
				toFire.Add(evt);
			else
				break;
		}

		foreach (var evt in toFire)
		{
			evt.Callback?.Invoke();
			eventQueue.Remove(evt);
		}
	}
}

public class ScheduledEvent
{
	public DateTime TriggerTime { get; }
	public Action Callback { get; }

	public ScheduledEvent(DateTime triggerTime, Action callback)
	{
		TriggerTime = triggerTime;
		Callback = callback;
	}
}
