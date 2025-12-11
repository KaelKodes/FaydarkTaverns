using Godot;

public static class NodeExtensions
{
	public static void ClearChildren(this Node node)
	{
		if (node == null || !GodotObject.IsInstanceValid(node)) return;

		foreach (var child in node.GetChildren())
		{
			if (child != null && GodotObject.IsInstanceValid(child))
				child.QueueFree();
		}
	}

	public static void QueueFreeChildren(this Control container)
	{
		foreach (var child in container.GetChildren())
			child.QueueFree();
	}

}
