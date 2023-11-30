using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BallDetection : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("BowlingBall"))
		{
			collision.gameObject.GetComponent<XRGrabInteractable>().enabled = false;
			collision.gameObject.GetComponent<Rigidbody>().velocity *= 1.4f;
		}
	}
}
