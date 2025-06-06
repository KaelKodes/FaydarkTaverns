using Godot;
using System;
using System.Text;

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

	private Quest quest;

	public override void _Ready()
	{
	
	CloseRequested += OnCloseRequested;
	CloseButton.Pressed += Hide;
	AcceptButton.Pressed += OnAcceptPressed;
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
	if (TitleLabel == null || DescriptionLabel == null)
		return;
		TitleLabel.Text = q.Title;
		RegionLabel.Text = $"Region: {q.Region}";
		TypeLabel.Text = $"Type: {q.Type}";
		RewardLabel.Text = $"Reward: {q.Reward}g";
		TimeLabel.Text = $"Est: {q.GetTotalExpectedTU()} TU / Due: {q.DeadlineTU}";

		var roles = new StringBuilder("Optimal Roles: ");
foreach (var role in q.OptimalRoles)
	roles.Append($"{GetRoleName(role)}, ");
OptimalRolesLabel.Text = roles.ToString().TrimEnd(',', ' ');


		DescriptionLabel.Text = q.Description;
	}

	private void OnAcceptPressed()
	{
		GD.Print($"✅ Quest accepted: {quest.Title}");
		// Eventually: assign adventurers or flag as 'in progress'
		Hide();
	}
	private void OnCloseRequested()
{
	Hide(); // or QueueFree() if you want to destroy it
}

}
