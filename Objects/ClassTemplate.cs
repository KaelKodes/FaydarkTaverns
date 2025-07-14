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
	// --- Role Skill Stats ---
public int Athletics;
public int Tracking;
public int LockPicking;
public int Buffing;
public int Debuffing;
public int Transport;
public int Taming;
public int SpellResearch;
public int Investigation;

// --- Classic Combat Role Tags ---
public int Tank;
public int pDPS;
public int mDPS;
public int Healer;



	public ClassTemplate(string name, int roleId,
int str, int dex, int con, int intel,
int strGain, int dexGain, int conGain, int intGain,
int athletics, int tracking, int lockPicking, int buffing, int debuffing,
int transport, int taming, int spellResearch, int investigation,
int tank, int pDPS, int mDPS, int healer)

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
	
	Athletics = athletics;
Tracking = tracking;
LockPicking = lockPicking;
Buffing = buffing;
Debuffing = debuffing;
Transport = transport;
Taming = taming;
SpellResearch = spellResearch;
Investigation = investigation;

Tank = tank;
pDPS = pDPS;
mDPS = mDPS;
Healer = healer;

}

	// These are the base stats for each class. Yes, it looks gross. However there is a spreadsheet available that makes this much easier to read and each stat is defined above!
	public static Dictionary<string, ClassTemplate> GetDefaultClassTemplates()
	{
		return new Dictionary<string, ClassTemplate>
{
	{ "Warrior", new ClassTemplate("Warrior", 1, 0, 0, 0, 0, 0, 0, 0, 0, 9, 3, 0, 0, 0, 5, 0, 0, 4, 9, 9, 1, 0) },
	{ "Monk", new ClassTemplate("Monk", 1, 0, 0, 0, 0, 0, 0, 0, 0, 9, 2, 2, 0, 3, 2, 3, 1, 2, 5, 8, 1, 2) },
	{ "Shadowknight", new ClassTemplate("Shadowknight", 1, 0, 0, 0, 0, 0, 0, 0, 0, 6, 0, 0, 0, 5, 1, 0, 3, 3, 7, 6, 6, 3) },
	{ "Paladin", new ClassTemplate("Paladin", 1, 0, 0, 0, 0, 0, 0, 0, 0, 7, 0, 0, 4, 2, 0, 2, 2, 3, 8, 6, 2, 4) },
	{ "Necromancer", new ClassTemplate("Necromancer", 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 1, 9, 1, 1, 7, 5, 2, 1, 8, 2) },
	{ "Magician", new ClassTemplate("Magician", 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 1, 4, 4, 3, 0, 8, 5, 3, 1, 8, 3) },
	{ "Wizard", new ClassTemplate("Wizard", 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 7, 9, 0, 9, 5, 1, 1, 9, 0) },
	{ "Rogue", new ClassTemplate("Rogue", 1, 0, 0, 0, 0, 0, 0, 0, 0, 8, 2, 9, 0, 3, 1, 0, 0, 8, 3, 9, 1, 0) },
	{ "Cleric", new ClassTemplate("Cleric", 1, 0, 0, 0, 0, 0, 0, 0, 0, 2, 0, 0, 9, 1, 0, 3, 6, 4, 5, 2, 3, 9) },
	{ "Beastlord", new ClassTemplate("Beastlord", 1, 0, 0, 0, 0, 0, 0, 0, 0, 4, 4, 0, 4, 4, 3, 7, 2, 2, 4, 5, 3, 4) },
	{ "Enchanter", new ClassTemplate("Enchanter", 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 2, 8, 4, 1, 8, 5, 7, 2, 1, 7, 0) },
	{ "Shaman", new ClassTemplate("Shaman", 1, 0, 0, 0, 0, 0, 0, 0, 0, 3, 1, 0, 6, 3, 5, 7, 1, 0, 3, 1, 5, 8) },
	{ "Druid", new ClassTemplate("Druid", 1, 0, 0, 0, 0, 0, 0, 0, 0, 2, 9, 0, 6, 1, 9, 5, 0, 0, 1, 0, 5, 8) },
	{ "Bard", new ClassTemplate("Bard", 1, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 2, 5, 4, 5, 3, 0, 3, 5, 5, 5, 5) },
	{ "Ranger", new ClassTemplate("Ranger", 1, 0, 0, 0, 0, 0, 0, 0, 0, 8, 9, 1, 0, 1, 0, 6, 0, 7, 6, 8, 0, 0) }
};

	}
	
	public static List<string> GetAllClassNames()
{
	var templates = GetDefaultClassTemplates();
	return new List<string>(templates.Keys);
}
public static ClassTemplate GetTemplateByName(string className)
{
	var templates = GetDefaultClassTemplates();
	if (templates.ContainsKey(className))
		return templates[className];

	GameLog.Debug($"❌ ClassTemplate not found for class '{className}'.");
	return null;
}
public static int GetRoleIdFromClass(string className)
{
	return className switch
	{
		"Warrior" => 1,
		"Mage" => 2,
		"Rogue" => 3,
		"Healer" => 4,
		_ => 0
	};
}


}
