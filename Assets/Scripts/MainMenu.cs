using UnityEngine;

/// <summary>
/// Manages the main menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private InteractorManager _interactorManager;

    private void Awake() => _interactorManager.ActivateMenuBehavior();

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
