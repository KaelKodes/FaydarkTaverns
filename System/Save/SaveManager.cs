using Godot;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

public static class SaveManager
{
	// ================================
	// VERSIONING
	// ================================
	public static readonly SaveVersion CurrentSaveVersion = new SaveVersion(0, 9, 36);

	// ================================
	// ROOT PATHS
	// ================================
	private static string RootSaveDir => OS.GetUserDataDir(); // Godot-safe user:// path

	// OLD SYSTEM (still works — do not remove yet)
	private static string AutoSaveDir => Path.Combine(RootSaveDir, "saves", "auto");
	private static string ManualSaveDir => Path.Combine(RootSaveDir, "saves", "manual");
	private static string AutoSavePath => Path.Combine(AutoSaveDir, "autosave.json");

	// NEW SYSTEM — SLOT-BASED SAVE PATHS
	public static int CurrentSlot { get; private set; } = 1;

	private static string GetSlotDir(int slot)
	{
		return Path.Combine(RootSaveDir, "saves", $"Slot{slot}");
	}

	private static string GetSlotManualPath(int slot)
	{
		return Path.Combine(GetSlotDir(slot), "manual.json");
	}

	private static string GetSlotAutoPath(int slot)
	{
		return Path.Combine(GetSlotDir(slot), "autosave.json");
	}

	private static void EnsureSlotDirectory(int slot)
	{
		Directory.CreateDirectory(GetSlotDir(slot));
	}

	public static void SetCurrentSlot(int slot)
	{
		if (slot < 1 || slot > 4)
		{
			GD.PrintErr($"[SaveManager] Invalid slot index {slot}. Must be 1–4.");
			return;
		}

		CurrentSlot = slot;
		GD.Print($"[SaveManager] Selected Slot {slot}");
	}

