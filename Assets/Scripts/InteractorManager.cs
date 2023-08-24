using UnityEngine;

/// <summary>
/// Manages which interactor is enabled or not.
/// </summary>
public class InteractorManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _leftDirectController;
    [SerializeField]
    private GameObject _rightDirectController;
    [SerializeField]
    private GameObject _leftRayController;
    [SerializeField]
    private GameObject _rightRayController;

    /// <summary>
    /// Activates the RayInteractor and deactivates the DirectInteractor.
    /// </summary>
    public void ActivateMenuBehavior()
    {
        _leftDirectController.SetActive(false);
        _rightDirectController.SetActive(false);
        _leftRayController.SetActive(true);
        _rightRayController.SetActive(true);
    }

    /// <summary>
    /// Activates the DirectInteractor and deactivates the RayInteractor.
    /// </summary>
    public void ActivateGameBehavior()
    {
        _leftDirectController.SetActive(true);
        _rightDirectController.SetActive(true);
        _leftRayController.SetActive(false);
        _rightRayController.SetActive(false);
    }
}
