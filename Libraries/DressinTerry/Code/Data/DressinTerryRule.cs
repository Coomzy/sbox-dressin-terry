
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using static DressinTerry;

[GameResource("Dressin Terry Rule", "rule", "A Dressin Terry Rule for Random Characters", Icon = "rule")]
public class DressinTerryRule : GameResource
{
	[Group("Height"), Order(1), Property, Range(0.5f, 2.0f)] public float? characterHeightOverride { get; set; } = 1.0f;
	[Group("Height"), Order(1), Property, Range(0.5f, 2.0f)] public Vector2 characterHeightRandomRange { get; set; } = new Vector2(0.8f, 1.2f);

	[Group("Global Filters"), Order(2), Property] public List<Clothing> clothingGlobalWhiteList { get; set; } = new List<Clothing>();
	[Group("Global Filters"), Order(2), Property] public List<Clothing> clothingGlobalBlacklist { get; set; } = new List<Clothing>();

	[Group("Global Filters"), Order(2), Property] public List<Clothing.ClothingCategory> categoriesGlobalWhiteList { get; set; } = new List<Clothing.ClothingCategory>();
	[Group("Global Filters"), Order(2), Property] public List<Clothing.ClothingCategory> categoriesGlobalBlacklist { get; set; } = new List<Clothing.ClothingCategory>();

	[Group("Global Filters"), Order(2), Property] public List<string> subCategoriesGlobalWhiteList { get; set; } = new List<string>();
	[Group("Global Filters"), Order(2), Property] public List<string> subCategoriesGlobalBlacklist { get; set; } = new List<string>();

	[Group("Rules"), Order(3), Property, InlineEditor] public List<RuleInst> rules { get; set; } = new List<RuleInst>();

	public static ClothingContainer ToClothingContainer(DressinTerryRule rule)
	{
		var clothingContainer = new ClothingContainer();
		if (rule != null)
		{
			var allRules = rule.rules.ToList();
			List<Clothing> currentClothing = new List<Clothing>();
			foreach (var ruleInst in allRules)
			{
				if (ruleInst.chanceOf != null)
				{
					var rndChance = Game.Random.Float();

					if (rndChance > ruleInst.chanceOf)
						continue;
				}

				var entry = ruleInst.GetRandomEntry(rule, currentClothing);
				if (entry?.Clothing == null)
				{
					continue;
				}

				clothingContainer.Toggle(entry.Clothing);

				var entryInst = clothingContainer.FindEntry(entry.Clothing);
				if (entryInst != null)
				{
					entryInst.Tint = entry.Tint;
				}

				currentClothing.Add(entry.Clothing);
			}
			clothingContainer.Height = rule.characterHeightOverride != null ? rule.characterHeightOverride.Value : Game.Random.Float(rule.characterHeightRandomRange.x, rule.characterHeightRandomRange.y);
		}
		return clothingContainer;
	}

	public void Apply(SkinnedModelRenderer bodyRenderer)
	{
		if (bodyRenderer == null)
		{
			Log.Error($"Tried to apply rule '{this}' to a null bodyRenderer!");
			return;
		}
		//bodyRenderer.ApplyClothing(ToClothingContainer(this));
		DressinTerry.ApplyClothing(bodyRenderer, ToClothingContainer(this));
	}
}

public enum DressingTerryRuleType
{
	Any,
	FromListOnly,
	Blacklist
}

public class RuleInst
{
	[Group(/*"Clothing"*/"Rule"), Order(1), Property] public DressingTerryRuleType clothingRuleType { get; set; }
	[Group(/*"Clothing"*/"Rule"), Order(2), Property, ShowIf(nameof(shouldShowClothing), true)] public List<Clothing> clothing { get; set; } = new List<Clothing>();

	[Group(/*"Category"*/"Rule"), Order(3), Property, ShowIf(nameof(shouldShowCategories), true)] public DressingTerryRuleType categoriesRuleType { get; set; }
	[Group(/*"Category"*/"Rule"), Order(4), Property, ShowIf(nameof(shouldShowCategoriesList), true)] public List<Clothing.ClothingCategory> categories { get; set; } = new List<Clothing.ClothingCategory>();

	[Group(/*"Sub Category"*/"Rule"), Order(5), Property, ShowIf(nameof(shouldShowSubCategories), true)] public DressingTerryRuleType subCategoriesRuleType { get; set; }
	[Group(/*"Sub Category"*/"Rule"), Order(6), Property, ShowIf(nameof(shouldShowSubCategoriesList), true)] public List<string> subCategories { get; set; } = new List<string>();

