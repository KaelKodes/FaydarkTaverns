using Godot;
using System.Text;
using FaydarkTaverns.Objects;

/// <summary>
/// Popup window showing full NPC details for debugging/testing.
/// </summary>
public partial class NPCDetailPopup : Window
{
	// --- Exports: hook these up in the .tscn ---
	[Export] public Label NameLabel;
	[Export] public Label GenderLabel;
	[Export] public Label VisitHourLabel;
	[Export] public Label VisitDayLabel;
	[Export] public Label LoyaltyRatingLabel;
	[Export] public Label MaxSpendMultiplierLabel;
	[Export] public Label FavoriteFoodGroupLabel;
	[Export] public Label HatedFoodGroupLabel;
	[Export] public Label FavoriteDrinkGroupLabel;
	[Export] public Label HatedDrinkGroupLabel;
	[Export] public Label BirthRegionLabel;
	[Export] public Label HatedRegionLabel;
	[Export] public Label FavoriteQuestTypeLabel;
	[Export] public Label HatedQuestTypeLabel;
	[Export] public Label FavoriteClassLabel;
	[Export] public Label HatedClassLabel;
	[Export] public Label IsHungryLabel;
	[Export] public Label IsThirstyLabel;
	[Export] public Label HasEatenTodayLabel;
	[Export] public Label HasDrankTodayLabel;
	[Export] public Label RoleLabel;
	[Export] public Label StateLabel;
	[Export] public Label ClassNameLabel;
	[Export] public Label StrengthLabel;
	[Export] public Label DexterityLabel;
	[Export] public Label ConstitutionLabel;
	[Export] public Label IntelligenceLabel;
	[Export] public Label AggressionLabel;
	[Export] public Label DistanceLabel;
	[Export] public Label HealingUseLabel;
	[Export] public Label FocusLabel;
	[Export] public Label AthleticsLabel;
	[Export] public Label TrackingLabel;
	[Export] public Label LockPickingLabel;
	[Export] public Label BuffingLabel;
	[Export] public Label DebuffingLabel;
	[Export] public Label TransportLabel;
	[Export] public Label TamingLabel;
	[Export] public Label SpellResearchLabel;
	[Export] public Label InvestigationLabel;
	[Export] public Label TankLabel;
	[Export] public Label PhysicalDPSLabel;
	[Export] public Label MagicDPSLabel;
	[Export] public Label HealerLabel;
	[Export] public Label LevelLabel;
	[Export] public Label XpLabel;
	[Export] public Label XPToLevelUpLabel;
	[Export] public Label AssignedQuestIdLabel;
	[Export] public Label HasPostedTodayLabel;
	[Export] public Label QuestsPostedLabel;
	[Export] public Label QuestsCompletedLabel;
	[Export] public Label QuestsFailedLabel;
	[Export] public Label GoldGivenLabel;
	[Export] public Label ExpGivenLabel;
	[Export] public Label HappinessLabel;
	[Export] public Label ActiveQuestLabel;
	[Export] public Label PostedQuestLabel;
	[Export] public Label EntryPatienceLabel;
	[Export] public Label TavernLingerTimeLabel;
	[Export] public Label SeatRetryIntervalLabel;
	[Export] public Label SocializeDurationLabel;
	[Export] public Label HealthLabel;
	[Export] public Label ManaLabel;
	[Export] public TextureRect PortraitRect;


	public override void _Ready()
	{
		CloseRequested += Hide;
	}

