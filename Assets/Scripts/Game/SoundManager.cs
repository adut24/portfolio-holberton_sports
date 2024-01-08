using System;
using System.IO;

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

    private string _filePath;

    /// <summary>
    /// Initializes the file path for saving sound settings and loads the saved settings.
    /// </summary>
    private void Awake()
    {
        _filePath = Path.Combine(Application.persistentDataPath, "sound.json");
        LoadSoundSettings();
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
        SaveSoundSettings();
    }

    /// <summary>
    /// Saves the current sound settings to a JSON file.
    /// </summary>
    private void SaveSoundSettings()
    {
        SoundSettings soundSettings = new() 
        {
            bgmVolume = _bgmSlider.value,
            sfxVolume = _sfxSlider.value
        };
        File.WriteAllText(_filePath, JsonUtility.ToJson(soundSettings));
    }

    /// <summary>
    /// Loads previously saved sound settings from the JSON file, if it exists; otherwise, sets default values.
    /// </summary>
    private void LoadSoundSettings()
    {
        if (File.Exists(_filePath))
        {
            SoundSettings soundSettings = JsonUtility.FromJson<SoundSettings>(File.ReadAllText(_filePath));
            _bgmSlider.value = soundSettings.bgmVolume;
            _sfxSlider.value = soundSettings.sfxVolume;
        }
        else
        {
            /* Set default values if the file does not exist. */
            _bgmSlider.value = 1f;
            _sfxSlider.value = 1f;
        }
        SetVolumeLevel();
    }
}

/// <summary>
/// Represents the sound settings, including background music (BGM) and sound effects (SFX) volume.
/// </summary>
[Serializable]
public class SoundSettings
{
    /// <summary>
    /// Represents the volume level for background music (BGM).
    /// </summary>
    public float bgmVolume;

    /// <summary>
    /// Represents the volume level for sound effects (SFX).
    /// </summary>
    public float sfxVolume;
}
