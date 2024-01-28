using Models;

using SaveEncryption;

using System.Collections.Generic;
using System.Data;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Responsible for managing the different sounds in the game.
/// </summary>
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

#if UNITY_STANDALONE_WIN
    private DataManager _dataManager;
#endif
	private VolumeSettings _volumeSettings;

    /// <summary>
    /// Initializes the file path for saving sound settings and loads the saved settings.
    /// </summary>
    private void OnEnable()
	{
#if UNITY_STANDALONE_WIN
        _dataManager = GameManager.Instance.DataManager;
        LoadSettings();
#endif
	}

	/// <summary>
	/// Sets the volume levels for background music (BGM) and sound effects (SFX) based on slider values.
	/// </summary>
	public void SetVolumeLevel()
    {
        float bgmValue = _bgmSlider.value;
        float volumeModifier = bgmValue != 0 ? (20 * Mathf.Log10(bgmValue)) : -80f;
        _mixer.SetFloat("bgmVol", volumeModifier - 20);
        float sfxValue = _sfxSlider.value;
        volumeModifier = sfxValue != 0 ? (20 * Mathf.Log10(sfxValue)) : -80f;
        _mixer.SetFloat("sfxVol", volumeModifier);
        _volumeSettings.bgm = bgmValue;
        _volumeSettings.sfx = sfxValue;
#if UNITY_STANDALONE_WIN
        SaveVolumeSettings();
#endif
	}

#if UNITY_STANDALONE_WIN
	/// <summary>
	/// Saves the current sound settings.
	/// </summary>
	private void SaveVolumeSettings()
	{
        const string selectQuery =
        @"SELECT *
          FROM volume_settings
          WHERE user_id = $userId;";
        const string updateQuery =
        @"UPDATE volume_settings
          SET bgm = $bgm, sfx = $sfx
          WHERE user_id = $userId;";
        const string insertQuery =
        @"INSERT INTO volume_settings (id, user_id, bgm, sfx)
          VALUES ($id, $userId, $bgm, $sfx);";

        Dictionary<string, object> parameters = new()
        {
            { "$id", EncryptionSystem.EncryptAES(_volumeSettings.id) },
            { "$userId", EncryptionSystem.EncryptAES(_volumeSettings.userID) },
            { "$bgm", _volumeSettings.bgm },
            { "$sfx", _volumeSettings.sfx }
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
    /// Loads previously saved sound settings, if it exists; otherwise, sets default values.
    /// </summary>
    private void LoadSettings()
	{
        _volumeSettings = GetVolumeSettings();

        if (_volumeSettings != null)
        {
            _bgmSlider.value = _volumeSettings.bgm;
            _sfxSlider.value = _volumeSettings.sfx;
        }
        else
        {
            /* Set default values if the file does not exist. */
            _volumeSettings = new(_dataManager.CurrentUser.id);
            _bgmSlider.value = 1f;
            _sfxSlider.value = 1f;
        }
        _dataManager.CurrentUser.volumeSettings = _volumeSettings;
        SetVolumeLevel();
    }

    private VolumeSettings GetVolumeSettings()
    {
        const string query = 
        @"SELECT id, user_id, bgm, sfx
          FROM volume_settings
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
            float bgm = reader.GetFloat(2);
            float sfx = reader.GetFloat(3);
            return new VolumeSettings(id, userId, bgm, sfx);
        }
        else
            return null;
	}
#endif
}