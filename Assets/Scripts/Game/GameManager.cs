using Photon.Pun;
using Photon.Realtime;

using System.Collections;

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
	public static GameManager Instance { get; set; }
	public GameObject Player { get; set; }

	[SerializeField] private FadeScreenManager _fadeScreenManager;
	[SerializeField] private SoundManager _soundManager;
	[SerializeField] private AccessibilityManager _accessibilityManager;
	[SerializeField] private PauseMenuManager _pauseMenuManager;
	[SerializeField] private NetworkManager _networkManager;
	[SerializeField] private DataManager _dataManager;
	[SerializeField] private TutorialManager _tutorialManager;

	private string _sport;

	private void Start()
	{
		PhotonNetwork.AutomaticallySyncScene = true;
		Instance = this;
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
		PhotonNetwork.OfflineMode = NumberPlayers != 2;

		if (!PhotonNetwork.OfflineMode)
			PhotonNetwork.ConnectUsingSettings();

		_fadeScreenManager.FadeOut();
		yield return new WaitForSeconds(_fadeScreenManager.FadeDuration);

		if (PhotonNetwork.IsMasterClient)
			PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = NumberPlayers });
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
