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
	public int StrengthPerLevel;
	public int DexterityPerLevel;
	public int ConstitutionPerLevel;
	public int IntelligencePerLevel;


	public ClassTemplate(string name, int roleId,
	int str, int dex, int con, int intel,
	int strGain, int dexGain, int conGain, int intGain)
{
	ClassName = name;
	RoleId = roleId;

	Strength = str;
	Dexterity = dex;
	Constitution = con;
	Intelligence = intel;

	StrengthPerLevel = strGain;
	DexterityPerLevel = dexGain;
	ConstitutionPerLevel = conGain;
	IntelligencePerLevel = intGain;
}


	public static Dictionary<string, ClassTemplate> GetDefaultClassTemplates()
	{
		return new Dictionary<string, ClassTemplate>
{
	{ "Warrior",      new ClassTemplate("Warrior",      1, 8, 4, 8, 2, 1, 0, 1, 0) },
	{ "Paladin",      new ClassTemplate("Paladin",      1, 6, 5, 7, 4, 1, 0, 1, 0) },
	{ "Shadowknight", new ClassTemplate("Shadowknight", 1, 6, 4, 6, 6, 1, 0, 1, 0) },

	{ "Monk",         new ClassTemplate("Monk",         2, 7, 8, 5, 3, 0, 1, 1, 0) },
	{ "Rogue",        new ClassTemplate("Rogue",        2, 4, 8, 4, 4, 0, 2, 0, 0) },
	{ "Ranger",       new ClassTemplate("Ranger",       2, 5, 7, 5, 5, 1, 1, 0, 0) },

	{ "Wizard",       new ClassTemplate("Wizard",       3, 3, 4, 4, 9, 0, 0, 0, 2) },
	{ "Magician",     new ClassTemplate("Magician",     3, 3, 4, 4, 8, 0, 0, 0, 2) },
	{ "Necromancer",  new ClassTemplate("Necromancer",  3, 4, 4, 5, 8, 0, 0, 1, 1) },

	{ "Cleric",       new ClassTemplate("Cleric",       4, 4, 4, 7, 6, 0, 0, 1, 1) },
	{ "Druid",        new ClassTemplate("Druid",        4, 4, 5, 5, 7, 0, 1, 0, 1) },
	{ "Shaman",       new ClassTemplate("Shaman",       4, 4, 4, 6, 6, 0, 0, 1, 1) },

	{ "Enchanter",    new ClassTemplate("Enchanter",    5, 3, 6, 4, 8, 0, 0, 0, 2) },
	{ "Bard",         new ClassTemplate("Bard",         5, 5, 7, 5, 5, 0, 1, 0, 1) },
	{ "Beastlord",    new ClassTemplate("Beastlord",    5, 6, 6, 6, 4, 1, 0, 1, 0) }
};

	}
}
