using Godot;
using System;
using System.Collections.Generic;

public partial class AdventurerGenerator : RefCounted
{
	private static Random random = new Random();
	private static string[] FirstNames = new string[] {
		"Kael", "Tara", "Dorin", "Brynn", "Elias", "Nima", "Thalen", "Riona", "Serra", "Jalen"
		// ... (fill in from our list later)
	};

	private static string[] LastNames = new string[] {
		"Ironroot", "Thornveil", "Ashenmark", "Dawnblade", "Stormwatch", "Firebrand"
		// ... (fill in from our list later)
	};

	public static string GenerateName()
	{
		string first = FirstNames[random.Next(FirstNames.Length)];
		string last = LastNames[random.Next(LastNames.Length)];
		return first + " " + last;
	}

	public static int RandomizeStat(int baseValue)
	{
		return Math.Clamp(baseValue + random.Next(-1, 2), 1, 10);
	}

	public static int RandomTrait() => random.Next(-50, 51);

	public static Adventurer GenerateAdventurer(int id, ClassTemplate template)
	{
		return new Adventurer(
			id,
			GenerateName(),
			template.ClassName,
			template.RoleId,
			RandomizeStat(template.Strength),
			RandomizeStat(template.Dexterity),
			RandomizeStat(template.Constitution),
			RandomizeStat(template.Intelligence),
			RandomTrait(),
			RandomTrait(),
			RandomTrait(),
			RandomTrait()
		);
	}
}
