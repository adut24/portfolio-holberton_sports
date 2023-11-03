using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
	[SerializeField] private AudioMixer _mixer;
	[SerializeField] private Slider _bgmSlider;
	[SerializeField] private Slider _sfxSlider;
	private float _storedBGMVolume;
	private float _storedSFXVolume;
	private string _filePath;

	private void Start()
	{
		_filePath = Path.Combine(Application.persistentDataPath, "sound.json");
		LoadSoundSettings();
		float volumeModifier = _storedBGMVolume != 0 ? (20 * Mathf.Log10(_storedBGMVolume)) : -80f;
		_mixer.SetFloat("bgmVol", volumeModifier - 20);
		volumeModifier = _storedSFXVolume != 0 ? (20 * Mathf.Log10(_storedSFXVolume)) : -80f;
		_mixer.SetFloat("sfxVol", volumeModifier - 25);
		SetVolumeLevel();
	}

	public void ApplySoundSettings() => SaveSoundSettings();

	public void SetVolumeLevel()
	{
		float bgmValue = _bgmSlider.value;
		float volumeModifier = bgmValue != 0 ? (20 * Mathf.Log10(bgmValue)) : -80f;
		_mixer.SetFloat("bgmVol", volumeModifier - 20);
		float sfxValue = _sfxSlider.value;
		volumeModifier = sfxValue != 0 ? (20 * Mathf.Log10(sfxValue)) : -80f;
		_mixer.SetFloat("sfxVol", volumeModifier - 25);
	}

	public void ResetVolumeLevel()
	{
		_bgmSlider.value = _storedBGMVolume;
		float volumeModifier = _storedBGMVolume != 0 ? (20 * Mathf.Log10(_storedBGMVolume)) : -80f;
		_mixer.SetFloat("bgmVol", volumeModifier - 20);
		_sfxSlider.value = _storedSFXVolume;
		volumeModifier = _storedSFXVolume != 0 ? (20 * Mathf.Log10(_storedSFXVolume)) : -80f;
		_mixer.SetFloat("sfxVol", volumeModifier - 25);
	}

	private void SaveSoundSettings()
	{
		_storedBGMVolume = _bgmSlider.value;
		_storedSFXVolume = _sfxSlider.value;

		SoundSettings soundSettings = new SoundSettings() 
		{
			bgmVolume = _bgmSlider.value,
			sfxVolume = _sfxSlider.value
		};
		File.WriteAllText(_filePath, JsonUtility.ToJson(soundSettings));
	}

	private void LoadSoundSettings()
	{

		if (File.Exists(_filePath))
		{
			SoundSettings soundSettings = JsonUtility.FromJson<SoundSettings>(File.ReadAllText(_filePath));

			_storedBGMVolume = soundSettings.bgmVolume;
			_storedSFXVolume = soundSettings.sfxVolume;
			_bgmSlider.value = _storedBGMVolume;
			_sfxSlider.value = _storedSFXVolume;
		}
		else
		{
			_storedBGMVolume = 1f;
			_storedSFXVolume = 1f;
			_bgmSlider.value = _storedBGMVolume;
			_sfxSlider.value = _storedSFXVolume;
		}
		SetVolumeLevel();
	}
}

[System.Serializable]
public class SoundSettings
{
	public float bgmVolume;
	public float sfxVolume;
}