	/// <summary>
	/// Populate all fields from NPCData for full visibility.
	/// </summary>
	public void SetNPC(NPCData npc)
	{
		NameLabel.Text                 = npc.Name;
		GenderLabel.Text               = $"Gender: {npc.Gender}";
		VisitHourLabel.Text            = $"Visit Hour: {(npc.VisitHour >= 0 ? npc.VisitHour.ToString() : "N/A")}";
		VisitDayLabel.Text             = $"Visit Day: {(npc.VisitDay >= 0 ? npc.VisitDay.ToString() : "N/A")}";
		LoyaltyRatingLabel.Text        = $"Loyalty: {npc.LoyaltyRating:F2}";
		MaxSpendMultiplierLabel.Text   = $"Max Spend Mult.: {npc.MaxSpendMultiplier:F2}";
		FavoriteFoodGroupLabel.Text    = $"Fav Food: {npc.FavoriteFoodGroup}";
		HatedFoodGroupLabel.Text       = $"Hated Food: {npc.HatedFoodGroup}";
		FavoriteDrinkGroupLabel.Text   = $"Fav Drink: {npc.FavoriteDrinkGroup}";
		HatedDrinkGroupLabel.Text      = $"Hated Drink: {npc.HatedDrinkGroup}";
		BirthRegionLabel.Text          = $"Birth Region: {npc.BirthRegion}";
		HatedRegionLabel.Text          = $"Hated Region: {npc.HatedRegion}";
		FavoriteQuestTypeLabel.Text    = $"Fav Quest: {npc.FavoriteQuestType}";
		HatedQuestTypeLabel.Text       = $"Hated Quest: {npc.HatedQuestType}";
		FavoriteClassLabel.Text        = $"Fav Class: {npc.FavoriteClass}";
		HatedClassLabel.Text           = $"Hated Class: {npc.HatedClass}";
		IsHungryLabel.Text             = $"Hungry: {npc.IsHungry}";
		IsThirstyLabel.Text            = $"Thirsty: {npc.IsThirsty}";
		HasEatenTodayLabel.Text        = $"Has Eaten Today: {npc.HasEatenToday}";
		HasDrankTodayLabel.Text        = $"Has Drank Today: {npc.HasDrankToday}";
		RoleLabel.Text                 = $"Role: {npc.Role}";
		StateLabel.Text                = $"State: {npc.State}";
		ClassNameLabel.Text            = $"Class: {npc.ClassName}";
		StrengthLabel.Text             = $"Strength: {npc.Strength}";
		DexterityLabel.Text            = $"Dexterity: {npc.Dexterity}";
		ConstitutionLabel.Text         = $"Constitution: {npc.Constitution}";
		IntelligenceLabel.Text         = $"Intelligence: {npc.Intelligence}";
		AggressionLabel.Text           = $"Aggression: {npc.Aggression}";
		DistanceLabel.Text             = $"Distance: {npc.Distance}";
		HealingUseLabel.Text           = $"Healing Use: {npc.HealingUse}";
		FocusLabel.Text                = $"Focus: {npc.Focus}";
		AthleticsLabel.Text            = $"Athletics: {npc.Athletics}";
		TrackingLabel.Text             = $"Tracking: {npc.Tracking}";
		LockPickingLabel.Text          = $"LockPicking: {npc.LockPicking}";
		BuffingLabel.Text              = $"Buffing: {npc.Buffing}";
		DebuffingLabel.Text            = $"Debuffing: {npc.Debuffing}";
		TransportLabel.Text            = $"Transport: {npc.Transport}";
		TamingLabel.Text               = $"Taming: {npc.Taming}";
		SpellResearchLabel.Text        = $"SpellResearch: {npc.SpellResearch}";
		InvestigationLabel.Text        = $"Investigation: {npc.Investigation}";
		TankLabel.Text                 = $"Tank: {npc.Tank}";
		PhysicalDPSLabel.Text          = $"pDPS: {npc.pDPS}";
		MagicDPSLabel.Text             = $"mDPS: {npc.mDPS}";
		HealerLabel.Text               = $"Healer: {npc.Healer}";
		LevelLabel.Text                = $"Level: {npc.Level}";
		XpLabel.Text                   = $"XP: {npc.Xp}/{npc.XPToLevelUp}";
		XPToLevelUpLabel.Text          = $"XP to Level: {npc.XPToLevelUp}";
		AssignedQuestIdLabel.Text      = $"Assigned Quest: {(npc.AssignedQuestId.HasValue ? npc.AssignedQuestId.Value.ToString() : "None")}";
		HasPostedTodayLabel.Text       = $"Posted Today: {npc.HasPostedToday}";
		QuestsPostedLabel.Text         = $"Quests Posted: {npc.QuestsPosted}";
		QuestsCompletedLabel.Text      = $"Quests Completed: {npc.QuestsCompleted}";
		QuestsFailedLabel.Text         = $"Quests Failed: {npc.QuestsFailed}";
		GoldGivenLabel.Text            = $"Gold Given: {npc.GoldGiven}";
		ExpGivenLabel.Text             = $"Exp Given: {npc.ExpGiven}";
		HappinessLabel.Text            = $"Happiness: {npc.Happiness:F2}";
		ActiveQuestLabel.Text          = $"Active Quest ID: {(npc.ActiveQuest != null ? npc.ActiveQuest.QuestId.ToString() : "None")}";
		PostedQuestLabel.Text          = $"Posted Quest ID: {(npc.PostedQuest != null ? npc.PostedQuest.QuestId.ToString() : "None")}";
		EntryPatienceLabel.Text        = $"Entry Patience: {npc.EntryPatience:F2}";
		TavernLingerTimeLabel.Text     = $"Linger Time: {npc.TavernLingerTime:F2}";
		SeatRetryIntervalLabel.Text    = $"Seat Retry: {npc.SeatRetryInterval:F2}";
		SocializeDurationLabel.Text    = $"Socialize Duration: {npc.SocializeDuration:F2}";
		HealthLabel.Text   = $"HP: {npc.GetHp()}";
		ManaLabel.Text = $"Mana: {npc.GetMana()}";;

		
// Portrait
// Build path from ClassName, gender initial, and PortraitId
string genderInitial = npc.Gender == "Male" ? "M" : "F";
string className     = npc.ClassName;
int portraitId       = npc.PortraitId;
string assetPath     = $"res://Assets/UI/ClassPortraits/{className}/{className}{genderInitial}{portraitId}.jpg";

var tex = ResourceLoader.Load<Texture2D>(assetPath);
if (tex != null)
	PortraitRect.Texture = tex;
else
	GD.PrintErr($"Portrait not found at {assetPath}");



	}
}
