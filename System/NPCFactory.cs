using System;
using FaydarkTaverns.Objects;

public static class NPCFactory
{
	private static Random random = new Random();

	private static string[] MaleFirstNames = new string[] {
	"Aelric","Bran","Caelan","Darian","Faelar","Garen","Haleth","Jareth",
	"Kaelin","Noren","Orin","Quinn","Sylas","Ulric","Xavian","Zarek",
	"Ashen","Caius","Delia","Fenn","Hadrian","Jax","Leif","Matthias",
	"Niall","Perrin","Torin","Wyatt","Yorick","Ansel","Cedric","Eron",
	"Fray","Heron","Jude","Kellan","Phalen","Ronan","Solin","Urien"
};


	private static string[] FemaleFirstNames = new string[] {
	"Elowen","Isolde","Lira","Maelis","Pyria","Rowan","Thalia","Vanya",
	"Wren","Ysolde","Zinnia","Arielle","Bryn","Cassian","Daelin","Eira",
	"Fiora","Gisela","Ilyana","Keira","Lucan","Mira","Ondine","Quilla",
	"Rhea","Seren","Una","Willa","Xara","Zara","Bellis","Darya","Gilda",
	"Ianthe","Liora","Nessa","Orla","Sarai","Veris","Wyn","Xenia","Yara"
};

	private static string[] LastNames = new string[] {
	"Abernant","Briarwood","Caskwell","Duskbane","Evenbrook","Fallowmere",
	"Greymoor","Hawkwind","Ironridge","Jadeshade","Kilbride","Larkspur",
	"Mournvale","Nightriver","Oakmantle","Pinewatch","Quillstone",
	"Ravenhall","Stormcaller","Thornefield","Umbermoor","Vexley","Wyrmbane",
	"Xanthis","Yarrowell","Zephyrine","Ashguard","Blackbriar","Coldspring",
	"Dawnbreak","Eastmere","Frosthelm","Glimmershade","Hollowcrest"
};


	public static NPCData CreateBaseNPC()
{
	string gender = GetRandomGender();
	string first = GenerateFirstName(gender);
	string last = GenerateLastName();

	var npc = new NPCData
	{
		Id = Guid.NewGuid().ToString(),
		Gender = gender,
		FirstName = first,
		LastName = last,
		State = NPCState.Elsewhere,
		Level = 1,
		Xp = 0
	};
	return npc;
}


	public static void AssignAdventurerStats(NPCData npc, ClassTemplate template)
	{
		npc.ClassName = template.ClassName;

		npc.Strength = RandomizeStat(template.Strength);
		npc.Dexterity = RandomizeStat(template.Dexterity);
		npc.Constitution = RandomizeStat(template.Constitution);
		npc.Intelligence = RandomizeStat(template.Intelligence);

		npc.Aggression = RandomTrait();
		npc.Distance = RandomTrait();
		npc.HealingUse = RandomTrait();
		npc.Focus = RandomTrait();

		npc.Level = 1;
		npc.Xp = 0;
	}

	public static void AssignInformantStats(NPCData npc)
	{
		npc.ClassName = "Informant";
		npc.Intelligence = 8;
		npc.Happiness = 0;
	}
	public static void AssignPortrait(NPCData npc)
{
	if (npc.Role == NPCRole.QuestGiver)
	{
		npc.PortraitId = npc.Gender == "Male"
			? random.Next(1, 4)
			: 1;
	}
	else
	{
		npc.PortraitId = random.Next(1, 3);
	}
}


	private static string GetRandomGender()
	{
		return random.Next(0, 2) == 0 ? "Male" : "Female";
	}

	private static string GenerateFirstName(string gender)
	{
		return gender == "Male"
			? MaleFirstNames[random.Next(MaleFirstNames.Length)]
			: FemaleFirstNames[random.Next(FemaleFirstNames.Length)];
	}

	private static string GenerateLastName()
	{
		return LastNames[random.Next(LastNames.Length)];
	}

	private static int RandomizeStat(int baseValue)
	{
		return Math.Clamp(baseValue + random.Next(-1, 2), 1, 10);
	}

	private static int RandomTrait()
	{
		return random.Next(-50, 51);
	}
}
