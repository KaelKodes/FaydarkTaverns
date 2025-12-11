using System;

public enum SaveKind
{
	Auto,
	Manual
}

[Serializable]
public struct SaveVersion : IComparable<SaveVersion>
{
	public int Major;
	public int Minor;
	public int Patch;

	public SaveVersion(int major, int minor, int patch)
	{
		Major = major;
		Minor = minor;
		Patch = patch;
	}

	public override string ToString()
	{
		return $"{Major}.{Minor}.{Patch}";
	}

	public static SaveVersion FromString(string version)
	{
		if (string.IsNullOrWhiteSpace(version))
			return new SaveVersion(0, 0, 0);

		var parts = version.Split('.');
		if (parts.Length != 3)
			return new SaveVersion(0, 0, 0);

		int major = int.TryParse(parts[0], out var ma) ? ma : 0;
		int minor = int.TryParse(parts[1], out var mi) ? mi : 0;
		int patch = int.TryParse(parts[2], out var pa) ? pa : 0;

		return new SaveVersion(major, minor, patch);
	}

	public int CompareTo(SaveVersion other)
	{
		if (Major != other.Major) return Major.CompareTo(other.Major);
		if (Minor != other.Minor) return Minor.CompareTo(other.Minor);
		return Patch.CompareTo(other.Patch);
	}
}

[Serializable]
public class MetaData
{
	public SaveVersion Version;
	public SaveKind Kind;

	// convenience / UI fields
	public string RealWorldTimestamp;   // "2025-12-07 23:10"
	public int GameDay;
	public int GameHour;
	public string TavernName;
	public int TavernLevel;
	public int Renown;
}
