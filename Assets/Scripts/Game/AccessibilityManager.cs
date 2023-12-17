using System.IO;

using UnityEngine;
using UnityEngine.UI;

public class AccessibilityManager : MonoBehaviour
{
	public Toggle ReducedMobilityToggle { get; set; }
	public Toggle OneHandedToggle { get; set; }
	public bool ReducedMobilityMode { get; set; }
	public bool OneHandedMode { get; set; }

	[SerializeField] private Toggle _reducedMobility;
	[SerializeField] private Toggle _oneHanded;

	private string _filePath;

	private void Awake()
	{
		ReducedMobilityToggle = _reducedMobility;
		OneHandedToggle = _oneHanded;
		_filePath = Path.Combine(Application.persistentDataPath, "accessilibity.json");
		LoadSettings();
	}

	public void SaveSettings()
	{
		AccessibilitySettings accessibilitySettings = new()
		{
			hasReducedMobility = ReducedMobilityMode,
			isOneHanded = OneHandedMode
		};
		File.WriteAllText(_filePath, JsonUtility.ToJson(accessibilitySettings));
	}

	private void LoadSettings()
	{
		if (File.Exists(_filePath))
		{
			AccessibilitySettings accessibilitySettings = JsonUtility.FromJson<AccessibilitySettings>(File.ReadAllText(_filePath));
			ReducedMobilityMode = accessibilitySettings.hasReducedMobility;
			OneHandedMode = accessibilitySettings.isOneHanded;
		}
		ReducedMobilityToggle.isOn = ReducedMobilityMode;
		OneHandedToggle.isOn = OneHandedMode;
	}
}

[System.Serializable]
public class AccessibilitySettings
{
	public bool hasReducedMobility;
	public bool isOneHanded;
}
