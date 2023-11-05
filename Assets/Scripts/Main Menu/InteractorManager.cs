using UnityEngine;

/// <summary>
/// Manages which interactor is enabled or not.
/// </summary>
public class InteractorManager : MonoBehaviour
{
	[SerializeField] private GameObject _leftDirectController;
	[SerializeField] private GameObject _rightDirectController;
	[SerializeField] private GameObject _leftRayController;
	[SerializeField] private GameObject _rightRayController;

	/// <summary>
	/// Activates the RayInteractor and deactivates the DirectInteractor.
	/// </summary>
	public void ToggleMenuBehavior(bool menuBehavior = true)
	{
		_leftDirectController.SetActive(!menuBehavior);
		_rightDirectController.SetActive(!menuBehavior);
		_leftRayController.SetActive(menuBehavior);
		_rightRayController.SetActive(menuBehavior);
	}
}
