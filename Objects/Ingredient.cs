using System.Collections.Generic;

public class Ingredient
{
	public string Id;
	public string Name;
	public string Type;
	public int Value;
	public string Rarity;
	public string Region;
	public bool Perishable;
	public List<string> Flavors;

	public Ingredient(string id, string name, string type, int value, string rarity, string region, bool perishable, List<string> flavors)
	{
		Id = id;
		Name = name;
		Type = type;
		Value = value;
		Rarity = rarity;
		Region = region;
		Perishable = perishable;
		Flavors = flavors;
	}
}
