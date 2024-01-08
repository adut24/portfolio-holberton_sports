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

    /// <summary>
    /// Checks if the bowling ball that entered the collider is the bowling ball or not.
    /// </summary>
    /// <param name="other">Data to check</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("BowlingBall") && !_isResetting)
        {
            _isResetting = true;
            StartCoroutine(DestroyBallAfterDelay(_delay, other.gameObject.GetPhotonView()));
        }
    }

    /// <summary>
    /// Destroys the ball and instantiates a new one in the ball loader.
    /// </summary>
    /// <param name="delay">The delay before the ball is destroyed</param>
    /// <param name="photonView">The PhotonView component of the ball</param>
    /// <returns>A WaitForSeconds object representing the delay before destroying the current ball.</returns>
    private IEnumerator DestroyBallAfterDelay(float delay, PhotonView photonView)
    {
        yield return new WaitForSeconds(delay);
        if (photonView != null && photonView.IsMine)
        {
            PhotonNetwork.Destroy(photonView.gameObject);
            PhotonNetwork.Instantiate("BowlingBall", _resetTransform.position, _resetTransform.rotation);
        }
        _isResetting = false;
    }
}
