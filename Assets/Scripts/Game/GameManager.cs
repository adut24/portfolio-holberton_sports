using Photon.Pun;
using Photon.Realtime;

using System.Collections;

using UnityEngine;

public class GameManager : MonoBehaviour
{
	[SerializeField] private InteractorManager _interactorManager;
	[SerializeField] private FadeScreenManager _fadeScreen;
	private string _sport;
	public int NumberPlayers { get; set; }
	public static GameManager Instance { get; set; }

	private void Awake()
	{
		Instance = this;
		_interactorManager.ToggleMenuBehavior();
		DontDestroyOnLoad(gameObject);
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

		_fadeScreen.FadeOut();
		yield return new WaitForSeconds(_fadeScreen.FadeDuration);

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
