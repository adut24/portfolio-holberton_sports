using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
	[SerializeField] private AudioMixer _mixer;
    public Slider BGMSlider { get; set; }
    public Slider SFXSldier { get; set; }
    private string _filePath;

	private void Start()
	{
		foreach (Slider slider in GameManager.Instance.GetComponent<PauseMenuManager>().PauseMenu.GetComponentsInChildren<Slider>(true))
		{
			if (slider.name == "BGMSlider")
				BGMSlider = slider;
			else
				SFXSldier = slider;
		}
		_filePath = Path.Combine(Application.persistentDataPath, "sound.json");
		LoadSoundSettings();
	}
	public void SetVolumeLevel()
	{
		float bgmValue = BGMSlider.value;
		float volumeModifier = bgmValue != 0 ? (20 * Mathf.Log10(bgmValue)) : -80f;
		_mixer.SetFloat("bgmVol", volumeModifier - 20);
		float sfxValue = SFXSldier.value;
		volumeModifier = sfxValue != 0 ? (20 * Mathf.Log10(sfxValue)) : -80f;
		_mixer.SetFloat("sfxVol", volumeModifier - 25); 
		SaveSoundSettings();
	}

	private void SaveSoundSettings()
	{
		SoundSettings soundSettings = new() 
		{
			bgmVolume = BGMSlider.value,
			sfxVolume = SFXSldier.value
		};
		File.WriteAllText(_filePath, JsonUtility.ToJson(soundSettings));
	}

	private void LoadSoundSettings()
	{
		if (File.Exists(_filePath))
		{
			SoundSettings soundSettings = JsonUtility.FromJson<SoundSettings>(File.ReadAllText(_filePath));
			BGMSlider.value = soundSettings.bgmVolume;
			SFXSldier.value = soundSettings.sfxVolume;
		}
		else
		{
			BGMSlider.value = 1f;
			SFXSldier.value = 1f;
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
