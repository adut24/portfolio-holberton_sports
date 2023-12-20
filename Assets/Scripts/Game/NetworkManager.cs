using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
	public string Game { get; set; }
	public string RoomCode { get; set; }
	public int NumberPlayers { get; set; }
	public bool IsCreator { get; set; }
	public GameObject PauseMenu { get; set; }

	[SerializeField] private GameObject _playerPrefab;
	[SerializeField] private GameObject _player;
	[SerializeField] private Transform _roomListContent;
	[SerializeField] private GameObject _roomButton;

	private GameManager _gameManager;

	public override void OnConnectedToMaster()
	{
		if (!PhotonNetwork.OfflineMode)
			PhotonNetwork.JoinLobby();
		else
			PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = NumberPlayers, });
	}

	public override void OnJoinedLobby()
	{
		if (IsCreator)
			PhotonNetwork.CreateRoom(RoomCode,
									 new RoomOptions
									 {
										 MaxPlayers = NumberPlayers,
										 IsVisible = true,
										 IsOpen = true
									 },
									 TypedLobby.Default);
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		foreach (Transform child in _roomListContent)
			Destroy(child.gameObject);

		foreach (RoomInfo room in roomList)
		{
			if (room.PlayerCount != room.MaxPlayers)
			{
				GameObject roomButton = Instantiate(_roomButton, _roomListContent);
				roomButton.GetComponentInChildren<TextMeshPro>().text = room.Name;
				roomButton.GetComponent<Button>().onClick.AddListener(() => Connect(room.Name));
			}
		}
	}

	public override void OnJoinedRoom() => StartCoroutine(LoadPlayer());

	public override void OnLeftRoom()
	{
		PhotonNetwork.Destroy(GameManager.Instance.gameObject);
		if (PhotonNetwork.IsMasterClient)
			PhotonNetwork.DestroyAll();
		SceneManager.LoadScene(1);
		if (!PhotonNetwork.OfflineMode)
		{
			PhotonNetwork.LeaveLobby();
			PhotonNetwork.Disconnect();
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
	}

	public void Connect(string roomName) => StartCoroutine(LoadScene(roomName));

	private IEnumerator LoadScene(string roomName)
	{
		FadeScreenManager fade = _player.GetComponentInChildren<FadeScreenManager>();
		fade.FadeOut();
		yield return new WaitForSeconds(fade.FadeDuration);

		PhotonNetwork.JoinRoom(roomName);
	}

	public IEnumerator LoadPlayer()
	{
		PhotonNetwork.LoadLevel(Game);
		_gameManager = GameManager.Instance;
		PhotonView photonView = _gameManager.gameObject.GetPhotonView();
		photonView.ViewID = 0;
		PhotonNetwork.AllocateViewID(photonView);

		while (PhotonNetwork.LevelLoadingProgress < 1)
			yield return null;

		Transform spawnPoint = PhotonNetwork.IsMasterClient ? GameObject.Find("Spawn Point Player 1").transform : GameObject.Find("Spawn Point Player 2").transform;
		_player = PhotonNetwork.Instantiate(_playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
		photonView = _player.GetPhotonView();
		photonView.ViewID = 0;
		PhotonNetwork.AllocateViewID(photonView);
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

		if (!PhotonNetwork.OfflineMode)
			GameObject.Find("ScreenMultifunction").GetComponentInChildren<TextMeshPro>().text = RoomCode;

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
