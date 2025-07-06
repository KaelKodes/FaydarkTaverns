using Godot;
using System;
using System.Text;
using System.Linq;


public partial class QuestDetailPopup : Window
{
	[Export] public Label TitleLabel;
	[Export] public Label RegionLabel;
	[Export] public Label TypeLabel;
	[Export] public Label RewardLabel;
	[Export] public Label TimeLabel;
	[Export] public Label OptimalRolesLabel;
	[Export] public Label DescriptionLabel;
	[Export] public Button AcceptButton;
	[Export] public Button CloseButton;
	[Export] public Button DismissButton;


	private Quest quest;
	private Quest boundQuest;


	public override void _Ready()
	{
	
	CloseRequested += OnCloseRequested;
	CloseButton.Pressed += Hide;
	AcceptButton.Pressed += OnAcceptPressed;
	DismissButton.Pressed += OnDismissPressed;

	}
	
	private string GetRoleName(int roleId)
{
	return roleId switch
	{
		1 => "Tank",
		2 => "Physical DPS",
		3 => "Magic DPS",
		4 => "Healer",
		5 => "Utility",
		_ => "Unknown"
	};
}


	public void SetQuest(Quest q)
{
	quest = q;
	boundQuest = q;

	if (TitleLabel == null || DescriptionLabel == null)
		return;

	TitleLabel.Text = q.Title;
	RegionLabel.Text = $"Region: {q.Region}";
	TypeLabel.Text = $"Type: {q.Type}";
	RewardLabel.Text = $"Reward: {q.Reward}g";
	if (q.IsAccepted)
	TimeLabel.Text = $"Est: {q.GetTotalExpectedTU()} hrs / Due: {q.Deadline:MMM dd, HH:mm}";
else
	TimeLabel.Text = $"Est: {q.GetTotalExpectedTU()} hrs";


	var roles = new StringBuilder("Optimal Roles: ");
	foreach (var role in q.OptimalRoles)
		roles.Append($"{GetRoleName(role)}, ");
	OptimalRolesLabel.Text = roles.ToString().TrimEnd(',', ' ');

	DescriptionLabel.Text = q.Description;

	// ✅ Enable/disable Accept button properly
	AcceptButton.Disabled = q.IsAccepted || q.IsLocked;
}



private void OnAcceptPressed()
{
	if (boundQuest == null) return;

	GD.Print($"[ACCEPT] Accepting quest ref: {boundQuest.GetHashCode()} | QuestId: {boundQuest.QuestId} | Title: {boundQuest.Title}");

	if (boundQuest.Failed && boundQuest.IsComplete)
{
	QuestManager.Instance.RetryQuest(boundQuest);
}
else
{
	boundQuest.Accept();
	AcceptButton.Disabled = true;
}

Hide();


	// Refresh all quest cards
	foreach (var card in GetTree().GetNodesInGroup("QuestCard"))
	{
		if (card is QuestCard qc && qc.HasQuest(boundQuest))
			qc.UpdateDisplay();
	}

	// 🚶 Remove assigned adventurers from the tavern floor
	foreach (var adventurer in boundQuest.AssignedAdventurers)
	{
		var guest = TavernManager.Instance
			.GetGuestsInside()
			.FirstOrDefault(g => g.BoundAdventurer == adventurer);

		if (guest != null)
		{
			var table = guest.AssignedTable;

			// ✅ Remove from the actual table data
			table?.RemoveGuest(guest);

			// ✅ Remove from tavern floor
			TavernManager.Instance.OnGuestRemoved(guest);
			GameLog.Debug($"📦 {guest.Name} left to go on a quest.");

			// ✅ Update the table UI
			table.LinkedPanel.UpdateSeatSlots();

		}
		else
		{
			GD.Print($"⚠️ Could not find guest for adventurer {adventurer.Name}");
		}
	}
}
private void OnDismissPressed()
{
	if (boundQuest == null) return;

	QuestManager.Instance.DismissQuest(boundQuest);
	Hide();
}

	private void OnCloseRequested()
{
	Hide(); // or QueueFree() if you want to destroy it
}

public void SetQuestDetails(Quest quest)
{
	this.quest = quest;
	this.boundQuest = quest;

	bool showRetry = quest.IsComplete && quest.Failed;

	DismissButton.Visible = showRetry;

	if (showRetry)
	{
		AcceptButton.Text = "Retry";
		AcceptButton.TooltipText = "Pay 15% of reward to try again.";
	}
	else
	{
		AcceptButton.Text = "Accept";
		AcceptButton.TooltipText = "Send adventurers on this quest.";
	}
}





}
