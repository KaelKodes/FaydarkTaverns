using Godot;
using System;

public partial class QuestJournalUnlockController : Control
{
	[Export] public TextureButton QuestJournalButton;
	[Export] public Control QuestJournalFX;
	[Export] public AnimatedSprite2D SparkleBurst;
	[Export] public ColorRect ShineSweep;
	[Export] public PackedScene QuestJournalScene;

	private Control _journalInstance;

	public static QuestJournalUnlockController Instance { get; private set; }

	private bool _unlocked = false;

	public override void _Ready()
	{
		if (Instance != null && Instance != this)
			GD.PrintErr("❌ Multiple QuestJournalUnlockController instances detected.");

		Instance = this;

		if (QuestJournalButton != null)
		{
			QuestJournalButton.Visible = false;
			QuestJournalButton.Scale = new Vector2(0.25f, 0.25f);
			QuestJournalButton.Position = new Vector2(17f, 833f);
			QuestJournalButton.RotationDegrees = -7.9f;
			QuestJournalButton.Modulate = Colors.White;

			QuestJournalButton.Pressed += OnJournalButtonPressed;
		}

		if (QuestJournalFX != null)
			QuestJournalFX.Visible = false;

		if (SparkleBurst != null)
		{
			SparkleBurst.Visible = false;
			SparkleBurst.Stop();
		}

		if (ShineSweep != null)
		{
			ShineSweep.Visible = false;
			var c = ShineSweep.Modulate;
			c.A = 0f;
			ShineSweep.Modulate = c;
		}
	}

	private void OnJournalButtonPressed()
	{
		if (QuestJournalScene == null)
		{
			GD.PrintErr("❌ QuestJournalScene not assigned!");
			return;
		}

		if (_journalInstance == null)
		{
			_journalInstance = QuestJournalScene.Instantiate<Control>();
			GetTree().Root.AddChild(_journalInstance);
		}

		_journalInstance.Visible = true;
		_journalInstance.MoveToFront();
	}

	public void TryUnlockJournal()
	{
		if (_unlocked)
			return;

		_unlocked = true;
		PlayUnlockAnimation();
	}

	private void PlayUnlockAnimation()
	{
		if (QuestJournalButton == null)
			return;

		if (QuestJournalFX != null)
			QuestJournalFX.Visible = true;

		if (SparkleBurst != null)
		{
			SparkleBurst.Visible = true;
			SparkleBurst.Play();
		}

		Vector2 finalScale = new Vector2(0.25f, 0.25f);
		Vector2 startScale = finalScale * 0.2f;
		Vector2 popScale = finalScale * 1.15f;

		QuestJournalButton.Visible = true;
		QuestJournalButton.Scale = startScale;

		var buttonColor = QuestJournalButton.Modulate;
		buttonColor.A = 0f;
		QuestJournalButton.Modulate = buttonColor;

		var tween = CreateTween();
		tween.SetTrans(Tween.TransitionType.Cubic);
		tween.SetEase(Tween.EaseType.Out);

		tween.TweenInterval(0.15);

		tween.Parallel()
			.TweenProperty(QuestJournalButton, "scale", popScale, 0.3);

		tween.Parallel()
			.TweenProperty(QuestJournalButton, "modulate:a", 1.0f, 0.3);

		tween.TweenProperty(QuestJournalButton, "scale", finalScale, 0.15);

		if (ShineSweep != null)
		{
			float startX = -ShineSweep.Size.X;
			float endX = (QuestJournalButton.Size.X * finalScale.X) + ShineSweep.Size.X;

			ShineSweep.Visible = true;
			var shineColor = ShineSweep.Modulate;
			shineColor.A = 0f;
			ShineSweep.Modulate = shineColor;
			ShineSweep.Position = new Vector2(startX, ShineSweep.Position.Y);

			tween.Parallel()
				.TweenProperty(ShineSweep, "position:x", endX, 0.4);

			tween.Parallel()
				.TweenProperty(ShineSweep, "modulate:a", 1.0f, 0.2)
				.SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.InOut);

			tween.Parallel()
				.TweenProperty(ShineSweep, "modulate:a", 0.0f, 0.2)
				.SetDelay(0.2);
		}

		tween.TweenCallback(Callable.From(() =>
		{
			if (SparkleBurst != null)
			{
				SparkleBurst.Stop();
				SparkleBurst.Visible = false;
			}

			if (ShineSweep != null)
				ShineSweep.Visible = false;

			if (QuestJournalFX != null)
				QuestJournalFX.Visible = false;
		}));
	}
}
