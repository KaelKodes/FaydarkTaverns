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
	[Export] public Label RequiredSkillsLabel;
	[Export] public Label RecommendedSkillsLabel;
	[Export] public Label DescriptionLabel;
	[Export] public Button AcceptButton;
	[Export] public Button CloseButton;
	[Export] public Button DismissButton;
	[Export] public ConfirmationDialog RetryDialog;



	private Quest quest;
	private Quest boundQuest;


	public override void _Ready()
	{
	
	CloseRequested += OnCloseRequested;
	CloseButton.Pressed += Hide;
	AcceptButton.Pressed += OnAcceptPressed;
	DismissButton.Pressed += OnDismissPressed;
	RetryDialog.Confirmed += OnRetryConfirmed;

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

	// safety check
	if (TitleLabel == null || DescriptionLabel == null)
		return;

	// Basic fields
	TitleLabel.Text       = q.Title;
	RegionLabel.Text      = $"Region: {q.Region}";
	TypeLabel.Text        = $"Type: {q.Type}";
	RewardLabel.Text      = $"Reward: {q.Reward}g";
	if (q.IsAccepted)
		TimeLabel.Text    = $"Est: {q.GetTotalExpectedTU()} hrs / Due: {q.Deadline:MMM dd, HH:mm}";
	else
		TimeLabel.Text    = $"Est: {q.GetTotalExpectedTU()} hrs";
	DescriptionLabel.Text = q.Description;

	// ── NEW: Required Skills ──
	var reqBuilder = new StringBuilder("Required Skills: ");
	foreach (var kv in q.Requirements.RequiredStats)
		reqBuilder.Append($"{kv.Key} {kv.Value}, ");
	RequiredSkillsLabel.Text = reqBuilder.ToString().TrimEnd(',', ' ');

	// ── NEW: Recommended (Bonus) Skills ──
	if (q.Requirements.BonusStats.Count > 0)
	{
		var recBuilder = new StringBuilder("Recommended Skills: ");
		foreach (var kv in q.Requirements.BonusStats)
			recBuilder.Append($"{kv.Key} {kv.Value}, ");
		RecommendedSkillsLabel.Text = recBuilder.ToString().TrimEnd(',', ' ');
	}
	else
	{
		RecommendedSkillsLabel.Text = "Recommended Skills: None";
	}

	// 🔧 ✅ Update retry/dismiss logic as before
	SetQuestDetails(q);
}





private void OnAcceptPressed()
{
	if (boundQuest == null) return;

	GD.Print($"[ACCEPT] Accepting quest ref: {boundQuest.GetHashCode()} | QuestId: {boundQuest.QuestId} | Title: {boundQuest.Title}");

	if (boundQuest.Failed && boundQuest.IsComplete)
	{
		int retryCost = (int)Math.Floor(boundQuest.Reward * 0.15f);
		RetryDialog.DialogText = $"Reimburse {retryCost}g to retry this mission?";
		RetryDialog.Show();
	}
	else
	{
		boundQuest.Accept();
		AcceptButton.Disabled = true;
		Hide();
	}

	Hide();

	// 🔁 Refresh all quest cards
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
			.FirstOrDefault(g => g.BoundNPC != null && g.BoundNPC == adventurer);

		if (guest != null)
		{
			var table = guest.AssignedTable;

			// ✅ Remove from the actual table data
			table?.RemoveGuest(guest);

			// ✅ Remove from tavern floor
			TavernManager.Instance.OnGuestRemoved(guest);
			GameLog.Debug($"📦 {guest.Name} left to go on a quest.");

			// ✅ Update the table UI
			table?.LinkedPanel?.UpdateSeatSlots();
		}
		else
		{
			GD.Print($"⚠️ Could not find guest for adventurer {adventurer.Name}");
		}
	}
}

private void OnDismissPressed()
{
	if (boundQuest == null)
		return;

	if (boundQuest.IsAccepted && !boundQuest.IsComplete)
	{
		GameLog.Info("⛔ You can't dismiss a quest that is already in progress.");
		return;
	}

	// 🚫 Remove assigned adventurers
	foreach (var adventurer in boundQuest.AssignedAdventurers.ToList())
	{
		QuestManager.Instance.UnassignAdventurer(boundQuest, adventurer);
	}

	QuestManager.Instance.DismissQuest(boundQuest);
	Hide();
}

private void OnRetryConfirmed()
{
	if (boundQuest == null)
		return;

	int retryCost = (int)Math.Floor(boundQuest.Reward * 0.15f);
	if (TavernManager.Gold < retryCost)
	{
		int shortfall = retryCost - TavernManager.Gold;
		GameLog.Info($"⛔ You are {shortfall}g short to retry this quest.");
		return;
	}

	QuestManager.Instance.RetryQuest(boundQuest);
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
bool isComplete = quest.IsComplete && !quest.Failed;
bool inProgress = quest.IsAccepted && !quest.IsComplete;

// ✨ Ensure Accept button is visible
AcceptButton.Visible = true;
AcceptButton.Disabled = isComplete || inProgress;

//Dismiss
DismissButton.Visible = true;

if (quest.IsAccepted && !quest.IsComplete)
{
	DismissButton.Disabled = true;
	DismissButton.TooltipText = "They've already left!";
}
else
{
	DismissButton.Disabled = false;
	DismissButton.TooltipText = "Dismiss this quest and remove any assigned adventurers.";
}



// 🔄 Configure Accept button based on quest state
if (showRetry)
{
	AcceptButton.Text = "Retry";
	AcceptButton.TooltipText = "Pay 15% of reward to try again.";
}
else if (isComplete)
{
	AcceptButton.Text = "Completed";
	AcceptButton.TooltipText = "This quest has already been completed.";
}
else if (inProgress)
{
	AcceptButton.Text = "In Progress";
	AcceptButton.TooltipText = "This quest is already underway.";
}
else
{
	AcceptButton.Text = "Accept";
	AcceptButton.TooltipText = "Send adventurers on this quest.";
}


}







}
