using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
	[SerializeField] private GameObject _playerPrefab;
	public static string Game { get; set; }

	public override void OnJoinedRoom()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			StartCoroutine(LoadPlayerOne());
		}
		else
		{

		}
	}

	private IEnumerator LoadPlayerOne()
	{
		PhotonNetwork.LoadLevel(Game);

		while (PhotonNetwork.LevelLoadingProgress < 1)
			yield return null;

		Transform spawnPoint = GameObject.Find("Spawn Point Player 1").transform;
		GameObject player = PhotonNetwork.Instantiate(_playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
		player.name = "Player";

		FadeScreenManager fadeScreen = player.transform.GetComponentInChildren<FadeScreenManager>();
		fadeScreen.enabled = true;
		yield return null;
		InteractorManager interactorManager = player.GetComponent<InteractorManager>();
		interactorManager.ToggleMenuBehavior();

		switch (Game)
		{
			case "Bowling":
				BowlingManager manager = GameManager.Instance.gameObject.AddComponent<BowlingManager>();
				manager.FadeScreen = fadeScreen;
				break;
		}

		PauseMenuManager pauseMenu = GameManager.Instance.gameObject.GetComponent<PauseMenuManager>();
		pauseMenu.PauseMenu = player.GetComponentInChildren<Canvas>(true).gameObject;

		AccessibilityManager accessibilityManager = GameManager.Instance.gameObject.GetComponent<AccessibilityManager>();
		SoundManager soundManager = GameManager.Instance.gameObject.GetComponent<SoundManager>();

		foreach (Toggle toggle in pauseMenu.PauseMenu.GetComponentsInChildren<Toggle>(true))
		{
			if (toggle.name == "ReducedMobilityToggle")
				accessibilityManager.ReducedMobilityToggle = toggle;
			else
				accessibilityManager.OneHandedToggle = toggle;
		}

		foreach (Slider slider in pauseMenu.PauseMenu.GetComponentsInChildren<Slider>(true))
		{
			if (slider.name == "BGMSlider")
				soundManager.BGMSlider = slider;
			else
				soundManager.SFXSldier = slider;
		}

		fadeScreen.FadeIn();
		yield return new WaitForSeconds(fadeScreen.FadeDuration);
	}
}
