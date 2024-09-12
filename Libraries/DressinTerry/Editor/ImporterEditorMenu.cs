using Editor;
using Sandbox;
using Sandbox.Internal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static DressinTerry;
using static Sandbox.ClothingContainer;

public static class ImporterEditorMenu
{
	public static string dressingTerryDataFolder => $"{Editor.FileSystem.Root.GetFullPath("/data/local/dressin_terry")}";

	[Menu("Editor", "Library/Dressin Terry/Import All Characters to Project")]
	public static void OpenMyMenu()
	{
		var dressingTerryCharactersFolder = $"{dressingTerryDataFolder}/characters";
		var dressingTerryProjectRootFolder = $"{Project.Current.RootDirectory.FullName}/Assets/dressin_terry/characters";

		var filePaths = Directory.EnumerateFiles(dressingTerryCharactersFolder, $"*.{extension}", SearchOption.AllDirectories);
		bool hasFailed = false;

		foreach (var filePath in filePaths)
		{
			string relativePath = Path.GetRelativePath(dressingTerryCharactersFolder, filePath);
			string destinationPath = Path.Combine(dressingTerryProjectRootFolder, relativePath);
			string destinationPathWithoutExtension = destinationPath.Replace(extension, "");

			string destinationDirectory = Path.GetDirectoryName(destinationPath);
			if (!Directory.Exists(destinationDirectory))
			{
				Directory.CreateDirectory(destinationDirectory);
			}

			var fileContents = File.ReadAllText(filePath);
			var characterContainer = ClothingContainer.CreateFromJson(fileContents);

			var characterAsset = AssetSystem.CreateResource(extension, destinationPathWithoutExtension);

			if (!characterAsset.TryLoadResource<DressinTerryCharacter>(out var character))
			{
				Log.Error($"characterAsset: {characterAsset} failed to TryLoadResource");
				hasFailed = true;
				continue;
			}

			character.clothingEntries = characterContainer.Clothing.Select(inst => ClothingInst.Convert(inst)).ToList();
			character.characterHeight = characterContainer.Height;
			characterAsset.SaveToDisk(character);

			AssetSystem.RegisterFile(destinationPath);

			Log.Info($"Copied {filePath} to {destinationPath}");
		}

		if (EditorPreferences.NotificationSounds)
		{
			if (hasFailed)
			{
				EditorUtility.PlayRawSound("sounds/editor/fail.wav");
			}
			else
			{
				EditorUtility.PlayRawSound("sounds/editor/success.wav");
			}
		}
	}
}
