using Godot;
using System;
using System.Collections.Generic;


public partial class AdventurerGenerator : RefCounted
{
	private static Random random = new Random();

	// Split first names by gender:
	private static string[] MaleFirstNames = new string[] {
		"Aelric","Bran","Caelan","Darian","Faelar","Garen","Haleth","Jareth",
		"Kaelin","Noren","Orin","Quinn","Sylas","Ulric","Xavian","Zarek",
		"Ashen","Caius","Delia","Fenn","Hadrian","Jax","Leif","Matthias",
		"Niall","Perrin","Torin","Wyatt","Yorick","Ansel","Cedric","Eron",
		"Fray","Heron","Jude","Kellan","Phalen","Ronan","Solin","Urien",
		// …and so on…
	};

	private static string[] FemaleFirstNames = new string[] {
		"Elowen","Isolde","Lira","Maelis","Pyria","Rowan","Thalia","Vanya",
		"Wren","Ysolde","Zinnia","Arielle","Bryn","Cassian","Daelin","Eira",
		"Fiora","Gisela","Ilyana","Keira","Lucan","Mira","Ondine","Quilla",
		"Rhea","Seren","Una","Willa","Xara","Zara","Bellis","Darya","Gilda",
		"Ianthe","Liora","Nessa","Orla","Sarai","Veris","Wyn","Xenia","Yara",
		// …and so on…
	};

	private static string[] LastNames = new string[] {
		"Abernant","Briarwood","Caskwell","Duskbane","Evenbrook","Fallowmere",
		"Greymoor","Hawkwind","Ironridge","Jadeshade","Kilbride","Larkspur",
		"Mournvale","Nightriver","Oakmantle","Pinewatch","Quillstone",
		"Ravenhall","Stormcaller","Thornefield","Umbermoor","Vexley","Wyrmbane",
		"Xanthis","Yarrowell","Zephyrine","Ashguard","Blackbriar","Coldspring",
		"Dawnbreak","Eastmere","Frosthelm","Glimmershade","Hollowcrest",
		// …and so on…
	};

	// Pick a first name list based on gender:
	public static string GenerateName(Gender gender)
	{
		var firstList = gender == Gender.Male
			? MaleFirstNames
			: FemaleFirstNames;
		string first = firstList[random.Next(firstList.Length)];
		string last  = LastNames[random.Next(LastNames.Length)];
		return $"{first} {last}";
	}

	// Default name generator will choose a random gender internally:
	public static string GenerateName()
		=> GenerateName((Gender)random.Next(0, 2));

	public static int RandomizeStat(int baseValue)
		=> Math.Clamp(baseValue + random.Next(-1, 2), 1, 10);

	public static int RandomTrait() => random.Next(-50, 51);

	// New overload to generate an Adventurer WITH gender
	public static Adventurer GenerateAdventurer(int id, ClassTemplate template, Gender gender)
	{
		string name = GenerateName(gender);
		var adventurer = new Adventurer(
			id,
			name,
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

		adventurer.Template = template;
		// Make sure Adventurer has a Gender property:
		adventurer.Gender = gender;  

		return adventurer;
	}

	// (Optional) keep the old signature, delegating to the new one:
	public static Adventurer GenerateAdventurer(int id, ClassTemplate template)
	{
		var gender = (Gender)random.Next(0, 2);
		return GenerateAdventurer(id, template, gender);
	}
}
