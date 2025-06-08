using Godot;
using System;
using System.Collections.Generic;

public partial class AdventurerGenerator : RefCounted
{
	private static Random random = new Random();
	private static string[] FirstNames = new string[] {
		"Aelric","Bran","Caelan","Darian","Elowen","Faelar","Garen","Haleth","Isolde","Jareth",
"Kaelin","Lira","Maelis","Noren","Orin","Pyria","Quinn","Rowan","Sylas","Thalia",
"Ulric","Vanya","Wren","Xavian","Ysolde","Zarek","Arielle","Bryn","Cassian","Daelin",
"Eira","Fiora","Galen","Hale","Ilyana","Jorin","Keira","Lucan","Mira","Niall",
"Ondine","Perrin","Quilla","Riven","Seren","Talon","Una","Varek","Willa","Xara",
"Yorick","Zinnia","Ashen","Briala","Caius","Delia","Elandra","Fenn","Gisela","Hadrian",
"Isera","Jax","Kira","Leif","Melis","Nyra","Olen","Petra","Quen","Rhea",
"Solin","Tessa","Urien","Vesper","Wyatt","Xenia","Yara","Zephyr","Ansel","Bellis",
"Cedric","Darya","Eron","Fray","Gilda","Heron","Ianthe","Jude","Kellan","Liora",
"Matthias","Nessa","Orla","Phalen","Ronan","Sarai","Torin","Veris","Wyn","Zara"
// ... (fill in from our list later)
	};

	private static string[] LastNames = new string[] {
		"Abernant","Briarwood","Caskwell","Duskbane","Evenbrook","Fallowmere","Greymoor","Hawkwind","Ironridge","Jadeshade",
"Kilbride","Larkspur","Mournvale","Nightriver","Oakmantle","Pinewatch","Quillstone","Ravenhall","Stormcaller","Thornefield",
"Umbermoor","Vexley","Wyrmbane","Xanthis","Yarrowell","Zephyrine","Ashguard","Blackbriar","Coldspring","Dawnbreak",
"Eastmere","Frosthelm","Glimmershade","Hollowcrest","Inkwell","Jadefang","Kingsworn","Lightmere","Moonshadow","Northwind",
"Oldridge","Palerose","Quickwater","Redbrook","Silverbranch","Tarnvale","Ulthain","Valeborn","Windrow","Xorlan",
"Youngblood","Zalreth","Amberhall","Brightspire","Cresthaven","Dewharrow","Elmsend","Fernbrooke","Goldbranch","Hollowind",
"Ironsoul","Juneblade","Keenvale","Lanebrook","Marrowfall","Nightbloom","Oakenforge","Pendrake","Quartzfell","Runebridge",
"Shadeveil","Tanglehollow","Underbranch","Violetmoor","Wolfsbane","Xelborn","Yewshade","Zarovin","Arkwright","Barrowmere",
"Chasmire","Draymoor","Emberbrook","Foxglade","Grimthorne","Hearthfell","Icewind","Jettford","Kraventh","Lilystone",
"Mournshade","Netheridge","Orrindale","Petalshade","Quickthorn","Ridgebane","Stonecroft","Thistlewood","Umberglen","Virethorn"
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
