using Photon.Pun;

using TMPro;

using UnityEngine;

/// <summary>
/// Reponsible for managing almost every aspect of the game in the background.
/// </summary>
public class GameManager : MonoBehaviour
{

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

	/// <summary>
	/// Gets or sets the component managing a bowling game.
	/// </summary>
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
	[SerializeField] private SoundManager _soundManager;
	[SerializeField] private AccessibilityManager _accessibilityManager;
	[SerializeField] private PauseMenuManager _pauseMenuManager;
	[SerializeField] private NetworkManager _networkManager;
	[SerializeField] private DataManager _dataManager;
	[SerializeField] private TutorialManager _tutorialManager;
	[SerializeField] private TextMeshProUGUI _codeSlot;

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
	/// Loads the game using the provided room code.
	/// </summary>
	/// <param name="code">The room code to connect to.</param>
	public void LoadGame(TextMeshProUGUI code) => NetworkManager.Connect(code.text.TrimEnd()[..^1]);

	/// <summary>
	/// Generates and displays a random room code.
	/// </summary>
	public void GenerateRoomCode()
	{
		/* 167 961 600 000 000 possibilités */
		const string chars = "ABCDEFGHIJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
		char[] stringChars = new char[8];

		for (int i = 0; i < stringChars.Length; i++)
			stringChars[i] = chars[Random.Range(0, chars.Length)];
		string code = new(stringChars);
		_codeSlot.text = code;
		NetworkManager.RoomCode = code;
	}

	/// <summary>
	/// Quits the game.
	/// </summary>
	public void QuitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
}
