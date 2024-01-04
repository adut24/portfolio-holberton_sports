using Photon.Pun;

using System.Collections;

using UnityEngine;

/// <summary>
/// Responsible for retrieving and resetting the state of a bowling ball when it collides with the floor.
/// </summary>
public class BallRetriever : MonoBehaviour
{
	[SerializeField] private Transform _resetTransform;

	private readonly float _delay = 3f;
	private bool _isResetting = false;

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.CompareTag("BowlingBall") && !_isResetting)
		{
			_isResetting = true;
			StartCoroutine(ResetBallStateAfterDelay(_delay, other.gameObject.GetPhotonView()));
		}
	}


	private IEnumerator ResetBallStateAfterDelay(float delay, PhotonView photonView)
	{
		yield return new WaitForSeconds(delay);
		if (photonView.IsMine)
		{
			PhotonNetwork.Destroy(photonView.gameObject);
			PhotonNetwork.Instantiate("BowlingBall", _resetTransform.position, _resetTransform.rotation);
		}
		_isResetting = false;
	}
}