	// ================================
	// JSON SETTINGS
	// ================================
	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		IncludeFields = true,
		PropertyNameCaseInsensitive = true,
		Converters = { new JsonStringEnumConverter() }
	};

	// ================================
	// PUBLIC API — OLD AUTOSAVE SYSTEM
	// (still works for now, not removed)
	// ================================
	public static void SaveAuto(SaveData data)
	{
		EnsureDirectories();

		data.Meta.Kind = SaveKind.Auto;
		data.Meta.Version = CurrentSaveVersion;
		StampMeta(data);

		WriteToFile(AutoSavePath, data);
		GD.Print($"[SaveManager] Autosaved to {AutoSavePath}");
	}

	public static SaveData LoadAuto()
	{
		if (!File.Exists(AutoSavePath))
		{
			GD.Print("[SaveManager] No autosave found.");
			return null;
		}

		var data = ReadFromFile(AutoSavePath);
		if (data == null)
			return null;

		MigrateIfNeeded(data);

		GD.Print($"[SaveManager] Loaded autosave from {AutoSavePath} (v{data.Meta.Version})");
		return data;
	}

	// ================================
	// PUBLIC API — OLD MANUAL SAVE SYSTEM
	// (kept for compatibility)
	// ================================
	public static string CreateNewManualSave(SaveData data)
	{
		EnsureDirectories();

		string fileName = GenerateManualSlotFileName();
		string path = Path.Combine(ManualSaveDir, fileName);

		data.Meta.Kind = SaveKind.Manual;
		data.Meta.Version = CurrentSaveVersion;
		StampMeta(data);

		WriteToFile(path, data);
		GD.Print($"[SaveManager] Created manual save: {path}");

		return path;
	}

	public static void OverwriteManualSave(string path, SaveData data)
	{
		EnsureDirectories();

		data.Meta.Kind = SaveKind.Manual;
		data.Meta.Version = CurrentSaveVersion;
		StampMeta(data);

		WriteToFile(path, data);
		GD.Print($"[SaveManager] Overwrote manual save: {path}");
	}

	public static SaveData LoadManual(string path)
	{
		if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
		{
			GD.PrintErr($"[SaveManager] Manual load failed. File missing: {path}");
			return null;
		}

		var data = ReadFromFile(path);
		if (data == null)
			return null;

		MigrateIfNeeded(data);

		GD.Print($"[SaveManager] Loaded manual save from {path} (v{data.Meta.Version})");
		return data;
	}

	public static List<ManualSaveSlotInfo> GetManualSaves()
	{
		EnsureDirectories();

		var result = new List<ManualSaveSlotInfo>();

		if (!Directory.Exists(ManualSaveDir))
			return result;

		foreach (var file in Directory.GetFiles(ManualSaveDir, "*.json"))
		{
			try
			{
				var data = ReadFromFile(file);
				if (data == null || data.Meta == null)
					continue;

				result.Add(new ManualSaveSlotInfo
				{
					FilePath = file,
					Version = data.Meta.Version,
					Kind = data.Meta.Kind,
					RealWorldTimestamp = data.Meta.RealWorldTimestamp,
					GameDay = data.Meta.GameDay,
					GameHour = data.Meta.GameHour,
					TavernName = data.Meta.TavernName,
					TavernLevel = data.Meta.TavernLevel,
					Renown = data.Meta.Renown
				});
			}
			catch (Exception e)
			{
				GD.PrintErr($"[SaveManager] Failed reading manual save: {file}\n{e}");
			}
		}

		return result;
	}

	// ======================================================================
	// NEW SYSTEM — SLOT-BASED SAVE/LOAD
	// ======================================================================

	public static void SaveManualToSlot(int slot, SaveData data)
	{
		EnsureSlotDirectory(slot);

		string path = GetSlotManualPath(slot);

		data.Meta.Kind = SaveKind.Manual;
		data.Meta.Version = CurrentSaveVersion;
		StampMeta(data);

		WriteToFile(path, data);
		GD.Print($"[SaveManager] Saved MANUAL to Slot {slot}: {path}");
	}

	public static void SaveAutoToSlot(int slot, SaveData data)
	{
		EnsureSlotDirectory(slot);

		string path = GetSlotAutoPath(slot);

		data.Meta.Kind = SaveKind.Auto;
		data.Meta.Version = CurrentSaveVersion;
		StampMeta(data);

		WriteToFile(path, data);
		GD.Print($"[SaveManager] Saved AUTOSAVE to Slot {slot}: {path}");
	}

	public static SaveData LoadManualFromSlot(int slot)
	{
		string path = GetSlotManualPath(slot);

		if (!File.Exists(path))
			return null;

		var data = ReadFromFile(path);
		if (data == null)
			return null;

		MigrateIfNeeded(data);
		return data;
	}

	public static SaveData LoadAutoFromSlot(int slot)
	{
		string path = GetSlotAutoPath(slot);

		if (!File.Exists(path))
			return null;

		var data = ReadFromFile(path);
		if (data == null)
			return null;

		MigrateIfNeeded(data);
		return data;
	}

	// Convenience for using the "active" slot
	public static void SaveManualToCurrentSlot(SaveData data)
		=> SaveManualToSlot(CurrentSlot, data);

	public static void SaveAutoToCurrentSlot(SaveData data)
		=> SaveAutoToSlot(CurrentSlot, data);

	// ======================================================================
	// SLOT METADATA FOR UI
	// ======================================================================
	public static ManualSaveSlotInfo GetManualSlotInfo(int slot)
	{
		string path = GetSlotManualPath(slot);

		if (!File.Exists(path))
			return null;

		var data = ReadFromFile(path);
		if (data?.Meta == null)
			return null;

		return new ManualSaveSlotInfo
		{
			FilePath = path,
			Version = data.Meta.Version,
			Kind = data.Meta.Kind,
			RealWorldTimestamp = data.Meta.RealWorldTimestamp,
			GameDay = data.Meta.GameDay,
			GameHour = data.Meta.GameHour,
			TavernName = data.Meta.TavernName,
			TavernLevel = data.Meta.TavernLevel,
			Renown = data.Meta.Renown
		};
	}

	public static ManualSaveSlotInfo GetAutoSlotInfo(int slot)
	{
		string path = GetSlotAutoPath(slot);

		if (!File.Exists(path))
			return null;

		var data = ReadFromFile(path);
		if (data?.Meta == null)
			return null;

		return new ManualSaveSlotInfo
		{
			FilePath = path,
			Version = data.Meta.Version,
			Kind = data.Meta.Kind,
			RealWorldTimestamp = data.Meta.RealWorldTimestamp,
			GameDay = data.Meta.GameDay,
			GameHour = data.Meta.GameHour,
			TavernName = data.Meta.TavernName,
			TavernLevel = data.Meta.TavernLevel,
			Renown = data.Meta.Renown
		};
	}

	// ================================
	// INTERNAL HELPERS
	// ================================

	private static void EnsureDirectories()
	{
		Directory.CreateDirectory(AutoSaveDir);
		Directory.CreateDirectory(ManualSaveDir);
	}

	private static void StampMeta(SaveData data)
	{
		if (data.Meta == null)
			data.Meta = new MetaData();

		data.Meta.Version = CurrentSaveVersion;
		data.Meta.RealWorldTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
	}

	private static void WriteToFile(string path, SaveData data)
	{
		string json = JsonSerializer.Serialize(data, JsonOptions);
		File.WriteAllText(path, json);
	}

	private static SaveData ReadFromFile(string path)
	{
		try
		{
			string json = File.ReadAllText(path);
			return JsonSerializer.Deserialize<SaveData>(json, JsonOptions);
		}
		catch (Exception e)
		{
			GD.PrintErr($"[SaveManager] Failed to read: {path}\n{e}");
			return null;
		}
	}

	private static string GenerateManualSlotFileName()
	{
		int index = 1;

		while (File.Exists(Path.Combine(ManualSaveDir, $"slot_{index:000}.json")))
			index++;

		return $"slot_{index:000}.json";
	}

	// ================================
	// MIGRATION SYSTEM
	// ================================
	private static void MigrateIfNeeded(SaveData data)
	{
		if (data.Meta == null)
			data.Meta = new MetaData { Version = new SaveVersion(0, 0, 0) };

		// Add migrations here later

		data.Meta.Version = CurrentSaveVersion;
	}

	private static bool IsLessThan(SaveVersion a, SaveVersion b)
		=> a.CompareTo(b) < 0;
}

/* ========== UI-FACING SAVE SLOT INFO ========== */

public class ManualSaveSlotInfo
{
	public string FilePath;
	public SaveVersion Version;
	public SaveKind Kind;
	public string RealWorldTimestamp;
	public int GameDay;
	public int GameHour;
	public string TavernName;
	public int TavernLevel;
	public int Renown;
}
