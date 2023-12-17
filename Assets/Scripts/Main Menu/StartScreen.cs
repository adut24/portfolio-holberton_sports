using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
	[SerializeField] private InputActionReference _triggerLeftController;
	[SerializeField] private InputActionReference _triggerRightController;
	[SerializeField] private Image _leftTrigger;
	[SerializeField] private Image _rightTrigger;
	[SerializeField] private Sprite _leftTriggerPressed;
	[SerializeField] private Sprite _rightTriggerPressed;

	private void OnEnable()
	{
		_triggerLeftController.action.Enable();
		_triggerRightController.action.Enable();
	}


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

	private void LoadMenu() => SceneManager.LoadScene("MainMenu");
}
