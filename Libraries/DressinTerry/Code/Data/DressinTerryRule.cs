
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

	[Group("Clothing"), Order(2), Property] public List<Clothing> clothingWhiteList { get; set; } = new List<Clothing>();
	[Group("Clothing"), Order(2), Property] public List<Clothing> clothingBlacklsit { get; set; } = new List<Clothing>();

	[Group("Rules"), Order(3), Property, InlineEditor] public List<RuleInst> rules { get; set; } = new List<RuleInst>();

	public static ClothingContainer ToClothingContainer(DressinTerryRule rule)
	{
		var clothingContainer = new ClothingContainer();
		if (rule != null)
		{
			var rulesReversed = rule.rules.ToList();
			//rulesReversed.Reverse();
			List<Clothing> wearingClothing = new List<Clothing>();
			foreach (var ruleInst in rulesReversed)
			{
				if (ruleInst.chanceOf != null)
				{
					var rndChance = Game.Random.Float();

					if (rndChance > ruleInst.chanceOf)
						continue;
				}

				var entry = ruleInst.GetRandomEntry(wearingClothing);
				if (entry == null)
				{
					continue;
				}

				clothingContainer.Toggle(entry.Clothing);

				var entryInst = clothingContainer.FindEntry(entry.Clothing);
				if (entryInst != null)
				{
					entryInst.Tint = entry.Tint;
				}

				wearingClothing.Add(entry.Clothing);
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
		bodyRenderer.ApplyClothing(ToClothingContainer(this));
	}
}

public enum RuleType
{
	Any,
	FromListOnly,
	Blacklist
}

public class RuleInst
{
	[Group(/*"Clothing"*/"Rule"), Order(1), Property] public RuleType clothingRuleType { get; set; }
	[Group(/*"Clothing"*/"Rule"), Order(2), Property, ShowIf(nameof(shouldShowClothing), true)] public List<Clothing> clothing { get; set; } = new List<Clothing>();

	[Group(/*"Category"*/"Rule"), Order(3), Property, ShowIf(nameof(shouldShowCategories), true)] public RuleType categoriesRuleType { get; set; }
	[Group(/*"Category"*/"Rule"), Order(4), Property, ShowIf(nameof(shouldShowCategoriesList), true)] public List<Clothing.ClothingCategory> categories { get; set; } = new List<Clothing.ClothingCategory>();

	[Group(/*"Sub Category"*/"Rule"), Order(5), Property, ShowIf(nameof(shouldShowSubCategories), true)] public RuleType subCategoriesRuleType { get; set; }
	[Group(/*"Sub Category"*/"Rule"), Order(6), Property, ShowIf(nameof(shouldShowSubCategoriesList), true)] public List<string> subCategories { get; set; } = new List<string>();

	[Group(/*"Tint"*/"Rule"), Order(10), Property, Range(0, 1)] public float? tintOverride { get; set; }
	[Group(/*"Tint"*/"Rule"), Order(11), Property, Range(0, 1)] public float? chanceOf { get; set; }

	[Hide] public bool shouldShowClothing => clothingRuleType != RuleType.Any;
	[Hide] public bool shouldShowCategories => clothingRuleType != RuleType.FromListOnly;
	[Hide] public bool shouldShowCategoriesList => shouldShowCategories && categoriesRuleType != RuleType.Any;
	[Hide] public bool shouldShowSubCategories => clothingRuleType != RuleType.FromListOnly;
	[Hide] public bool shouldShowSubCategoriesList => shouldShowCategories && subCategoriesRuleType != RuleType.Any;

	public ClothingContainer.ClothingEntry GetRandomEntry(List<Clothing> wearingClothing = null)
	{
		var invalidClothing = new List<Clothing>(wearingClothing);
		if (clothingRuleType == RuleType.FromListOnly)
		{
			var clothingFromList = clothing.ToList().Where(x => !invalidClothing.Contains(x));
			if (clothingFromList.Any())
			{
				var entry = new ClothingContainer.ClothingEntry(RandomInList(clothingFromList));
				entry.Tint = tintOverride != null ? tintOverride.Value : Game.Random.Float();
				return entry;
			}
			return null;
		}

		Clothing.ClothingCategory category = Clothing.ClothingCategory.None;
		if (categoriesRuleType == RuleType.FromListOnly)
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
		else if (categoriesRuleType == RuleType.Blacklist)
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
		if (subCategoriesRuleType == RuleType.FromListOnly)
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
		else if (subCategoriesRuleType == RuleType.Blacklist)
		{
			if (clothingCategoryToSubCategory.ContainsKey(category))
			{
				var list = clothingCategoryToSubCategory[category];

				if (list.Any())
				{
					var subCategoriesTypes = list.ToList().Where(x => !subCategory.Contains(x));
					subCategory = RandomInList(subCategoriesTypes);
				}

			}
		}

		var clothingEntry = new ClothingContainer.ClothingEntry(RandomClothing(category, subCategory, invalidClothing));
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