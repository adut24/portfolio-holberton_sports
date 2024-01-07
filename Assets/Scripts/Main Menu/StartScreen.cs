using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Responsible for managing the first screen of the game.
/// </summary>
public class StartScreen : MonoBehaviour
{
	[SerializeField] private InputActionReference _triggerLeftController;
	[SerializeField] private InputActionReference _triggerRightController;
	[SerializeField] private Image _leftTrigger;
	[SerializeField] private Image _rightTrigger;
	[SerializeField] private Sprite _leftTriggerPressed;
	[SerializeField] private Sprite _rightTriggerPressed;

	/// <summary>
	/// Called when the component is enabled. Activates the tracking of the actions.
	/// </summary>
	private void OnEnable()
	{
		_triggerLeftController.action.Enable();
		_triggerRightController.action.Enable();
	}

	/// <summary>
	/// Called every frame. Checks if one of the trigger was pressed or not.
	/// </summary>
	private void Update()
	{
		if (_triggerLeftController.action.triggered)
		{
			_leftTrigger.sprite = _leftTriggerPressed;
			LoadMenu();
		}
		if (_triggerRightController.action.triggered)
		{
			_rightTrigger.sprite = _rightTriggerPressed;
			LoadMenu();
		}
	}

	/// <summary>
	/// Loads the main menu scene.
	/// </summary>
	private void LoadMenu() => SceneManager.LoadScene(1);
}
