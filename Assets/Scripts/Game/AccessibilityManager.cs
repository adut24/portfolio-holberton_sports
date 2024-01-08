using System;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Responsible for managing the accessibility options of the game.
/// </summary>
public class AccessibilityManager : MonoBehaviour
{
    /// <summary>
    /// Gets or sets the option of reduced mobility. If activated, it gives a "laser" to the player to grab the interactable objects from afar.
    /// </summary>
    public bool ReducedMobilityMode { get; set; }

    /// <summary>
    /// Gets or sets the option to play with only one hand. What it does depends on the sport played.
    /// </summary>
    public bool OneHandedMode { get; set; }

    [SerializeField] private Toggle _reducedMobility;
    [SerializeField] private Toggle _oneHanded;

    private string _filePath;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        _filePath = Path.Combine(Application.persistentDataPath, "accessilibity.json");
        LoadSettings();
    }

    /// <summary>
    /// Saves the current accessibility settings to a JSON file.
    /// </summary>
    public void SaveSettings()
    {
        AccessibilitySettings accessibilitySettings = new()
        {
            hasReducedMobility = ReducedMobilityMode,
            isOneHanded = OneHandedMode
        };
        File.WriteAllText(_filePath, JsonUtility.ToJson(accessibilitySettings));
    }

    /// <summary>
    /// Loads accessibility settings from the JSON file.
    /// </summary>
    private void LoadSettings()
    {
        if (File.Exists(_filePath))
        {
            AccessibilitySettings accessibilitySettings = JsonUtility.FromJson<AccessibilitySettings>(File.ReadAllText(_filePath));
            ReducedMobilityMode = accessibilitySettings.hasReducedMobility;
            OneHandedMode = accessibilitySettings.isOneHanded;
        }
        _reducedMobility.isOn = ReducedMobilityMode;
        _oneHanded.isOn = OneHandedMode;
    }
}

/// <summary>
/// Represents accessibility settings for the game.
/// </summary>
[Serializable]
public class AccessibilitySettings
{
    /// <summary>
    /// Tells whether the "Reduced Mobility Mode" is enabled.
    /// </summary>
    public bool hasReducedMobility;

    /// <summary>
    /// Tells whether the "One-Handed Mode" is enabled.
    /// </summary>
    public bool isOneHanded;
}
