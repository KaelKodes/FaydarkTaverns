using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FaydarkTaverns.Objects
{
	public class NPCData
	{
		private static readonly Random _levelRng = new Random();
		
		// Identity
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Name => $"{FirstName} {LastName}";
		public string Gender { get; set; }
		public int PortraitId { get; set; }
		public int VisitHour { get; set; } = -1; // -1 means not set
		public int VisitDay { get; set; } = -1;

		// --- Loyalty & Renown Reflection ---
		public float LoyaltyRating { get; set; } = 0.0f; // Builds with repeated visits
		public float MaxSpendMultiplier => 1.25f + (LoyaltyRating * 0.25f);

		// --- Food & Drink Preferences ---
		public string FavoriteFoodGroup { get; set; }
		public string HatedFoodGroup { get; set; }
		public string FavoriteDrinkGroup { get; set; }
		public string HatedDrinkGroup { get; set; }

		// --- Regional Affinities ---
		public Region BirthRegion { get; set; }
		public Region HatedRegion { get; set; }

		// --- Quest Affinities ---
		public QuestType FavoriteQuestType { get; set; }
		public QuestType HatedQuestType { get; set; }

		// --- Social Preferences ---
		public string FavoriteClass { get; set; }
		public string HatedClass { get; set; }

		// --- Tavern Triggers ---
		public bool IsHungry { get; set; } = false;
		public bool IsThirsty { get; set; } = false;
		public bool HasEatenToday { get; set; } = false;
		public bool HasDrankToday { get; set; } = false;



		// Role & State
		public NPCRole Role { get; set; }
		public NPCState State { get; set; }

		// Core Adventurer Stats (used if Role == Adventurer)
		public string ClassName { get; set; }
		public int Strength { get; set; }
		public int Dexterity { get; set; }
		public int Constitution { get; set; }
		public int Intelligence { get; set; }

		// Adventurer Personality & Behavior
		public int Aggression { get; set; }      // -50 to +50
		public int Distance { get; set; }
		public int HealingUse { get; set; }
		public int Focus { get; set; }

		// --- Adventurer Role Skill Stats ---
		public int Athletics { get; set; }
		public int Tracking { get; set; }
		public int LockPicking { get; set; }
		public int Buffing { get; set; }
		public int Debuffing { get; set; }
		public int Transport { get; set; }
		public int Taming { get; set; }
		public int SpellResearch { get; set; }
		public int Investigation { get; set; }
		public int Tank { get; set; }
		public int pDPS { get; set; }
		public int mDPS { get; set; }
		public int Healer { get; set; }


		// Adventurer Progression
		public int Level { get; set; } = 1;
		public int Xp { get; set; } = 0;
		public int XPToLevelUp => Level * 100;
		public int? AssignedQuestId { get; set; } = null;

		// Quest Giver-Specific Stats
		public bool HasPostedToday = false;
		public int QuestsPosted { get; set; } = 0;
		public int QuestsCompleted { get; set; } = 0;
		public int QuestsFailed { get; set; } = 0;

		public int GoldGiven { get; set; } = 0;
		public int ExpGiven { get; set; } = 0;
		public float Happiness { get; set; } = 0f; // -100 to +100, 0 is neutral: see GetMoodStatus

		public Quest ActiveQuest { get; set; } = null;
		public Quest PostedQuest { get; set; } = null;

		// Timers
		public float EntryPatience { get; set; }
		public float TavernLingerTime { get; set; }
		public float SeatRetryInterval { get; set; }
		public float SocializeDuration { get; set; }


		// Utility Methods
		public void GainXP(int amount)
		{
			Xp += amount;
			while (Xp >= XPToLevelUp)
			{
				Xp -= XPToLevelUp;
				Level++;
				GameLog.Info($"📈 {Role} {Name} leveled up to {Level}!");
			}
		}

		public string GetMoodStatus()
		{
			if (Happiness == 0) return "Neutral";
			return $"{(Happiness > 0 ? "+" : "")}{MathF.Round(Happiness)}";
		}
		public void AdjustHappiness(float amount)
		{
			float before = Happiness;
			Happiness = Mathf.Clamp(Happiness + amount, -100f, 100f);
			GameLog.Debug($"🧠 Happiness changed: {before} → {Happiness} ({(amount >= 0 ? "+" : "")}{amount})");
		}

		public void LevelUp(ClassTemplate template)
		{
			// 1) Bump level and Constitution
			Level++;
			Constitution++;

			// 2) Gather the other three stats and their class‐defined weights
			var weights = new List<(Action bump, int weight)> {
			(() => Strength++,        template.StrengthPerLevel),
			(() => Dexterity++,       template.DexterityPerLevel),
			(() => Intelligence++,    template.IntelligencePerLevel)
		};

			// 3) If all weights are zero, give them equal chance
			if (weights.All(w => w.weight <= 0))
				for (int i = 0; i < weights.Count; i++)
					weights[i] = (weights[i].bump, 1);

			// 4) Weighted random pick
			int totalWeight = weights.Sum(w => w.weight);
			int roll = _levelRng.Next(totalWeight);
			int cum = 0;
			foreach (var (bump, weight) in weights)
			{
				cum += weight;
				if (roll < cum)
				{
					bump();   // actually increase that stat
					break;
				}
			}

			// 5) (Optional) Recalculate derived stats if you have HP/Mana formulas:
			// MaxHP     = BaseHP  + Constitution * HP_PER_CON;
			// CurrentHP = MaxHP;
			// MaxMana   = BaseMana+ Intelligence * MANA_PER_INT;
			// CurrentMana = MaxMana;
		}
	


		// Combat-related methods (adventurers only)
		public int GetHp() => Constitution * 10;
		public int GetMana() => Intelligence * 10;
		public float GetDodge() => Dexterity * 1.5f;
		public float GetArmor(float baseArmor) => baseArmor + (Constitution * 0.5f);
		public float GetMeleeDamage() => Strength * 1.5f;
		public float GetMagicDamage() => Intelligence * 1.5f;
		public float GetSpeed() => Dexterity * 0.75f;
	}
	}

	public enum NPCRole
{
	Adventurer,
	Informant,
	Builder,
	QuestGiver
}
