using Godot;
using System;
using System.Collections.Generic;

public class Furniture
{
	public string Name { get; set; }
	public int Seats { get; set; }
	public int UnlockLevel { get; set; }
	public string Description { get; set; }

	public Furniture(string name, int seats, int unlockLevel, string description)
	{
		Name = name;
		Seats = seats;
		UnlockLevel = unlockLevel;
		Description = description;
	}
}

public static class FurnitureDatabase
{
	public static List<Furniture> AllFurniture = new()
	{
		new Furniture("Small Table", 4, 1, "Standard table, seats 4. Comes with the tavern."),
		new Furniture("Tiny Table", 2, 2, "Small corner table, seats 2."),
		new Furniture("Medium Table", 6, 6, "Crowd-favorite for group quests."),
		new Furniture("Large Table", 8, 8, "Room for entire raid groups.")
	};
}
