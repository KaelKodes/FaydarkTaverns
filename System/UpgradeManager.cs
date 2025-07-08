using Godot;
using System;
using System.Collections.Generic;

public partial class UpgradeManager : Node
{
	public static UpgradeManager Instance { get; private set; }

	public override void _Ready()
	{
		if (Instance != null)
		{
			GD.PrintErr("❌ Multiple UpgradeManager instances detected!");
			QueueFree();
			return;
		}
		Instance = this;
	}

	public void ApplyUpgrade(ShopItem item)
	{
		switch (item.Name)
		{
			// 🪑 Repeatable: Increase Floor Cap
			case "Increase Floor Cap":
				TavernStats.Instance.MaxFloorGuests++;
				IncrementUpgrade("FloorCapUpgradeCount");
				GameLog.Info("🏗️ Floor cap increased by 1.");
				break;

			// 📜 Repeatable: Increase Quest Cap
			case "Increase Quest Cap":
				QuestManager.Instance.IncreaseQuestLimit();
				IncrementUpgrade("QuestBoardUpgradeCount");
				GameLog.Info("📜 Quest board capacity increased by 1.");
				break;

			// 📣 One-Time: Upgrade Tavern Sign
			case "Upgrade Tavern Sign":
				TavernStats.Instance.TavernSignLevel++;
				TavernStats.Instance.AddRenown(15);
				GameLog.Info("📣 The tavern sign was upgraded. +15 renown!");
				break;

			// 🔓 Unlocks
			case "Unlock Oven":
				TavernStats.Instance.UnlockedUpgrades.Add("Unlocked_Oven");
				GameLog.Info("🔥 Oven unlocked. You can now sell food!");
				break;

			case "Unlock Keg Stand":
				TavernStats.Instance.UnlockedUpgrades.Add("Unlocked_KegStand");
				GameLog.Info("🍺 Keg stand unlocked. You can now sell drinks!");
				break;

			case "Unlock Lodging":
				TavernStats.Instance.UnlockedUpgrades.Add("Unlocked_Lodging");
				GameLog.Info("🛏️ Lodging unlocked! (Rooms coming soon.)");
				break;

			default:
				GameLog.Debug($"⚠️ Unknown upgrade: {item.Name}");
				break;
		}
	}

	private void IncrementUpgrade(string key)
	{
		if (!TavernStats.Instance.UpgradeCounts.ContainsKey(key))
			TavernStats.Instance.UpgradeCounts[key] = 1;
		else
			TavernStats.Instance.UpgradeCounts[key]++;
	}
}
