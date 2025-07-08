using Godot;

public partial class UIAudio : Node
{
	public static UIAudio Instance { get; private set; }

	public override void _EnterTree()
	{
		Instance = this;
	}
	
	[Export] public AudioStreamPlayer AudioPlayer;

	[Export] public AudioStream HoverSound;
	[Export] public AudioStream ClickSound;
	[Export] public AudioStream ShopEnterSound; // Add this line!

	// ...other sounds...

	public void PlayHover(Node button = null)
	{
		// (Your existing hover logic)
		if (HoverSound != null && AudioPlayer != null)
		{
			AudioPlayer.Stream = HoverSound;
			AudioPlayer.Play();
		}
	}

	public void PlayClick()
	{
		if (ClickSound != null && AudioPlayer != null)
		{
			AudioPlayer.Stream = ClickSound;
			AudioPlayer.Play();
		}
	}

	// Dedicated Shop Enter sound
	public void PlayShopEnter()
	{
		if (ShopEnterSound != null && AudioPlayer != null)
		{
			AudioPlayer.Stream = ShopEnterSound;
			AudioPlayer.Play();
		}
	}
}
