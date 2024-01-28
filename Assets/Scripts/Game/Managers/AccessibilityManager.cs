using Models;

using SaveEncryption;

using System.Collections.Generic;
using System.Data;
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

#if UNITY_STANDALONE_WIN
    private DataManager _dataManager;
#endif
    private AccessibilitySettings _accessibilitySettings;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    private void OnEnable()
	{
#if UNITY_STANDALONE_WIN
        _dataManager = GameManager.Instance.DataManager;
        LoadSettings();
#endif
	}

#if UNITY_STANDALONE_WIN
	/// <summary>
	/// Saves the current accessibility settings to a JSON file.
	/// </summary>
	public void SaveSettings()
    {
        const string selectQuery =
        @"SELECT *
          FROM accessibility_settings
          WHERE user_id = $userId;";
        const string updateQuery =
        @"UPDATE accessibility_settings
          SET reduced_mobility = $mobility, one_handed = $hand
          WHERE user_id = $userId;";
        const string insertQuery =
        @"INSERT INTO accessibility_settings (id, user_id, reduced_mobility, one_handed)
          VALUES ($id, $userId, $mobility, $hand);";

        Dictionary<string, object> parameters = new()
        {
            { "$id", EncryptionSystem.EncryptAES(_accessibilitySettings.id) },
            { "$userId", EncryptionSystem.EncryptAES(_accessibilitySettings.userID) },
            { "$mobility", ReducedMobilityMode },
            { "$hand", OneHandedMode }
        };

        using IDataReader reader = _dataManager.ExecuteQuery(selectQuery, parameters, true);
        if (reader.Read())
        {
            /* If the row exists, update it */
            _dataManager.ExecuteQuery(updateQuery, parameters);
        }
        else
        {
            /* If the row doesn't exist, insert it */
            _dataManager.ExecuteQuery(insertQuery, parameters);
        }
    }

    /// <summary>
    /// Loads accessibility settings from the JSON file.
    /// </summary>
    private void LoadSettings()
    {
        _accessibilitySettings = GetAccessibilitySettings();

        if (_accessibilitySettings != null)
        {
            ReducedMobilityMode = _accessibilitySettings.hasReducedMobility;
            OneHandedMode = _accessibilitySettings.isOneHanded;
        }
        else
            _accessibilitySettings = new(_dataManager.CurrentUser.id);
        _reducedMobility.isOn = ReducedMobilityMode;
        _oneHanded.isOn = OneHandedMode;
        _dataManager.CurrentUser.accessibilitySettings = _accessibilitySettings;
        SaveSettings();
	}

    private AccessibilitySettings GetAccessibilitySettings()
    {
        const string query =
        @"SELECT id, user_id, reduced_mobility, one_handed
          FROM accessibility_settings
          WHERE user_id = $userId;";

        Dictionary<string, object> parameters = new()
        {
            { "$userId", EncryptionSystem.EncryptAES(_dataManager.CurrentUser.id) }
        };
        using IDataReader reader = _dataManager.ExecuteQuery(query, parameters, true);
        if (reader.Read())
        {
            string id = EncryptionSystem.DecryptAES(reader.GetString(0));
            string userId = EncryptionSystem.DecryptAES(reader.GetString(1));
            bool hasReducedMobility = reader.GetBoolean(2);
            bool isOneHanded = reader.GetBoolean(3);
            return new AccessibilitySettings(id, userId, hasReducedMobility, isOneHanded);
        }
        else
            return null;
    }
#endif
}
