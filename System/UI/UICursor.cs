using Godot;

public partial class UICursor : Node
{
	public static UICursor Instance { get; private set; }

	[Export] public Texture2D IdleCursor;
	[Export] public Texture2D GrabCursor;
	[Export] public Texture2D PointCursor;

	public override void _EnterTree()
	{
		Instance = this;
		SetIdle();
	}

	public void SetIdle()
	{
		Input.SetCustomMouseCursor(IdleCursor, Input.CursorShape.Arrow, new Vector2(8, 8));
	}

	public void SetPoint()
	{
		Input.SetCustomMouseCursor(PointCursor, Input.CursorShape.PointingHand, new Vector2(8, 0));
	}

	public void SetGrab()
	{
		Input.SetCustomMouseCursor(GrabCursor, Input.CursorShape.Drag, new Vector2(8, 8));
	}
}
