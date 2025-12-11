using Godot;
using System;

public partial class ConfirmationWindow : Control
{
	public static ConfirmationWindow Instance;

	[Export] public Label ConfirmationLabel;
	[Export] public Button YesButton;
	[Export] public Button NoButton;

	private Action _onConfirm;
	private Action _onCancel;

	public override void _Ready()
	{
		Instance = this;
		Visible = false;

		YesButton.Pressed += () =>
		{
			Hide();
			_onConfirm?.Invoke();
		};

		NoButton.Pressed += () =>
		{
			Hide();
			_onCancel?.Invoke();
		};
	}

	public void ShowWindow(
		string message,
		Action onConfirm,
		Action onCancel = null)
	{
		ConfirmationLabel.Text = message;
		_onConfirm = onConfirm;
		_onCancel = onCancel;

		Visible = true;
	}

	public void ShowTripleChoice(
		string title,
		string manualText,
		string autoText,
		Action onManual,
		Action onAuto,
		Action onCancel)
	{
		ConfirmationLabel.Text = title;

		YesButton.Text = manualText;
		NoButton.Text = autoText;

		_onConfirm = onManual;
		_onCancel = onAuto;

		Visible = true;
	}
}
