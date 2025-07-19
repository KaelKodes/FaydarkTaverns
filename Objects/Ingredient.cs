using System.Collections.Generic;

public class Ingredient
{
	public string Id { get; set; }
	public string Name { get; set; }
	public string Type { get; set; }
	public float Value { get; set; }
	public int Rarity { get; set; }
	public List<string> FoundInAreas { get; set; }
	public bool Perishable { get; set; }
	public List<string> Flavors { get; set; }
	public bool IsDiscovered { get; set; }  // New field

	// Parameterless constructor for deserialization
	public Ingredient()
	{
		FoundInAreas = new List<string>();
		Flavors = new List<string>();
	}

	// Optional constructor for manual instantiation
	public Ingredient(string id, string name, string type, float value, int rarity, List<string> foundInAreas, bool perishable, List<string> flavors, bool isDiscovered = false)
	{
		Id = id;
		Name = name;
		Type = type;
		Value = value;
		Rarity = rarity;
		FoundInAreas = foundInAreas ?? new List<string>();
		Perishable = perishable;
		Flavors = flavors ?? new List<string>();
		IsDiscovered = isDiscovered;
	}
}
