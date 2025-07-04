using Godot;
using System;

public class Guest
{
	public string Name;
	public bool IsAdventurer;

	public int VisitDay;
	public int VisitHour;
	public int WaitDuration;   // How long they'll wait outside
	public int StayDuration;   // How long they stay once inside

	public bool IsInside = false;
	public int? SeatIndex { get; set; } = null;
	public Table AssignedTable { get; set; } = null;

	public Adventurer BoundAdventurer { get; set; }
	public DateTime LastSeatCheck = DateTime.MinValue;
	public DateTime? DepartureTime { get; set; } // Nullable

	public event Action OnAdmitted;

	
	/// Call this method when the guest is admitted to the tavern.
	public void Admit()
	{
		IsInside = true;
		DepartureTime = ClockManager.CurrentTime.AddHours(StayDuration);
		OnAdmitted?.Invoke();
	}
}
