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
    private Transform _rigTransform;
    private Transform _spawnPoint;
    private bool _isPlayerOne;

    /// <summary>
    /// Called when connected to the Photon Master Server.
    /// </summary>
    public override void OnConnectedToMaster()
    {
        if (!PhotonNetwork.OfflineMode)
            PhotonNetwork.JoinLobby();
        else
            PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 1 });
    }

    /// <summary>
    /// Called when the list of available rooms is updated.
    /// </summary>
    /// <param name="roomList">The updated list of available rooms.</param>
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
    /// Called when the local player has joined a room.
    /// </summary>
    public override void OnJoinedRoom()
    {
        _gameManager = GameManager.Instance;
        _isPlayerOne = PhotonNetwork.LocalPlayer.ActorNumber == 1;
        if (_isPlayerOne)
            StartCoroutine(WaitForLoading());
        else
            StartCoroutine(LoadPlayer());
    }

    /// <summary>
    /// Called when joining a room fails.
    /// </summary>
    /// <param name="returnCode">The error code.</param>
    /// <param name="message">The error message.</param>
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
    /// Called when the local player has left the room.
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
    /// Called when the connection to Photon is lost.
    /// </summary>
    /// <param name="cause">The cause of the disconnection.</param>
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
    /// Called when a new player has entered the room.
    /// </summary>
    /// <param name="newPlayer">The new player.</param>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        _gameManager.BowlingManager.SetBall();
        _rigTransform.SetPositionAndRotation(_spawnPoint.position, _spawnPoint.rotation);
        WriteHistoryBetweenPlayers(_gameManager.DataManager.MatchHistory, _multipurposeScreen.transform.Find("HistoryText").GetComponent<TextMeshPro>(), PhotonNetwork.PlayerListOthers[0].UserId);
    }

    /// <summary>
    /// Creates a new room.
    /// </summary>
    public void CreateRoom() => PhotonNetwork.CreateRoom(RoomCode,
                                                         new RoomOptions
                                                         {
                                                             MaxPlayers = 2,
                                                             IsVisible = IsPublic,
                                                             IsOpen = true,
                                                             PublishUserId = true
                                                         },
                                                         TypedLobby.Default);

    /// <summary>
    /// Connects to an existing room.
    /// </summary>
    /// <param name="roomName">The name of the room to connect to.</param>
    public void Connect(string roomName) => StartCoroutine(FadeThenJoin(roomName));

    /// <summary>
    /// Fades the screen out.
    /// </summary>
    /// <returns>An IEnumerator for yielding in a coroutine context.</returns>
    private IEnumerator FadeScreen()
    {
        _menuFadeManager.FadeOut();
        yield return new WaitForSeconds(_menuFadeManager.FadeDuration);
    }

    /// <summary>
    /// Fades the screen out and then joins a room.
    /// </summary>
    /// <param name="roomName">The name of the room to join.</param>
    /// <returns>An IEnumerator for yielding in a coroutine context.</returns>
    private IEnumerator FadeThenJoin(string roomName)
    {
        RoomCode = roomName;
        yield return StartCoroutine(FadeScreen());
        PhotonNetwork.JoinRoom(roomName);
    }

    /// <summary>
    /// Waits for loading the level and then loads the player.
    /// </summary>
    /// <returns>An IEnumerator for yielding in a coroutine context.</returns>
    private IEnumerator WaitForLoading()
    {
        yield return StartCoroutine(LoadLevel());
        StartCoroutine(LoadPlayer());
    }

    /// <summary>
    /// Loads the level with a fade effect.
    /// </summary>
    /// <returns>An IEnumerator for yielding in a coroutine context.</returns>
    private IEnumerator LoadLevel()
    {
        yield return StartCoroutine(FadeScreen());
        PhotonNetwork.LoadLevel(Sport);
        while (PhotonNetwork.LevelLoadingProgress < 1)
            yield return null;
    }

    /// <summary>
    /// Coroutine to load the player into the game.
    /// </summary>
    /// <returns>An IEnumerator for yielding in a coroutine context.</returns>
    public IEnumerator LoadPlayer()
    {
        _spawnPoint = _isPlayerOne ? GameObject.Find("Spawn Point Player 1").transform : GameObject.Find("Spawn Point Player 2").transform;
        _player = PhotonNetwork.Instantiate(_playerPrefab.name, _spawnPoint.position, _spawnPoint.rotation, 0);
        GameManager.Instance.Player = _player;

        PhotonView managerView = _gameManager.gameObject.GetPhotonView();
        managerView.ViewID = 0;
        PhotonNetwork.AllocateViewID(managerView);

        _rigTransform = GameObject.FindWithTag("Player").transform;
        _rigTransform.SetPositionAndRotation(_spawnPoint.position, _spawnPoint.rotation);

        NetworkPlayer networkPlayer = _player.GetComponent<NetworkPlayer>();
        networkPlayer.HeadRig = _rigTransform.Find("Camera Offset/Camera");
        networkPlayer.LeftHandRayRig = _rigTransform.Find("Camera Offset/LeftRay");
        networkPlayer.RightHandRayRig = _rigTransform.Find("Camera Offset/RightRay");
        networkPlayer.LeftHandDirectRig = _rigTransform.Find("Camera Offset/LeftDirect");
        networkPlayer.RightHandDirectRig = _rigTransform.Find("Camera Offset/RightDirect");
        networkPlayer.LeftHandDirectRig.gameObject.SetActive(false);
        networkPlayer.RightHandDirectRig.gameObject.SetActive(false);

        FadeScreenManager fadeScreenManager = _rigTransform.GetComponentInChildren<FadeScreenManager>();
        fadeScreenManager.enabled = true;
        yield return null;

        PauseMenuManager pauseMenuManager = _gameManager.PauseMenuManager;
        pauseMenuManager.PauseMenu = PauseMenu;
        PauseMenu.transform.SetParent(_player.transform, false);

        GameObject tutorial = GameObject.FindGameObjectsWithTag("Tutorial").FirstOrDefault(obj => obj.transform.parent == null);
        if (tutorial != null)
            tutorial.transform.SetParent(_player.transform, false);

        PrepareGameManager(fadeScreenManager);
        PrepareScreen();

        fadeScreenManager.FadeIn();
        yield return new WaitForSeconds(fadeScreenManager.FadeDuration);
    }

    /// <summary>
    /// Adds the script managing the chosen sport to GameManager.
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
                manager.PauseManager = _gameManager.PauseMenuManager;
                manager.ReplayMenu = GameObject.FindGameObjectsWithTag("Replay").FirstOrDefault(obj => obj.transform.parent == null);
                GameObject replayMenu = manager.ReplayMenu;
                if (replayMenu != null)
                {
                    replayMenu.transform.SetParent(_player.transform, false);
                    replayMenu.SetActive(false);
                }
                //_player.AddComponent<BowlingStatusManager>();
                break;
        }
        _gameManager.PauseMenuManager.InteractorManager = FindObjectOfType<XROrigin>().transform.GetComponent<InteractorManager>();
    }

    /// <summary>
    /// Connects to the Photon servers.
    /// </summary>
    public void ConnectToServer()
    {
        PhotonNetwork.AuthValues = new AuthenticationValues { UserId = GameManager.Instance.DataManager.UserID };
        //PhotonNetwork.AuthValues = new AuthenticationValues { UserId = System.Guid.NewGuid().ToString() };
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

    /// <summary>
    /// Writes the match history between players to a TextMeshPro component.
    /// </summary>
    /// <param name="matchHistory">The dictionary containing match history data for players.</param>
    /// <param name="historyText">The TextMeshPro component where the match history will be displayed.</param>
    /// <param name="playerID">The unique identifier of the oppnent.</param>
    private void WriteHistoryBetweenPlayers(Dictionary<string, (int, int)> matchHistory, TextMeshPro historyText, string playerID)
    {
        if (matchHistory.ContainsKey(playerID))
        {
            (int, int) scores = matchHistory[playerID];
            historyText.text = $"P1:{scores.Item1}/P2:{scores.Item2}";
        }
        else
        {
            matchHistory[playerID] = (0, 0);
            historyText.text = "P1:0/P2:0";
        }
        _gameManager.BowlingManager.OpponentID = playerID;
    }

    /// <summary>
    /// Writes the informations on the screen.
    /// </summary>
    private void PrepareScreen()
    {
        if (Sport.Equals("Bowling") || Sport.Equals("Archery"))
        {
            _multipurposeScreen = GameObject.Find("MultipurposeScreen").transform;
            if (PhotonNetwork.OfflineMode)
            {
                _multipurposeScreen.Find("CodeTitle").GetComponent<TextMeshPro>().text = string.Empty;
                _multipurposeScreen.Find("HistoryTitle").GetComponent<TextMeshPro>().text = "Record:";
                _multipurposeScreen.Find("HistoryText").GetComponent<TextMeshPro>().text = _gameManager.DataManager.HighScores[Sport].ToString();
            }
            else
            {
                _multipurposeScreen.Find("CodeText").GetComponent<TextMeshPro>().text = RoomCode;
                if (!_isPlayerOne)
                    WriteHistoryBetweenPlayers(_gameManager.DataManager.MatchHistory, 
                                               _multipurposeScreen.transform.Find("HistoryText").GetComponent<TextMeshPro>(), 
                                               PhotonNetwork.PlayerListOthers[0].UserId);
            }
        }
    }
}
