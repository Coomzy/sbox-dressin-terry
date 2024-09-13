using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public enum DresserType
{
	Character,
	Rule,
	Random,
}

[Group("Dressin Terry"), Title("Terry Dresser")]
public class TerryDresser : Component
{
	[Group("Setup"), Order(-1), Property] public SkinnedModelRenderer bodyRenderer { get; set; }

	[Group("Config"), Property] public bool applyOnAwake { get; set; }
	[Group("Config"), Property, Change(nameof(Change_dresserType))] public DresserType dresserType { get; set; }
	[Group("Config"), Property, ShowIf("dresserType", DresserType.Character), Change(nameof(Change_character)), InlineEditor] public DressinTerryCharacter character { get; set; }
	[Group("Config"), Property, ShowIf("dresserType", DresserType.Rule), Change(nameof(Change_rule)), InlineEditor] public DressinTerryRule rule { get; set; }

	ClothingContainer lastClothingContainer { get; set; }

	protected override void OnAwake()
	{
		if (!applyOnAwake)
			return;

		Apply();
	}

	[Group("Apply"), Order(10), Button]
	public void Apply()
	{
		switch (dresserType)
		{
			case DresserType.Character:
				Apply_Character();
				break;
			case DresserType.Rule:
				Apply_Rule();
				break;
			case DresserType.Random:
				Apply_Random();
				break;
		}
	}

	void Change_dresserType(DresserType oldValue, DresserType newValue)
	{
		Apply();
	}

	void Change_character(DressinTerryCharacter oldValue, DressinTerryCharacter newValue)
	{
		if (dresserType != DresserType.Character)
			return;

		Apply_Character();
	}

	public void Apply_Character()
	{
		if (bodyRenderer == null || !bodyRenderer.IsValid)
		{
			Log.Error($"Tried to apply character '{GameObject}' to a null bodyRenderer!");
			return;
		}
		lastClothingContainer = DressinTerryCharacter.ToClothingContainer(character);
		//bodyRenderer.ApplyClothing(lastClothingContainer);
		DressinTerry.ApplyClothing(bodyRenderer, lastClothingContainer);
	}

	void Change_rule(DressinTerryRule oldValue, DressinTerryRule newValue)
	{
		if (dresserType != DresserType.Rule)
			return;

		Apply_Rule();
	}

	public void Apply_Rule()
	{
		if (bodyRenderer == null || !bodyRenderer.IsValid)
		{
			Log.Error($"Tried to apply rule '{GameObject}' to a null bodyRenderer!");
			return;
		}
		lastClothingContainer = DressinTerryRule.ToClothingContainer(rule);
		//bodyRenderer.ApplyClothing(lastClothingContainer);
		DressinTerry.ApplyClothing(bodyRenderer, lastClothingContainer);
	}

	public void Apply_Random()
	{
		if (bodyRenderer == null || !bodyRenderer.IsValid)
		{
			Log.Error($"Tried to apply random '{GameObject}' to a null bodyRenderer!");
			return;
		}
		lastClothingContainer = DressinTerry.RandomCharacter();
		//bodyRenderer.ApplyClothing(lastClothingContainer);
		DressinTerry.ApplyClothing(bodyRenderer, lastClothingContainer);
	}

	[Group("Creation"), Order(80), Button, ShowIf("dresserType", DresserType.Rule)]
	public void CreateLastUsedClothingContainer()
	{
		if (lastClothingContainer == null)
		{
			Log.Warning($"lastClothingContainer is null, it doesn't serialize so if you switch scene or something it's gone!");
			return;
		}
		DressinTerry.RequestCharacterCreation(lastClothingContainer, rule?.ResourceName);
	}

	[Group("Debug"), Order(100), Button]
	public void DumpLastUsedClothingContainer()
	{
		Log.Info($"DumpLastUsedClothingContainer()");
		if (lastClothingContainer == null)
		{
			Log.Warning($"lastClothingContainer is null");
			return;
		}
		Log.Info($"height: {lastClothingContainer.Height}");
		foreach (var clothingEntry in lastClothingContainer.Clothing)
		{
			Log.Info($"clothingEntry '{clothingEntry.Clothing.ResourceName}' tint: {clothingEntry.Tint}");
		}
	}

	public override void Reset()
	{
		base.Reset();

		bodyRenderer = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>();
	}
}