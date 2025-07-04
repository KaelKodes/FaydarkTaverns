using Godot;

public static class NodeExtensions
{
	public static void ClearChildren(this Node node)
	{
		foreach (var child in node.GetChildren())
			child.QueueFree();
	}
}
