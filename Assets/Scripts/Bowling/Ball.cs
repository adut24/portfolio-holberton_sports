using Photon.Pun;

using System.Collections;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Responsible for handling interactions and events related to the game ball in a VR environment.
/// </summary>
public class Ball : MonoBehaviourPun
{
	/// <summary>
	/// Gets or sets the component managing the game.
	/// </summary>
	public BowlingManager BowlingManager { get; set; }

	[SerializeField] private XRGrabInteractable _grabComponent;
	[SerializeField] private Renderer _renderer;
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private AudioClip _hitAlleyClip;
	[SerializeField] private AudioClip _rollAlleyClip;

	/// <summary>
	/// Called when the script instance is being loaded. Enables events regarding the grab of the ball and sets that the color is the same accross the clients.
	/// </summary>
	private void Start()
	{
		Color[] colors = new Color[]
		{
			Color.black,
			Color.blue,
			Color.cyan,
			Color.gray,
			Color.green,
			Color.red,
			Color.yellow,
			Color.magenta
		};
		Color color = colors[Random.Range(0, colors.Length)];
		photonView.RpcSecure("RPC_SetColor", RpcTarget.AllBuffered, true, new Vector3(color.r, color.g, color.b));
		_grabComponent.selectEntered.AddListener(OnGrab);
		_grabComponent.selectExited.AddListener(OnRelease);
	}

	/// <summary>
	/// Sets the color of the ball across all clients in a Photon network.
	/// </summary>
	/// <param name="color">The RGB color representation as a Vector3, in RGB order.</param>
	[PunRPC]
	void RPC_SetColor(Vector3 color) => _renderer.material.SetColor("_BaseColor", new Color(color.x, color.y, color.z, 1.0f));


	/// <summary>
	/// Called when a collision occurs with another object to play the correct hit sound.
	/// </summary>
	/// <param name="collision">The data associated with this event.</param>
	private void OnCollisionEnter(Collision collision)
	{
		_audioSource.PlayOneShot(_hitAlleyClip);
	}

	/// <summary>
	/// Event handler for when the ball is grabbed. Transfers ownership of the PhotonView to the player that grabbed the ball.
	/// </summary>
	/// <param name="args">The data associated with this event.</param>
	private void OnGrab(SelectEnterEventArgs args)
	{
		PhotonView playerView = args.interactorObject.transform.gameObject.GetComponentInParent<PhotonView>();
		if (playerView != null)
			photonView.TransferOwnership(playerView.OwnerActorNr);
	}

	/// <summary>
	/// Event handler for when the ball is released. Makes that the ball can't be grabbed again.
	/// </summary>
	/// <param name="args">The data associated with this event.</param>
	private void OnRelease(SelectExitEventArgs args) => StartCoroutine(DisableGrab());

	/// <summary>
	/// Coroutine that waits for a short delay before disabling the XRGrabInteractable component.
	/// This allows the ball to fall under gravity before it stops being interactable.
	/// Afterwards, waits for a certain amount of time before destroying the ball. Useful in case the ball is stuck somewhere.
	/// </summary>
	/// <returns>A WaitForSeconds object representing the delay before the ball can't be grabbed anymore.</returns>
	private IEnumerator DisableGrab()
	{
		yield return new WaitForSeconds(0.1f);
		_grabComponent.enabled = false;
		yield return new WaitForSeconds(15f);
		if (photonView.IsMine)
		{
			if (BowlingManager == null)
				BowlingManager = GameManager.Instance.BowlingManager;
			PhotonNetwork.Destroy(gameObject);
			BowlingManager.BallDestroyed = true;
		}
	}
}