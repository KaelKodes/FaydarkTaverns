using Godot;
using System.Collections.Generic;
using System;

public partial class IngredientDatabase : Node
{
	public static Dictionary<string, Ingredient> Ingredients = new();

	public override void _Ready()
	{
		LoadFromCSV("res://System/Databases/IngredientList.csv");  // <– Path to your data
	}

	public static void LoadFromCSV(string path)
	{
		Ingredients.Clear();

		var file = Godot.FileAccess.Open(path, FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr("❌ Ingredient CSV not found at path: " + path);
			return;
		}

		string header = file.GetLine(); // skip header

		while (!file.EofReached())
		{
			string line = file.GetLine();
			if (string.IsNullOrWhiteSpace(line))
				continue;

			var parts = line.Split(',');

			if (parts.Length < 8)
			{
				GD.PrintErr("❌ Malformed ingredient row: " + line);
				continue;
			}

			string id = parts[0].Trim();
			string name = parts[1].Trim();
			string type = parts[2].Trim();
			bool perishable = parts[3].Trim().ToLower() == "true";
			int value = int.TryParse(parts[4].Trim(), out var v) ? v : 0;
			string rarity = parts[5].Trim();
			string region = parts[6].Trim();
			var flavors = new List<string>(parts[7].Split('|', StringSplitOptions.RemoveEmptyEntries));

			var ing = new Ingredient(id, name, type, value, rarity, region, perishable, flavors);
			Ingredients[id] = ing;
		}

		GD.Print($"✅ Loaded {Ingredients.Count} ingredients from {path}");
	}
}
