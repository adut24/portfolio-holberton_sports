using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public int NumberPlayers { get; set; }
	public SoundManager SoundManager { get; set; }
	public AccessibilityManager AccessibilityManager { get; set; }
	public PauseMenuManager PauseMenuManager { get; set; }
	public NetworkManager NetworkManager { get; set; }
	public DataManager DataManager { get; set; }
	public TutorialManager TutorialManager { get; set; }
	public GameObject Player { get; set; }
	public static GameManager Instance { get; set; }

	[SerializeField] private TextMeshProUGUI _version;
	[SerializeField] private FadeScreenManager _fadeScreenManager;
	[SerializeField] private SoundManager _soundManager;
	[SerializeField] private AccessibilityManager _accessibilityManager;
	[SerializeField] private PauseMenuManager _pauseMenuManager;
	[SerializeField] private NetworkManager _networkManager;
	[SerializeField] private DataManager _dataManager;
	[SerializeField] private TutorialManager _tutorialManager;

	private string _sport;
	private string _roomCode;

	private void Start()
	{
		Instance = this;

		PhotonNetwork.AutomaticallySyncScene = true;
		_version.text = string.Format("version {0}", Application.version);
		PhotonNetwork.GameVersion = Application.version;
		PhotonNetwork.Disconnect();

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

	public void SetSport(string sportName) => _sport = sportName;

	public void UnsetSport() => _sport = null;

	public void LoadSport()
	{
		if (_sport != null)
			StartCoroutine(LoadScene());
	}

	private IEnumerator LoadScene()
	{
		NetworkManager.Game = _sport;
		NetworkManager.RoomCode = _roomCode;
		NetworkManager.NumberPlayers = NumberPlayers;
		NetworkManager.IsCreator = true;

		_fadeScreenManager.FadeOut();
		yield return new WaitForSeconds(_fadeScreenManager.FadeDuration);

		PhotonNetwork.OfflineMode = NumberPlayers != 2;
		if (!PhotonNetwork.OfflineMode)
			PhotonNetwork.ConnectUsingSettings();
	}

	public void ConnectPlayerTwo()
	{
		NetworkManager.Game = _sport;
		NetworkManager.IsCreator = false;

		PhotonNetwork.OfflineMode = false;
		PhotonNetwork.ConnectUsingSettings();
	}

	public void GenerateRoomCode()
	{
		string alphaNumChar = Random.Range(0000, 10000).ToString("D4") + Random.Range(0000, 10000).ToString("D4");
		string finalCode = "";

		for (int i = 0; i < alphaNumChar.Length; i++)
		{
			if (i % 2 == 0)
				finalCode += (char)('A' + int.Parse(alphaNumChar[i].ToString()));
			else
				finalCode += alphaNumChar[i];
		}

		_roomCode = finalCode;
	}


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
