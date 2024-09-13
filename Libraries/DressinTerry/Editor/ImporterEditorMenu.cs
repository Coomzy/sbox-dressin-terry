using Editor;
using Sandbox;
using Sandbox.Internal;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static DressinTerry;
using static Editor.EditorEvent;
using static Sandbox.ClothingContainer;

public static class ImporterEditorMenu
{
	public static string dressingTerryDataFolder => $"{Editor.FileSystem.Root.GetFullPath("/data/coomzy/dressin_terry")}";
	public static string dressingTerryProjectRootFolder = $"{Project.Current.RootDirectory.FullName}/Assets/dressin_terry";
	public static string dressingTerryProjectCharactersFolder = $"{dressingTerryProjectRootFolder}/characters";

	[Menu("Editor", "Library/Dressin Terry/Import All Characters to Project")]
	public static void OpenMyMenu()
	{
		var dressingTerryCharactersFolder = $"{dressingTerryDataFolder}/characters";

		var filePaths = Directory.EnumerateFiles(dressingTerryCharactersFolder, $"*.{extension}", SearchOption.AllDirectories);
		bool hasFailed = false;

		foreach (var filePath in filePaths)
		{
			string relativePath = Path.GetRelativePath(dressingTerryCharactersFolder, filePath);
			string destinationPath = Path.Combine(dressingTerryProjectCharactersFolder, relativePath);

			string destinationDirectory = Path.GetDirectoryName(destinationPath);
			if (!Directory.Exists(destinationDirectory))
			{
				Directory.CreateDirectory(destinationDirectory);
			}

			var fileContents = File.ReadAllText(filePath);
			var characterContainer = ClothingContainer.CreateFromJson(fileContents);

			if (!CreateCharacterFromContainer(characterContainer, destinationPath))
			{
				hasFailed = true;
				Log.Info($"Copied FAILED {filePath} to {destinationPath}");
				continue;
			}

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

	public static bool CreateCharacterFromContainer(ClothingContainer clothingContainer, string destinationPath)
	{
		Log.Info($"CreateCharacterFromContainer destinationPath: {destinationPath}");
		string destinationPathWithoutExtension = destinationPath.Replace(extension, "");
		var characterAsset = AssetSystem.CreateResource(extension, destinationPathWithoutExtension);

		if (!characterAsset.TryLoadResource<DressinTerryCharacter>(out var character))
		{
			Log.Error($"characterAsset: {characterAsset} failed to TryLoadResource");
			return false;
		}

		character.clothingEntries = clothingContainer.Clothing.Select(inst => ClothingInst.Convert(inst)).ToList();
		character.characterHeight = clothingContainer.Height;
		if (!characterAsset.SaveToDisk(character))
		{
			Log.Error($"characterAsset: {characterAsset} failed to save to disk");
			return false;
		}

		AssetSystem.RegisterFile(destinationPath);
		return true;
	}

	[Event("reload"), Hotload]
	static void Register_Event()
	{
		RegisterOnRequestCreateCharacter(DressinTerry_OnRequestCreateCharacter);
	}

	static void DressinTerry_OnRequestCreateCharacter(ClothingContainer clothingContainer, string relativeFilePath)
	{
		string rootFilePath = dressingTerryProjectCharactersFolder;
		string desiredFileName = relativeFilePath ?? "Generated";
		string fileName = desiredFileName;
		string filePath = $"{rootFilePath}/{fileName}.{extension}";

		if (File.Exists(filePath))
		{
			int generatedCount = 0;
			do
			{
				generatedCount++;
				fileName = $"{desiredFileName}_{generatedCount}";
				filePath = $"{rootFilePath}/{fileName}.{extension}";
			}
			while (File.Exists(filePath));
		}

		CreateCharacterFromContainer(clothingContainer, filePath);
	}

	const string DOCS_LINK = "https://sbox.game/coomzy/dressin_terry/news/how-to-use-6358c4a9";
	[Menu("Editor", "Library/Dressin Terry/Documentation")]
	public static void OpenDocs()
	{ 
		try
		{
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(DOCS_LINK) { UseShellExecute = true });
		}
		catch (System.ComponentModel.Win32Exception)
		{
			Log.Error($"Could not open the URL: {DOCS_LINK}");
		}
	}
}