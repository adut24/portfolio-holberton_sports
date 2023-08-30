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

    private void Awake() => _interactorManager.ToggleMenuBehavior();

    private void Update()
    {

    }

    /// <summary>
    /// Loads <paramref name="sceneName"/>.
    /// </summary>
    /// <param name="sceneName">Scene to load</param>
    public void LoadScene(string sceneName)
    {
        GameObject player = GameObject.FindWithTag("Player");
        player.transform.position = new Vector3(20, 0, 0);
        SceneManager.LoadScene(sceneName);
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
