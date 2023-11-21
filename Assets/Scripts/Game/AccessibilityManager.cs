using System.IO;

using UnityEngine;
using UnityEngine.UI;

public class AccessibilityManager : MonoBehaviour
{
	/* https://www.youtube.com/watch?v=-ewIt4v_hjw&list=PLobY7vO0pgVIOZNKTVRhkPzrfCjDJ0CNl&index=124 */
	[SerializeField] private Toggle reducedMobilityToggle;
	[SerializeField] private Toggle oneHandedToggle;
    public static bool ReducedMobilityMode { get; set; }
    public static bool OneHandedMode { get; set; }

	private string _filePath;

	private void Start()
	{
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
		reducedMobilityToggle.isOn = ReducedMobilityMode;
		oneHandedToggle.isOn = OneHandedMode;
	}
}

[System.Serializable]
public class AccessibilitySettings
{
	public bool hasReducedMobility;
	public bool isOneHanded;
}
