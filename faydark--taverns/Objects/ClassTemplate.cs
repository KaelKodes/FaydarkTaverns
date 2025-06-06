using Godot;
using System.Collections.Generic;

public partial class ClassTemplate : RefCounted
{
	public string ClassName;
	public int RoleId;
	public int Strength;
	public int Dexterity;
	public int Constitution;
	public int Intelligence;

	public ClassTemplate(string name, int role, int str, int dex, int con, int intel)
	{
		ClassName = name;
		RoleId = role;
		Strength = str;
		Dexterity = dex;
		Constitution = con;
		Intelligence = intel;
	}

	public static Dictionary<string, ClassTemplate> GetDefaultClassTemplates()
	{
		return new Dictionary<string, ClassTemplate>
		{
			{ "Warrior", new ClassTemplate("Warrior", 1, 8, 4, 8, 2) },
			{ "Paladin", new ClassTemplate("Paladin", 1, 6, 5, 7, 4) },
			{ "Shadowknight", new ClassTemplate("Shadowknight", 1, 6, 4, 6, 6) },
			{ "Monk", new ClassTemplate("Monk", 2, 7, 8, 5, 3) },
			{ "Rogue", new ClassTemplate("Rogue", 2, 4, 8, 4, 4) },
			{ "Ranger", new ClassTemplate("Ranger", 2, 5, 7, 5, 5) },
			{ "Wizard", new ClassTemplate("Wizard", 3, 3, 4, 4, 9) },
			{ "Magician", new ClassTemplate("Magician", 3, 3, 4, 4, 8) },
			{ "Necromancer", new ClassTemplate("Necromancer", 3, 4, 4, 5, 8) },
			{ "Cleric", new ClassTemplate("Cleric", 4, 4, 4, 7, 6) },
			{ "Druid", new ClassTemplate("Druid", 4, 4, 5, 5, 7) },
			{ "Shaman", new ClassTemplate("Shaman", 4, 4, 4, 6, 6) },
			{ "Enchanter", new ClassTemplate("Enchanter", 5, 3, 6, 4, 8) },
			{ "Bard", new ClassTemplate("Bard", 5, 5, 7, 5, 5) },
			{ "Beastlord", new ClassTemplate("Beastlord", 5, 6, 6, 6, 4) }
		};
	}
}
