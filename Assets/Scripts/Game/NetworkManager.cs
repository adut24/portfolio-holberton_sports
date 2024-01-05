using Photon.Pun;
using Photon.Pun.UtilityScripts;
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
	/// Gets or sets if the room to create should be private or public.
	/// </summary>
	public bool IsPublic { get; set; }

	/// <summary>
	/// Gets or sets the GameObject representing the pause menu.
	/// </summary>
	public GameObject PauseMenu { get; set; }

	[SerializeField] private GameObject _playerPrefab;
	[SerializeField] private GameObject _roomButtonPrefab;
	[SerializeField] private GameObject _errorMenu;
	[SerializeField] private GameObject _roomMenu;
	[SerializeField] private TextMeshProUGUI _errorConnectText;
	[SerializeField] private TextMeshProUGUI _errorJoinText;
	[SerializeField] private Transform _roomListContent;
	[SerializeField] private FadeScreenManager _menuFadeManager;

	private GameManager _gameManager;
	private GameObject _player;
	private Transform _multipurposeScreen;
	private bool _isPlayerOne;

	/// <summary>
	/// 
	/// </summary>
	public override void OnConnectedToMaster()
	{
		if (!PhotonNetwork.OfflineMode)
			PhotonNetwork.JoinLobby();
		else
			PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 1 });
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="roomList"></param>
	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		foreach (Transform button in _roomListContent)
			Destroy(button.gameObject);

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

	/// <summary>
	/// 
	/// </summary>
	public override void OnJoinedRoom()
	{
		_gameManager = GameManager.Instance;
		StartCoroutine(LoadPlayer());
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="returnCode"></param>
	/// <param name="message"></param>
	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		_errorJoinText.text = returnCode switch
		{
			ErrorCode.GameDoesNotExist => "No opened room with this ID was found",
			ErrorCode.GameFull => "This room is full",
			ErrorCode.GameClosed => "This room was closed",
			ErrorCode.JoinFailedFoundActiveJoiner => "This user is already in",
			_ => "An error occured",
		};
	}

	/// <summary>
	/// 
	/// </summary>
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

	/// <summary>
	/// 
	/// </summary>
	/// <param name="cause"></param>
	public override void OnDisconnected(DisconnectCause cause)
	{
		if (cause.Equals(DisconnectCause.MaxCcuReached))
		{
			_roomMenu.SetActive(false);
			_errorMenu.SetActive(true);
			_errorConnectText.text = "Max capacity reached";
		}
		else if (cause.Equals(DisconnectCause.ServerTimeout))
		{
			_roomMenu.SetActive(false);
			_errorMenu.SetActive(true);
			_errorConnectText.text = "No internet connection";
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="newPlayer"></param>
	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
		{
			_gameManager.BowlingManager.SetBall();
			TextMeshPro historyBetweenPlayers = _multipurposeScreen.transform.Find("HistoryText").GetComponent<TextMeshPro>();
			DataManager dataManager = _gameManager.DataManager;
			string id = PhotonNetwork.PlayerListOthers[0].UserId;
			if (dataManager.MatchHistory.ContainsKey(id))
			{
				(int, int) scores = dataManager.MatchHistory[id];
				historyBetweenPlayers.text = $"P1 - P2: {(_isPlayerOne ? scores.Item1 : scores.Item2)} - {(_isPlayerOne ? scores.Item2 : scores.Item1)}";
			}
			else
				historyBetweenPlayers.text = "P1 - P2: 0 - 0";
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public void CreateRoom() => PhotonNetwork.CreateRoom(RoomCode,
														 new RoomOptions
														 {
															 MaxPlayers = 2,
															 IsVisible = IsPublic,
															 IsOpen = true
														 },
														 TypedLobby.Default);

	/// <summary>
	/// 
	/// </summary>
	/// <param name="roomName"></param>
	public void Connect(string roomName) => StartCoroutine(FadeThenJoin(roomName));

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	private IEnumerator FadeScreen()
	{
		_menuFadeManager.FadeOut();
		yield return new WaitForSeconds(_menuFadeManager.FadeDuration);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="roomName"></param>
	/// <returns></returns>
	private IEnumerator FadeThenJoin(string roomName)
	{
		RoomCode = roomName;
		yield return StartCoroutine(FadeScreen());
		PhotonNetwork.JoinRoom(roomName);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	public IEnumerator LoadPlayer()
	{
		Debug.Log("Actor number : " + PhotonNetwork.LocalPlayer.ActorNumber);
		_isPlayerOne = PhotonNetwork.LocalPlayer.ActorNumber == 1;
		if (_isPlayerOne)
		{
			yield return StartCoroutine(FadeScreen());
			PhotonNetwork.LoadLevel(Sport);
			while (PhotonNetwork.LevelLoadingProgress < 1)
				yield return null;
		}

		Transform spawnPoint = _isPlayerOne ? GameObject.Find("Spawn Point Player 1").transform : GameObject.Find("Spawn Point Player 2").transform;
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
		PauseMenu.transform.SetParent(_player.transform, false);

		GameObject tutorial = GameObject.FindGameObjectsWithTag("Tutorial").FirstOrDefault(obj => obj.transform.parent == null);
		if (tutorial != null)
			tutorial.transform.SetParent(_player.transform, false);

		if (Sport.Equals("Bowling") || Sport.Equals("Archery"))
		{
			_multipurposeScreen = GameObject.Find("MultipurposeScreen").transform;
			if (PhotonNetwork.OfflineMode)
			{
				_multipurposeScreen.Find("CodeTitle").GetComponent<TextMeshPro>().text = string.Empty;
				_multipurposeScreen.Find("HistoryTitle").GetComponent<TextMeshPro>().text = "Record:";
				_multipurposeScreen.Find("HistoryText").GetComponent<TextMeshPro>().text = GameManager.Instance.DataManager.HighScores[Sport].ToString();
			}
			else
				_multipurposeScreen.Find("CodeText").GetComponent<TextMeshPro>().text = RoomCode;
		}

		PrepareGameManager(fadeScreenManager);
		fadeScreenManager.FadeIn();
		yield return new WaitForSeconds(fadeScreenManager.FadeDuration);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="fadeScreen"></param>
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
	/// Connects to the Photon servers.
	/// </summary>
	public void ConnectToServer()
	{
		//PhotonNetwork.AuthValues = new AuthenticationValues { UserId = GameManager.Instance.DataManager.UserID };
		PhotonNetwork.AuthValues = new AuthenticationValues { UserId = System.Guid.NewGuid().ToString() };
		PhotonNetwork.ConnectUsingSettings();
	}

	/// <summary>
	/// Indicates that the game will be played locally only.
	/// </summary>
	public void PlayOffline() => PhotonNetwork.OfflineMode = true;

	/// <summary>
	/// Indicates that the game will be played online.
	/// </summary>
	public void PlayOnline() => PhotonNetwork.OfflineMode = false;

	/// <summary>
	/// Disconnects from the Photon servers.
	/// </summary>
	public void Disconnect() => PhotonNetwork.Disconnect();
}
