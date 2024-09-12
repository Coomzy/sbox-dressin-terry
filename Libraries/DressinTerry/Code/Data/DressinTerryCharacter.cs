
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

[GameResource("Character", DressinTerry.extension, "A Dressin Terry Character", Icon = "person")]
public class DressinTerryCharacter : GameResource
{
	[InlineEditor] public List<ClothingInst> clothingEntries { get; set; } = new List<ClothingInst>();
	public float characterHeight { get; set; } = 1.0f;

	public static ClothingContainer ToClothingContainer(DressinTerryCharacter character)
	{
		var clothingContainer = new ClothingContainer();
		if (character != null)
		{
			clothingContainer.Clothing = character.clothingEntries.Select(inst => (ClothingContainer.ClothingEntry)inst).ToList();
			clothingContainer.Height = character.characterHeight;
		}
		return clothingContainer;
	}

	public void Apply(SkinnedModelRenderer bodyRenderer)
	{
		if (!bodyRenderer.IsValid)
		{
			Log.Error($"Tried to apply character '{this}' to a null bodyRenderer!");
			return;
		}
		bodyRenderer.ApplyClothing(ToClothingContainer(this));
	}
}

public class ClothingInst
{
	[Group("Clothing"), Property] public Clothing clothing { get; set; }
	[Group("Tinting"), Property, Range(0, 1)] public float? tintSelection { get; set; }

	public ClothingInst(Clothing clothing, float? tintSelection = null)
	{
		this.clothing = clothing;
		this.tintSelection = tintSelection;
	}

	public static implicit operator ClothingContainer.ClothingEntry(ClothingInst inst)
	{
		return new ClothingContainer.ClothingEntry(inst.clothing)
		{
			Tint = inst.tintSelection
		};
	}

	public static ClothingInst Convert(ClothingContainer.ClothingEntry clothingEntry)
	{
		return new ClothingInst(clothingEntry.Clothing, clothingEntry.Tint);
	}
}