	[Group(/*"Tint"*/"Rule"), Order(10), Property, Range(0, 1)] public float? tintOverride { get; set; }
	[Group(/*"Tint"*/"Rule"), Order(11), Property, Range(0, 1)] public float? chanceOf { get; set; }

	[Hide] public bool shouldShowClothing => clothingRuleType != DressingTerryRuleType.Any;
	[Hide] public bool shouldShowCategories => clothingRuleType != DressingTerryRuleType.FromListOnly;
	[Hide] public bool shouldShowCategoriesList => shouldShowCategories && categoriesRuleType != DressingTerryRuleType.Any;
	[Hide] public bool shouldShowSubCategories => clothingRuleType != DressingTerryRuleType.FromListOnly;
	[Hide] public bool shouldShowSubCategoriesList => shouldShowCategories && subCategoriesRuleType != DressingTerryRuleType.Any;

	public ClothingContainer.ClothingEntry GetRandomEntry(DressinTerryRule rule, List<Clothing> currentClothing = null)
	{
		var whitelistedClothing =  new List<Clothing>(rule.clothingGlobalWhiteList);
		var blacklistedClothing =  new List<Clothing>(rule.clothingGlobalBlacklist);

		if (clothingRuleType == DressingTerryRuleType.FromListOnly)
		{
			whitelistedClothing.AddRange(clothing);
		}
		else if (clothingRuleType == DressingTerryRuleType.Blacklist)
		{
			blacklistedClothing.AddRange(clothing);
		}

		if (clothingRuleType == DressingTerryRuleType.FromListOnly)
		{
			var clothingFromList = clothing.ToList().Where(x => !blacklistedClothing.Contains(x) && 
																(whitelistedClothing.Count == 0 || whitelistedClothing.Contains(x)));
			if (clothingFromList.Any())
			{
				var entry = new ClothingContainer.ClothingEntry(RandomInList(clothingFromList));
				entry.Tint = tintOverride != null ? tintOverride.Value : Game.Random.Float();
				return entry;
			}
			return null;
		}

		Clothing.ClothingCategory category = Clothing.ClothingCategory.None;
		if (categoriesRuleType == DressingTerryRuleType.FromListOnly)
		{
			if (categories.Any())
			{
				category = RandomInList(categories);
			}
			else
			{
				category = RandomInList(clothingCategories);
			}
		}
		else if (categoriesRuleType == DressingTerryRuleType.Blacklist)
		{			
			var categoriesTypes = clothingCategories.ToList().Where(x => !categories.Contains(x));

			if (categoriesTypes.Any())
			{
				category = RandomInList(categoriesTypes);
			}
			else
			{
				Log.Error($"All Categories were blacklisted");
			}
		}

		string subCategory = null;
		if (subCategoriesRuleType == DressingTerryRuleType.FromListOnly)
		{
			if (subCategories.Any())
			{
				subCategory = RandomInList(subCategories);
			}
			else
			{
				if (clothingCategoryToSubCategory.ContainsKey(category))
				{
					if (clothingCategoryToSubCategory[category].Any())
					{
						subCategory = RandomInList(clothingCategoryToSubCategory[category]);
					}
				}
			}
		}
		else if (subCategoriesRuleType == DressingTerryRuleType.Blacklist)
		{
			if (clothingCategoryToSubCategory.ContainsKey(category))
			{
				if (clothingCategoryToSubCategory[category].Any())
				{
					var subCategoriesTypes = clothingCategoryToSubCategory[category].ToList().Where(x => !subCategories.Contains(x));
					subCategory = RandomInList(subCategoriesTypes);
				}

			}
		}

		var clothingEntry = new ClothingContainer.ClothingEntry(RandomClothing(category, subCategory, currentClothing, whitelistedClothing, blacklistedClothing));
		clothingEntry.Tint = tintOverride != null ? tintOverride.Value : Game.Random.Float();

		return clothingEntry;
	}

	public static T RandomInList<T>(IEnumerable<T> collection)
	{
		if (collection.Any())
		{
			var rndIndex = Game.Random.Int(0, collection.Count()-1);
			return collection.ElementAt(rndIndex);
		}
		return default(T);
	}
}