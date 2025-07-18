using Godot;
using System.Collections.Generic;

public partial class ClassTemplate : RefCounted
{
	public string ClassName;
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



	public ClassTemplate(string name,
int str, int dex, int con, int intel,
int strGain, int dexGain, int conGain, int intGain,
int athletics, int tracking, int lockPicking, int buffing, int debuffing,
int transport, int taming, int spellResearch, int investigation,
int tank, int pDPS, int mDPS, int healer)

{
	ClassName = name;

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
this.pDPS = pDPS;
this.mDPS = mDPS;
Healer = healer;

}

	// These are the base stats for each class. Yes, it looks gross. However there is a spreadsheet available that makes this much easier to read and each stat is defined above!
public static Dictionary<string, ClassTemplate> GetDefaultClassTemplates()
{
	return new Dictionary<string, ClassTemplate>
	{
		{ "Bard",         new ClassTemplate("Bard",         5,6,5,5,   5,6,1,5,   4,0,2,5,4,5,3,0,3,   5,5,5,5) },
		{ "Beastlord",    new ClassTemplate("Beastlord",    6,6,6,4,   6,6,1,4,   4,4,0,4,3,3,7,2,2,   4,5,3,4) },
		{ "Cleric",       new ClassTemplate("Cleric",       4,4,7,6,   4,4,1,7,   2,0,0,9,1,0,3,6,4,   5,2,3,9) },
		{ "Druid",        new ClassTemplate("Druid",        4,5,5,7,   4,5,1,5,   2,9,0,6,1,9,5,0,0,   1,0,5,8) },
		{ "Enchanter",    new ClassTemplate("Enchanter",    3,6,4,8,   3,6,1,4,   1,0,2,8,4,1,8,5,7,   2,1,7,0) },
		{ "Magician",     new ClassTemplate("Magician",     3,4,5,8,   3,4,1,4,   3,0,1,4,5,3,0,8,6,   5,1,8,3) },
		{ "Monk",         new ClassTemplate("Monk",         7,8,5,3,   7,8,1,5,   9,2,2,0,3,2,3,1,2,   5,8,1,2) },
		{ "Necromancer",  new ClassTemplate("Necromancer",  4,4,5,8,   4,4,1,8,   1,3,0,1,9,1,1,7,5,   2,1,8,2) },
		{ "Paladin",      new ClassTemplate("Paladin",      6,5,7,4,   6,5,1,4,   7,0,0,4,2,0,3,2,3,   8,6,2,4) },
		{ "Ranger",       new ClassTemplate("Ranger",       4,7,5,5,   5,7,1,5,   8,9,1,0,1,0,6,0,6,   6,8,0,0) },
		{ "Rogue",        new ClassTemplate("Rogue",        4,8,4,4,   4,8,1,4,   8,2,9,0,3,1,0,0,8,   3,9,1,0) },
		{ "Shadowknight", new ClassTemplate("Shadowknight", 6,4,6,6,   6,4,1,6,   6,0,0,0,5,1,0,3,3,   7,6,6,3) },
		{ "Shaman",       new ClassTemplate("Shaman",       4,4,6,6,   4,4,1,6,   3,1,0,6,4,5,7,1,0,   3,1,5,8) },
		{ "Warrior",      new ClassTemplate("Warrior",      8,5,9,2,   8,4,1,2,   9,3,0,0,0,5,0,0,4,   9,9,1,0) },
		{ "Wizard",       new ClassTemplate("Wizard",       3,4,4,9,   3,4,1,9,   1,0,0,0,7,9,0,9,5,   1,1,9,0) }
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


}
