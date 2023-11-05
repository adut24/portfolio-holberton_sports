using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the different aspects of the game.
/// </summary>
public class GameManager : MonoBehaviour
{
	[SerializeField] private InteractorManager _interactorManager;
	[SerializeField] private Transform _player;
	[SerializeField] private FadeScreenManager _fadeScreen;
	private string _sport = null;

	private void Awake() => _interactorManager.ToggleMenuBehavior();

	/// <summary>
	/// Sets which sport will be loaded.
	/// </summary>
	/// <param name="sportName">Name of the sport to load</param>
	public void SetSport(string sportName) => _sport = sportName;

	/// <summary>
	/// Unsets the sport to load when the user goes back.
	/// </summary>
	public void UnsetSport() => _sport = null;

	/// <summary>
	/// Loads the scene of the sport.
	/// </summary>
	public void LoadSport()
	{
		if (_sport != null)
			StartCoroutine(LoadScene(_sport));
		else
			return;
	}

	private IEnumerator LoadScene(string sceneName)
	{
		_fadeScreen.FadeOut();
		yield return new WaitForSeconds(_fadeScreen.FadeDuration);

		AsyncOperation loadAsync = SceneManager.LoadSceneAsync(sceneName);
		loadAsync.allowSceneActivation = false;

		while (!loadAsync.isDone)
		{
			if (loadAsync.progress >= 0.9f)
				loadAsync.allowSceneActivation = true;
			yield return null;
		}

		if (sceneName.Equals("MainMenu"))
			_interactorManager.ToggleMenuBehavior();
		else
			_interactorManager.ToggleMenuBehavior(false);

		Transform spawnPoint = GameObject.FindWithTag("SpawnPoint").transform;

		_player.position = spawnPoint.position;
		_player.rotation = spawnPoint.rotation;

		_fadeScreen.FadeIn();
		yield return new WaitForSeconds(_fadeScreen.FadeDuration);
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
