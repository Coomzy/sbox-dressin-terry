
using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System;
using static Sandbox.Clothing;
using Sandbox.Citizen;

public static class DressinTerry
{
	public const string extension = "chr";

	public static List<Clothing> allClothing = new List<Clothing>();
	public static List<ClothingCategory> clothingCategories = new List<ClothingCategory>();
	public static List<string> clothingSubCategories = new List<string>();
	public static Dictionary<ClothingCategory, List<string>> clothingCategoryToSubCategory = new Dictionary<ClothingCategory, List<string>>();

	public static ClothingContainer RandomCharacter()
	{
		List<Clothing> clothing = new List<Clothing>();
		foreach (var clothingCategory in clothingCategories)
		{
			var categoryItems = allClothing.Where((x) => x.Category == clothingCategory);
			if (!categoryItems.Any())
			{
				continue;
			}
			var clothingItem = categoryItems.OrderBy(x => Guid.NewGuid()).First();

			if (clothing.Contains(clothingItem))
				continue;
			clothing.Add(clothingItem);
		}

		var clothingContainer = new ClothingContainer();
		foreach (var clothingItem in clothing)
		{
			clothingContainer.Toggle(clothingItem);
		}
		return clothingContainer;
	}

	public static Clothing RandomClothing(ClothingCategory? category = null, string subCategory = null, List<Clothing> wearingClothing = null)
	{
		IEnumerable<Clothing> clothing = allClothing.ToList();

		if (wearingClothing != null && wearingClothing.Any())
		{
			clothing = clothing.Where(x => !wearingClothing.Contains(x) && wearingClothing.All(y => y.CanBeWornWith(x)));
		}

		if (category.HasValue)
		{
			clothing = clothing.Where((x) => x.Category == category.Value);
		}

		if (subCategory != null)
		{
			clothing = clothing.Where((x) => x.SubCategory == subCategory);
		}

		if (!clothing.Any())
		{
			return null;
		}

		var rndIndex = Game.Random.Int(0, clothing.Count()-1);
		return clothing.ElementAt(rndIndex);
	}

	public static void ApplyClothing(this SkinnedModelRenderer bodyRenderer, ClothingContainer clothingContainer)
	{
		if (bodyRenderer == null || !bodyRenderer.IsValid)
		{
			Log.Error($"Tried to apply clothing '{clothingContainer}' to a null bodyRenderer!");
			return;
		}

		// HACK: Workaround for spawning in edit time bug
		if (Game.IsEditor)
		{
			var activeScene = Game.ActiveScene;
			Game.ActiveScene = bodyRenderer.Scene;

			clothingContainer.Apply(bodyRenderer);
			var animHelper = bodyRenderer.Components.Get<CitizenAnimationHelper>();
			if (animHelper != null && animHelper.IsValid)
			{
				animHelper.Height = clothingContainer.Height;
			}
			else
			{
				Log.Warning($"bodyRenderer '{bodyRenderer.GameObject.Name}' had no CitizenAnimationHelper on it, unable to set character height!");
			}

			Game.ActiveScene = activeScene;

			if (bodyRenderer.Scene != null)
			{
				bodyRenderer.Scene.Editor.HasUnsavedChanges = true;
			}
		}
		else
		{
			clothingContainer.Apply(bodyRenderer);
			var animHelper = bodyRenderer.Components.Get<CitizenAnimationHelper>();
			if (animHelper != null && animHelper.IsValid)
			{
				animHelper.Height = clothingContainer.Height;
			}
			else
			{
				Log.Warning($"bodyRenderer '{bodyRenderer.GameObject.Name}' had no CitizenAnimationHelper on it, unable to set character height!");
			}
		}
	}

	[ConCmd("dump_clothing")]
	public static void DumpClothing()
	{
		Log.Info($"DumpClothing()");
		foreach (var category in clothingCategories)
		{
			Log.Info($"category: {category}");
		}
		foreach (var subCategory in clothingSubCategories)
		{
			Log.Info($"subCategory: {subCategory}");
		}
		foreach (var subCategoryList in clothingCategoryToSubCategory)
		{
			foreach (var subCategory in subCategoryList.Value)
			{
				Log.Info($"subCategoryList[{subCategoryList.Key}] subCategory: {subCategory}");
			}
		}
	}
}