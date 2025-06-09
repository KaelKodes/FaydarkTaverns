using Godot;
using System;

public partial class Adventurer : RefCounted
{
	public int Id;
	public string Name;
	public string ClassName;
	public int RoleId;

	public int Strength;
	public int Dexterity;
	public int Constitution;
	public int Intelligence;

	public int Aggression;     // -50 to +50
	public int Distance;
	public int HealingUse;
	public int Focus;

	public int Level = 1;
	public int Xp = 0;
	public string Status = "Idle";
	public int? AssignedQuestId { get; set; } = null;

	public int XPToLevelUp => Level * 100;

	public Adventurer(int id, string name, string className, int roleId, int str, int dex, int con, int intel,
				  int aggression, int distance, int healingUse, int focus)
	{
		Id = id;
		Name = name;
		ClassName = className;
		RoleId = roleId;
		Strength = str;
		Dexterity = dex;
		Constitution = con;
		Intelligence = intel;
		Aggression = aggression;
		Distance = distance;
		HealingUse = healingUse;
		Focus = focus;
	}

	public int GetHp() => Constitution * 10;
	public int GetMana() => Intelligence * 10;
	public float GetDodge() => Dexterity * 1.5f;
	public float GetArmor(float baseClassArmor) => baseClassArmor + (Constitution * 0.5f);
	public float GetMeleeDamage() => Strength * 1.5f;
	public float GetMagicDamage() => Intelligence * 1.5f;
	public float GetSpeed() => Dexterity * 0.75f;

	public void GainXP(int amount)
	{
		Xp += amount;
		while (Xp >= XPToLevelUp)
		{
			Xp -= XPToLevelUp;
			Level++;
			GD.Print($"⭐ {Name} leveled up to Level {Level}!");
			// Optional: Stat improvements or events
		}
	}
} 
