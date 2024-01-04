using Photon.Pun;
using Photon.Realtime;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using Unity.XR.CoreUtils;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Responsible for managing the callbacks from the Photon server.
/// </summary>
public class NetworkManager : MonoBehaviourPunCallbacks
{
	/// <summary>
	/// Gets or sets the name of the sport played
	/// </summary>
	public string Sport { get; set; }

	/// <summary>
	/// Gets or sets the generated code of the room.
	/// </summary>
	public string RoomCode { get; set; }

	/// <summary>
	/// Gets or sets if the client is the one that should create the room.
	/// </summary>
	public bool IsCreator { get; set; }

	/// <summary>
	/// Gets or sets if the room to create should be private or public.
	/// </summary>
	public bool IsPublic { get; set; }

	/// <summary>
	/// Gets or sets the GameObject representing the pause menu.
	/// </summary>
	public GameObject PauseMenu { get; set; }

	[SerializeField] private GameObject _playerPrefab;
	[SerializeField] private GameObject _player;
	[SerializeField] private GameObject _roomButtonPrefab;
	[SerializeField] private Transform _roomListContent;
	[SerializeField] private FadeScreenManager _menuFadeManager;
	[SerializeField] private TextMeshProUGUI _errorText;
	[SerializeField] private TextMeshProUGUI _codeText;

	private GameManager _gameManager;

	public override void OnConnectedToMaster()
	{
		if (!PhotonNetwork.OfflineMode)
			PhotonNetwork.JoinLobby();
		else
			PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 1 });
	}

	public override void OnJoinedLobby()
	{
		if (IsCreator)
			PhotonNetwork.CreateRoom(RoomCode,
									 new RoomOptions
									 {
										 MaxPlayers = 2,
										 IsVisible = IsPublic,
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
				GameObject roomButton = Instantiate(_roomButtonPrefab, _roomListContent);
				roomButton.GetComponentInChildren<TextMeshProUGUI>().text = room.Name;
				roomButton.GetComponent<Button>().onClick.AddListener(() => Connect(room.Name));
			}
		}
	}

	public override void OnJoinedRoom()
	{
		if (PhotonNetwork.LocalPlayer.ActorNumber == 2)
			StartCoroutine(FadePlayerTwo());
		StartCoroutine(LoadPlayer());
	}

	private IEnumerator FadePlayerTwo()
	{
		_menuFadeManager.FadeOut();
		yield return new WaitForSeconds(_menuFadeManager.FadeDuration);
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		if (IsCreator)
		{
			_menuFadeManager.FadeIn();
			_codeText.text = "Max number of players reached";
		}
		else
		{
			switch (returnCode)
			{
				case ErrorCode.GameDoesNotExist:
					_errorText.text = "No opened room with this ID was found";
					break;
				case ErrorCode.GameFull:
					_errorText.text = "This room is full";
					break;
				case ErrorCode.GameClosed:
					_errorText.text = "This room was closed";
					break;
			}
		}
	}

	public override void OnLeftRoom()
	{
		PhotonNetwork.Destroy(_player);
		PhotonNetwork.Destroy(GameManager.Instance.gameObject);
		if (PhotonNetwork.IsMasterClient)
			PhotonNetwork.DestroyAll();
		SceneManager.LoadScene(1);
		if (!PhotonNetwork.OfflineMode)
			PhotonNetwork.LeaveLobby();
		Disconnect();
	}

	public void Connect(string roomName) => PhotonNetwork.JoinRoom(roomName);

	public IEnumerator LoadPlayer()
	{
		PhotonNetwork.LoadLevel(Sport);
		_gameManager = GameManager.Instance;

		while (PhotonNetwork.LevelLoadingProgress < 1)
			yield return null;

		Transform spawnPoint = PhotonNetwork.LocalPlayer.ActorNumber == 1 ? GameObject.Find("Spawn Point Player 1").transform : GameObject.Find("Spawn Point Player 2").transform;
		_player = PhotonNetwork.Instantiate(_playerPrefab.name, spawnPoint.position, spawnPoint.rotation, 0);
		GameManager.Instance.Player = _player;

		PhotonView managerView = _gameManager.gameObject.GetPhotonView();
		managerView.ViewID = 0;
		PhotonNetwork.AllocateViewID(managerView);

		Transform rigTransform = GameObject.FindWithTag("Player").transform;
		rigTransform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

		NetworkPlayer networkPlayer = _player.GetComponent<NetworkPlayer>();
		networkPlayer.HeadRig = rigTransform.Find("Camera Offset/Camera");
		networkPlayer.LeftHandRayRig = rigTransform.Find("Camera Offset/LeftRay");
		networkPlayer.RightHandRayRig = rigTransform.Find("Camera Offset/RightRay");
		networkPlayer.LeftHandDirectRig = rigTransform.Find("Camera Offset/LeftDirect");
		networkPlayer.RightHandDirectRig = rigTransform.Find("Camera Offset/RightDirect");
		networkPlayer.LeftHandDirectRig.gameObject.SetActive(false);
		networkPlayer.RightHandDirectRig.gameObject.SetActive(false);

		FadeScreenManager fadeScreenManager = rigTransform.GetComponentInChildren<FadeScreenManager>();
		fadeScreenManager.enabled = true;
		yield return null;

		PauseMenuManager pauseMenuManager = _gameManager.PauseMenuManager;
		pauseMenuManager.PauseMenu = PauseMenu;
		pauseMenuManager.IsNotMainMenu = true;
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
		switch (Sport)
		{
			case "Bowling":
				BowlingManager manager = _gameManager.gameObject.AddComponent<BowlingManager>();
				_gameManager.BowlingManager = manager;
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
		_gameManager.PauseMenuManager.InteractorManager = FindObjectOfType<XROrigin>().transform.GetComponent<InteractorManager>();
	}

	/// <summary>
	/// Disconnects from the Photon servers.
	/// </summary>
	public void Disconnect() => PhotonNetwork.Disconnect();

}
