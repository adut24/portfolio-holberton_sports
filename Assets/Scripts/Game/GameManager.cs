using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	[SerializeField] private InteractorManager _interactorManager;
	[SerializeField] private Transform _player;
	[SerializeField] private FadeScreenManager _fadeScreen;
	private string _sport = null;

	private void Awake() => _interactorManager.ToggleMenuBehavior();


	public void SetSport(string sportName) => _sport = sportName;

	public void UnsetSport() => _sport = null;

	public void LoadSport()
	{
		if (_sport != null)
			StartCoroutine(LoadScene(_sport));
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
			_interactorManager.ToggleMenuBehavior(AccessibilityManager.ReducedMobilityMode);

		Transform spawnPoint = GameObject.FindWithTag("SpawnPoint").transform;

		_player.position = spawnPoint.position;
		_player.rotation = spawnPoint.rotation;

		_fadeScreen.FadeIn();
		yield return new WaitForSeconds(_fadeScreen.FadeDuration);
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
