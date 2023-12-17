using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

public class NetworkManager : MonoBehaviourPunCallbacks
{
	public string Game { get; set; }
	public GameObject PauseMenu { get; set; }

	[SerializeField] private GameObject _playerPrefab;

	private GameManager _gameManager;
	private GameObject _player;

	public override void OnJoinedRoom() => StartCoroutine(LoadPlayer());

	public override void OnLeftRoom()
	{
		Destroy(GameManager.Instance.gameObject);
		SceneManager.LoadScene(1);
	}

	public IEnumerator LoadPlayer()
	{
		PhotonNetwork.LoadLevel(Game);
		_gameManager = GameManager.Instance;

		while (PhotonNetwork.LevelLoadingProgress < 1)
			yield return null;

		Transform spawnPoint = PhotonNetwork.IsMasterClient ? GameObject.Find("Spawn Point Player 1").transform : GameObject.Find("Spawn Point Player 2").transform;
		_player = PhotonNetwork.Instantiate(_playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
		GameManager.Instance.Player = _player;

		FadeScreenManager fadeScreenManager = _player.transform.GetComponentInChildren<FadeScreenManager>();
		fadeScreenManager.enabled = true;
		yield return null;

		_gameManager.PauseMenuManager.PauseMenu = PauseMenu;
		PauseMenu.transform.SetParent(_player.transform, false);

		GameObject tutorial = GameObject.FindGameObjectsWithTag("Tutorial").FirstOrDefault(obj => obj.transform.parent == null);

		if (tutorial != null)
			tutorial.transform.SetParent(_player.transform, false);

		PrepareGameManager(fadeScreenManager);

		fadeScreenManager.FadeIn();
		yield return new WaitForSeconds(fadeScreenManager.FadeDuration);
	}

	private void PrepareGameManager(FadeScreenManager fadeScreen)
	{
		switch (Game)
		{
			case "Bowling":
				BowlingManager manager = _gameManager.gameObject.AddComponent<BowlingManager>();
				manager.FadeScreen = fadeScreen;
				manager.PauseManager = GameManager.Instance.PauseMenuManager;
				manager.ReplayMenu = GameObject.FindGameObjectsWithTag("Replay").FirstOrDefault(obj => obj.transform.parent == null);
				GameObject replayMenu = manager.ReplayMenu;
				if (replayMenu != null)
				{
					replayMenu.transform.SetParent(_player.transform, false);
					replayMenu.SetActive(false);
				}
				break;
		}
		_gameManager.PauseMenuManager.InteractorManager = _player.GetComponent<InteractorManager>();
	}
}
