using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the loading on the starting screen.
/// </summary>
public class StartScreen : MonoBehaviour
{
	[SerializeField] private InputActionReference _triggerLeftController;
	[SerializeField] private InputActionReference _triggerRightController;
	[SerializeField] private Image _leftTrigger;
	[SerializeField] private Image _rightTrigger;
	[SerializeField] private Sprite _leftTriggerNotPressed;
	[SerializeField] private Sprite _rightTriggerNotPressed;
	[SerializeField] private Sprite _leftTriggerPressed;
	[SerializeField] private Sprite _rightTriggerPressed;

	private void Update()
	{
		bool isLeftTriggerPressed = _triggerLeftController.action.ReadValue<float>() > 0;
		bool isRightTriggerPressed = _triggerRightController.action.ReadValue<float>() > 0;
		
		if (isLeftTriggerPressed)
			_leftTrigger.sprite = _leftTriggerPressed;
		else
			_leftTrigger.sprite = _leftTriggerNotPressed;

		if (isRightTriggerPressed)
			_rightTrigger.sprite = _rightTriggerPressed;
		else
			_rightTrigger.sprite = _rightTriggerNotPressed;

		if (isLeftTriggerPressed && isRightTriggerPressed)
			LoadMenu();
	}

	private void LoadMenu() => SceneManager.LoadScene("MainMenu");
}
