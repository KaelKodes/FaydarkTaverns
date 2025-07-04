using Godot;
using System;

public static class GuestLogic
{
	public static bool WantsToEnterTavern(Guest guest)
	{
		// 🔮 Future: Add AI logic here
		return true;
	}

	public static void EnterTavern(Guest guest)
{
	if (guest == null) return;
	guest.Admit();
}

}
