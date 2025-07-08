using Godot;

public partial class ShopButtonSound : Button
{
	public override void _Ready()
	{
		MouseEntered += () => UIAudio.Instance.PlayHover();
		FocusEntered += () => UIAudio.Instance.PlayHover();
		Pressed += () => UIAudio.Instance.PlayShopEnter();
	}
}
