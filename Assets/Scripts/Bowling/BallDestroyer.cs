 using UnityEngine;

public class BallDestroyer : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("BowlingBall"))
			Destroy(collision.gameObject);
	}
}
