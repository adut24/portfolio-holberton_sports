using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the animation on the hand depending on the triggers pressed.
/// </summary>
public class AnimateHandOnInput : MonoBehaviour
{
	[SerializeField] private InputActionProperty _pinchAnimation;
	[SerializeField] private InputActionProperty _gripAnimation;
	[SerializeField] private Animator _handAnim;

	private void Update()
	{
		_handAnim.SetFloat("Trigger", _pinchAnimation.action.ReadValue<float>());
		_handAnim.SetFloat("Grip", _gripAnimation.action.ReadValue<float>());
	}
}
