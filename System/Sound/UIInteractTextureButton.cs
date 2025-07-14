using Godot;

public partial class UIInteractTextureButton : TextureButton
{
	public override void _Ready()
	{
		MouseEntered += () =>
		{
			UIAudio.Instance.PlayHover(this);
			UICursor.Instance.SetPoint();
		};
		MouseExited += () =>
		{
			UICursor.Instance.SetIdle();
		};
		FocusEntered += () =>
		{
			UIAudio.Instance.PlayHover(this);
			UICursor.Instance.SetPoint();
		};
		FocusExited += () =>
		{
			UICursor.Instance.SetIdle();
		};
		Pressed += () => UIAudio.Instance.PlayClick();
	}
}
