using Godot;
using System;

public static class GameLog
{
	private static RichTextLabel _logText;

	public static void BindLogText(RichTextLabel label)
	{
		_logText = label;
		GD.Print("📝 GameLog bound to LogText.");
	}

	public static void Info(string message, bool showInConsole = true)
	{
		AddMessage(message, showInConsole);
	}

	public static void Debug(string message)
	{
		if (GameDebug.DebugMode)
			AddMessage($"[DEBUG] {message}", true);
	}

	private static void AddMessage(string message, bool showInConsole)
{
	string timestamp = ClockManager.CurrentTime.ToString("HH:mm");
	string formatted = $"[{timestamp}] {message}";

	if (showInConsole)
		GD.Print(formatted);

	if (_logText != null)
		_logText.AppendText(formatted + "\n");
}
}
