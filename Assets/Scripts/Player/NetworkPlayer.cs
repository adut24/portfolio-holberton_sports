using Photon.Pun;

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Responsible for how the avatar of the player looks like in the other clients.
/// </summary>
public class NetworkPlayer : MonoBehaviourPun
{
	/// <summary>
	/// Gets or sets the transform of the headset.
	/// </summary>
	public Transform HeadRig { get; set; }

	/// <summary>
	/// Gets or sets the transform of the left hand with the ray interactor.
	/// </summary>
	public Transform LeftHandRayRig { get; set; }

	/// <summary>
	/// Gets or sets the transform of the right hand with the ray interactor.
	/// </summary>
	public Transform RightHandRayRig { get; set; }

	/// <summary>
	/// Gets or sets the transform of the left hand with the direct interactor.
	/// </summary>
	public Transform LeftHandDirectRig { get; set; }

	/// <summary>
	/// Gets or sets the transform of the right hand with the direct interactor.
	/// </summary>
	public Transform RightHandDirectRig { get; set; }

	[SerializeField] private Transform _head;
	[SerializeField] private Transform _leftHand;
	[SerializeField] private Transform _rightHand;
	[SerializeField] private InputActionReference _leftPinchAction;
	[SerializeField] private InputActionReference _leftGripAction;
	[SerializeField] private InputActionReference _rightPinchAction;
	[SerializeField] private InputActionReference _rightGripAction;
	[SerializeField] private Animator _leftHandAnim;
	[SerializeField] private Animator _rightHandAnim;

	/// <summary>
	/// Updates the position and rotation of the avatar based on the player's actions.
	/// </summary>
	private void Update()
	{
		if (photonView.IsMine)
		{
			MapPosition(_head, HeadRig);
			if (LeftHandDirectRig.gameObject.activeSelf)
			{
				MapPosition(_leftHand, LeftHandDirectRig);
				MapPosition(_rightHand, RightHandDirectRig);
			}
			else
			{
				MapPosition(_leftHand, LeftHandRayRig);
				MapPosition(_rightHand, RightHandRayRig);
			}
			_leftHandAnim.SetFloat("Trigger", _leftPinchAction.action.ReadValue<float>());
			_leftHandAnim.SetFloat("Grip", _leftGripAction.action.ReadValue<float>());
			_rightHandAnim.SetFloat("Trigger", _rightPinchAction.action.ReadValue<float>());
			_rightHandAnim.SetFloat("Grip", _rightGripAction.action.ReadValue<float>());
		}
	}

	/// <summary>
	/// Maps the position and rotation of the target transform to match the given rig transform.
	/// </summary>
	/// <param name="target">The target transform to be updated.</param>
	/// <param name="rigTransform">The source rig transform.</param>
	private void MapPosition(Transform target, Transform rigTransform) => target.SetPositionAndRotation(rigTransform.position, rigTransform.rotation);
}
