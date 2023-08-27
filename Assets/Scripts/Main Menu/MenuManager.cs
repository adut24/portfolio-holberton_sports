using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the different menus.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private InteractorManager _interactorManager;

    private void Awake() => _interactorManager.ToggleMenuBehavior(true);



    /// <summary>
    /// Loads <paramref name="sceneName"/>.
    /// </summary>
    /// <param name="sceneName"></param>
    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);

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
