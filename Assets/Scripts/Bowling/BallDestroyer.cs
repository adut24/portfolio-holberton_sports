using Photon.Pun;

using UnityEngine;

/// <summary>
/// The BallDestroyer script is responsible for destroying the bowling ball when it enters a trigger collider.
/// It interacts with the network to synchronize the destruction across all connected players.
/// </summary>
public class BallDestroyer : MonoBehaviour
{
	private BowlingManager _bowlingManager;

	/// <summary>
	/// Called when the bowling ball collider enters this trigger collider to destroy the it.
	/// </summary>
	/// <param name="other">The Collider data associated with the entering object.</param>
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("BowlingBall") && other.gameObject.GetPhotonView().IsMine)
		{
			PhotonNetwork.Destroy(other.gameObject);
			if (_bowlingManager == null)
				_bowlingManager = GameManager.Instance.BowlingManager;
			_bowlingManager.BallDestroyed = true;
		}
	}
}
