
using Sandbox;
using Sandbox.Citizen;
using System.IO;
using static DressinTerry;

public class PreviewCharacter : Component
{
	public static PreviewCharacter instance { get; private set; }	

    public static List<Clothing> characterClothing = new List<Clothing>();
	public static Dictionary<Clothing, float> clothingToTint = new Dictionary<Clothing, float>();
	public static float characterHeight = 1.0f;

	public static string activeFilePath = "example";

	public static event System.Action OnBuildCharacter;
	public static int dirty;

	[Group("Setup"), Property] public CameraComponent camera { get; private set; }
	[Group("Setup"), Property] public SkinnedModelRenderer bodyRenderer { get; private set; }
	[Group("Setup"), Property] public CitizenAnimationHelper thirdPersonAnimationHelper { get; private set; }

	protected override void OnAwake()
	{
		instance = this;
		characterClothing.Clear();
		dirty = 0;

		base.OnAwake();

		Load(activeFilePath);
		//LoadClothing();

		thirdPersonAnimationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Walk;
		//thirdPersonAnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Pistol;
		//thirdPersonAnimationHelper.Handedness = CitizenAnimationHelper.Hand.Right;
	}

	protected override void OnUpdate()
	{
		CameraMovement();
		RotateCharacter();
	}

	void CameraMovement()
	{
		float forwardMovement = 0.0f;
		forwardMovement += Input.Down("Jump") ? 1.0f : 0.0f;
		forwardMovement -= Input.Down("Duck") ? 1.0f : 0.0f;

		float movementSpeed = Input.Down("Run") ? 125.0f : 50.0f;		

		Vector3 camPos = camera.Transform.Position;
		camPos.x += forwardMovement * Time.Delta * movementSpeed;
		camPos.z += Input.AnalogMove.x * Time.Delta * movementSpeed;
		camPos.y += Input.AnalogMove.y * Time.Delta * movementSpeed;
		camera.Transform.Position = camPos;
	}

	void RotateCharacter()
	{
		float rotateSpeed = 150.0f;
		float rotateAmount = 0.0f;
		if (Input.Down("Menu"))
		{
			rotateAmount -= 1.0f;
		}
		if (Input.Down("use"))
		{
			rotateAmount += 1.0f;
		}

		var angles = GameObject.Transform.Rotation.Angles();
		angles.yaw += rotateAmount * Time.Delta * rotateSpeed;
		GameObject.Transform.Rotation = angles.ToRotation();
	}

	public static void RebuildCharacter()
	{
		instance.bodyRenderer.ClearMaterialOverrides();
		var clothingContainer = GetPreviewClothingContainer();
		clothingContainer.Apply(instance.bodyRenderer);
		if (OnBuildCharacter != null)
		{
			OnBuildCharacter.Invoke();
		}
	}

	public static ClothingContainer GetPreviewClothingContainer()
	{		
		var clothingContainer = new ClothingContainer();
		clothingContainer.Height = characterHeight;
		foreach (var clothing in characterClothing)
		{
			if (clothing == null)
				continue;

			clothingContainer.Toggle(clothing);

			var entry = clothingContainer.FindEntry(clothing);
			if (clothingToTint.TryGetValue(clothing, out float tintValue))
			{
				entry.Tint = tintValue;
			}
		}
		return clothingContainer;
	}

	public static string GetSubCategory(Clothing clothing)
	{
		string subCat = clothing.SubCategory;

		if (string.IsNullOrEmpty(subCat))
		{
			if (clothing.Parent != null)
			{
				subCat = clothing.Parent.SubCategory;
			}
		}
		return string.IsNullOrWhiteSpace(subCat) ? "None" : subCat;
	}

	public static void Save(string filePath)
	{
		//filePath = $"characters/{filePath}";
		filePath = $"characters/{filePath}.{extension}";
		var folder = System.IO.Path.GetDirectoryName(filePath);
		Log.Info($"Save() filePath: {filePath}, folder: {folder}");
		if (!string.IsNullOrWhiteSpace(folder) && !FileSystem.Data.DirectoryExists(folder))
		{
			FileSystem.Data.CreateDirectory(folder);
		}
		FileSystem.Data.WriteAllText(filePath, GetPreviewClothingContainer().Serialize());
		
	}

	public static void Load(string filePath)
	{
		Log.Info($"Load() filePath: {filePath}");
		filePath = $"characters/{filePath}.{extension}";
		if (!FileSystem.Data.FileExists(filePath))
		{
			return;
		}
		var characterJson = FileSystem.Data.ReadAllText(filePath);
		var clothingContainer = new ClothingContainer();
		clothingContainer.Deserialize(characterJson);

		characterHeight = clothingContainer.Height;
		characterClothing.Clear();
		clothingToTint.Clear();

		foreach (var clothingEntry in clothingContainer.Clothing)
		{
			var clothing = clothingEntry.Clothing;
			if (!characterClothing.Contains(clothing))
			{
				characterClothing.Add(clothing);
			}

			float tintValue = clothingEntry.Tint != null ? clothingEntry.Tint.Value : clothing.TintDefault;
			clothingToTint[clothing] = tintValue;
		}
		RebuildCharacter();
	}
}