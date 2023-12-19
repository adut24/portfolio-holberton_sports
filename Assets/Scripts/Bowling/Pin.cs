using Photon.Pun;

using System.Collections;

using UnityEngine;

public class Pin : MonoBehaviour
{
	private bool _knocked;
	private Rigidbody _rb;
	private BowlingManager _bowlingManager;

	private void OnEnable()
	{
		_bowlingManager = GameManager.Instance.GetComponent<BowlingManager>();
		_rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		if (!_knocked && (_rb.velocity.magnitude > 1.95f || transform.up.z <= 0.5f))
		{
			_knocked = true;
			_bowlingManager.ScoreFrame++;
			_bowlingManager.Pins.Remove(gameObject);
			StartCoroutine(DelayedDestroy(1.5f));
		}
	}

	private IEnumerator DelayedDestroy(float delay)
	{
		yield return new WaitForSeconds(delay);
		PhotonNetwork.Destroy(gameObject);
	}

}
