using UnityEngine;
using Photon.Pun;

public class BallDestroyer : MonoBehaviour
{
	private BowlingManager _bowlingManager;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("BowlingBall"))
		{
			Destroy(other.gameObject);
			if (_bowlingManager == null)
				_bowlingManager = GameManager.Instance.GetComponent<BowlingManager>();
			_bowlingManager.BallDestroyed = true;
		}
	}
}
