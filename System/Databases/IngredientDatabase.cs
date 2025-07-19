using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

public partial class IngredientDatabase : Node
{
	public static Dictionary<string, Ingredient> Ingredients = new();

	public override void _Ready()
	{
		LoadFromJSON("res://System/Databases/IngredientDatabase.json");
	}

	public static void LoadFromJSON(string path)
	{
		Ingredients.Clear();

		try
		{
			// Read file contents using Godot API
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
			string jsonText = file.GetAsText();

			// Deserialize using System.Text.Json
			var ingredientList = JsonSerializer.Deserialize<List<Ingredient>>(jsonText);

			if (ingredientList != null)
			{
				Ingredients = ingredientList.ToDictionary(i => i.Id, i => i);
				GD.Print($"✅ Loaded {Ingredients.Count} ingredients from {path}");
			}
			else
			{
				GD.PrintErr("❌ Failed to deserialize ingredient list");
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"❌ Exception loading ingredient JSON: {ex.Message}");
		}
	}
}
