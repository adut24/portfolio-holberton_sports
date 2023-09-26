using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the different kinds of menus.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private InteractorManager _interactorManager;
    [SerializeField]
    private InputActionProperty _menuButtonLeftPress;
    [SerializeField]
    private InputActionProperty _menuButtonRightPress;
    [SerializeField]
    private Transform _player;

    private void Awake() => _interactorManager.ToggleMenuBehavior();

    private void Update()
    {

    }

    private void PauseGame()
    {
        Debug.Log("Pause Activated");
        _interactorManager.ToggleMenuBehavior();
    }


    public void LoadGame(string game) => StartCoroutine(LoadScene(game));

    private IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
            yield return null;

        Transform spawnPoint = GameObject.FindWithTag("SpawnPoint").transform;

        _player.position = spawnPoint.position;
        _player.rotation = spawnPoint.rotation;
        _interactorManager.ToggleMenuBehavior(false);
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
