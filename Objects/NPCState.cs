namespace FaydarkTaverns.Objects
{
	public enum NPCState
	{
		Elsewhere,         // Outside the game world (resting or returning)
		StreetOutside,     // Waiting to enter tavern
		StagingArea,       // Waiting to be assigned to a quest
		TavernFloor,       // Actively roaming the tavern
		Seats,             // Seated and engaging
		Lodging,           // Resting in the tavern's rooms
		AssignedToQuest,   // Currently on a quest
		Departed           // Removed or permanently left
	}
}
