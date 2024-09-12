using Sandbox;
using System.Collections.Generic;
using System.Linq;
using static DressinTerry;

public class DressinTerrySystem : GameObjectSystem
{
	public DressinTerrySystem(Scene scene) : base(scene)
	{
		if (allClothing.Count != 0)
			return;

		allClothing = ResourceLibrary.GetAll<Clothing>().ToList();

		foreach (var clothing in allClothing)
		{
			if (!clothingCategories.Contains(clothing.Category))
			{
				clothingCategories.Add(clothing.Category);
			}

			if (!clothingSubCategories.Contains(clothing.SubCategory))
			{
				clothingSubCategories.Add(clothing.SubCategory);
			}

			if (!clothingCategoryToSubCategory.ContainsKey(clothing.Category))
			{
				clothingCategoryToSubCategory[clothing.Category] = new List<string>();
			}

			if (!clothingCategoryToSubCategory[clothing.Category].Contains(clothing.SubCategory))
			{
				clothingCategoryToSubCategory[clothing.Category].Add(clothing.SubCategory);
			}
		}
	}
}