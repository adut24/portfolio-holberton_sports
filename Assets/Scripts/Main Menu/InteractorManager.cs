using UnityEngine;

/// <summary>
/// Responsible for managing the interaction method for the player.
/// </summary>
public class InteractorManager : MonoBehaviour
{
    [SerializeField] private GameObject _leftDirectController;
    [SerializeField] private GameObject _rightDirectController;
    [SerializeField] private GameObject _leftRayController;
    [SerializeField] private GameObject _rightRayController;

    /// <summary>
    /// Toggles the interaction behavior between direct and ray-based methods.
    /// </summary>
    /// <param name="rayBehavior">If true, activates ray-based interaction; otherwise, activates direct interaction.</param>
    public void ToggleBehavior(bool rayBehavior = true)
    {
        _leftDirectController.SetActive(!rayBehavior);
        _rightDirectController.SetActive(!rayBehavior);
        _leftRayController.SetActive(rayBehavior);
        _rightRayController.SetActive(rayBehavior);
    }
}
