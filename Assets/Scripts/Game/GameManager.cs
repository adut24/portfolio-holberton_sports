using Photon.Pun;

using System.Collections;

using TMPro;

using UnityEngine;

/// <summary>
/// Reponsible for managing almost every aspect of the game in the background.
/// </summary>
public class GameManager : MonoBehaviour
{
	/// <summary>
	/// Gets and sets the number of player.
	/// </summary>
	public int NumberPlayers { get; set; }

	/// <summary>
	/// Gets and sets the component managing the volume level.
	/// </summary>
	public SoundManager SoundManager { get; set; }

	/// <summary>
	/// Gets and sets the component managing the accessibility options.
	/// </summary>
	public AccessibilityManager AccessibilityManager { get; set; }

	/// <summary>
	/// Gets and sets the component managing the pause menu.
	/// </summary>
	public PauseMenuManager PauseMenuManager { get; set; }

	/// <summary>
	/// Gets and sets the component managing the communication of the game.
	/// </summary>
	public NetworkManager NetworkManager { get; set; }

	/// <summary>
	/// Gets and sets the component managing the data in the game.
	/// </summary>
	public DataManager DataManager { get; set; }

	/// <summary>
	/// Gets and sets the component managing the tutorial when entering a game.
	/// </summary>
	public TutorialManager TutorialManager { get; set; }

    public BowlingManager BowlingManager { get; set; }

    /// <summary>
    /// Gets and sets the instance of the local player.
    /// </summary>
    public GameObject Player { get; set; }

	/// <summary>
	/// Gets and sets the instance of the local game manager.
	/// </summary>
	public static GameManager Instance { get; private set; }

	[SerializeField] private TextMeshProUGUI _version;
	[SerializeField] private FadeScreenManager _fadeScreenManager;
	[SerializeField] private SoundManager _soundManager;
	[SerializeField] private AccessibilityManager _accessibilityManager;
	[SerializeField] private PauseMenuManager _pauseMenuManager;
	[SerializeField] private NetworkManager _networkManager;
	[SerializeField] private DataManager _dataManager;
	[SerializeField] private TutorialManager _tutorialManager;
	[SerializeField] private TextMeshProUGUI _codeSlot;

	private string _sport;
	private string _roomCode;

	/// <summary>
	/// Called when the script instance is being loaded. Initializes references to various managers, sets up network settings, 
	/// and ensures the persistence of essential components across scenes.
	/// </summary>
	private void Start()
	{
		Instance = this;

		PhotonNetwork.AutomaticallySyncScene = true;
		_version.text = string.Format("version {0}", Application.version);
		PhotonNetwork.GameVersion = Application.version;

		SoundManager = _soundManager;
		AccessibilityManager = _accessibilityManager;
		PauseMenuManager = _pauseMenuManager;
		NetworkManager = _networkManager;
		DataManager = _dataManager;
		TutorialManager = _tutorialManager;
		NetworkManager.PauseMenu = PauseMenuManager.PauseMenu;

		DontDestroyOnLoad(gameObject);
		DontDestroyOnLoad(PauseMenuManager.PauseMenu);
	}

	/// <summary>
	/// Sets the selected sport for the game.
	/// </summary>
	/// <param name="sportName">The name of the selected sport.</param>
	public void SetSport(string sportName) => _sport = sportName;

	/// <summary>
	/// Unsets the selected sport for the game.
	/// </summary>
	public void UnsetSport() => _sport = null;

	/// <summary>
	/// Loads the selected sport if it is set.
	/// </summary>
	public void LoadSport()
	{
		if (_sport != null)
			StartCoroutine(LoadScene());
	}

	/// <summary>
	/// Coroutine for loading the game scene.
	/// </summary>
	private IEnumerator LoadScene()
	{
		NetworkManager.Sport = _sport;
		NetworkManager.RoomCode = _roomCode;
		NetworkManager.IsCreator = true;

		_fadeScreenManager.FadeOut();
		yield return new WaitForSeconds(_fadeScreenManager.FadeDuration);

		PhotonNetwork.OfflineMode = NumberPlayers != 2;
		if (!PhotonNetwork.OfflineMode)
			PhotonNetwork.ConnectUsingSettings();
	}

	/// <summary>
	/// Connects player two to the server.
	/// </summary>
	public void ConnectPlayerTwo()
	{
		NetworkManager.Sport = _sport;
		NetworkManager.IsCreator = false;

		PhotonNetwork.OfflineMode = false;
		PhotonNetwork.ConnectUsingSettings();
	}

	/// <summary>
	/// Loads the game using the provided room code.
	/// </summary>
	/// <param name="code">The room code to connect to.</param>
	public void LoadGame(TextMeshProUGUI code) => NetworkManager.Connect(code.text.TrimEnd()[..^1]);

	/// <summary>
	/// Generates and displays a random room code.
	/// </summary>
	public void GenerateRoomCode()
	{
		string alphaNumChar = Random.Range(0000, 10000).ToString("D4") + Random.Range(0000, 10000).ToString("D4");
		_roomCode = string.Empty;

		for (int i = 0; i < alphaNumChar.Length; i++)
		{
			if (i % 2 == 0)
				_roomCode += (char)('A' + int.Parse(alphaNumChar[i].ToString()));
			else
				_roomCode += alphaNumChar[i];
		}
		_codeSlot.text = _roomCode;
	}

	/// <summary>
	/// Quits the game.
	/// </summary>
	public void QuitGame()
	{
		/* REMOVE THE UNITY EDITOR LINE IN FINAL VERSION */
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
}
