using Photon.Pun;

using System.Collections;

using UnityEngine;

/// <summary>
/// responsible for managing the behavior of bowling pins, including handling collisions, scoring, and destruction in a networked environment.
/// </summary>
public class Pin : MonoBehaviourPun
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _pinHit;

    private bool _knocked;
    private bool _clipPlayed;
    private BowlingManager _bowlingManager;

    /// <summary>
    /// Called when the script is activated. Gets the component managing the game and transfers the ownership to the client.
    /// </summary>
    private void OnEnable()
    {
        _bowlingManager = GameManager.Instance.BowlingManager;
        photonView.TransferOwnership(GameManager.Instance.Player.GetPhotonView().OwnerActorNr);
    }

    /// <summary>
    /// Called every frame. Checks for pin knockdown based on velocity and orientation, updates the score, and triggers destruction.
    /// </summary>
    private void Update()
    {
        if (!_knocked && (_rb.velocity.magnitude >= 2.3f || transform.up.z <= 0.7f) && photonView.IsMine)
        {
            _knocked = true;
            _bowlingManager.ScoreFrame++;
            _bowlingManager.Pins.Remove(gameObject);
            StartCoroutine(DelayedDestroy(1.5f));
        }
    }

    /// <summary>
    /// Plays the sound of a pin being hit.
    /// </summary>
    /// <param name="collision">informations about the object being hit</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Pin") || collision.transform.CompareTag("BowlingBall")  && !_clipPlayed)
        {
            _audioSource.PlayOneShot(_pinHit);
            _clipPlayed = true;
        }
    }

    /// <summary>
    /// Coroutine for delayed destruction of the pin.
    /// </summary>
    /// <param name="delay">The delay in seconds before destruction.</param>
    /// <returns>A WaitForSeconds object representing the delay before the pin is destroyed.</returns>
    private IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.Destroy(gameObject);
    }
}
