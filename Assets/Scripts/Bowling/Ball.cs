using System.Collections;

using UnityEngine;

public class Ball : MonoBehaviour
{
	[SerializeField] private AudioSource _audioSource;
	[SerializeField] private AudioClip _hitAlleyClip;
	[SerializeField] private AudioClip _rollAlleyClip;

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.CompareTag("Alley"))
		{
			_audioSource.PlayOneShot(_hitAlleyClip);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.transform.CompareTag("Alley"))
		{
			_audioSource.PlayOneShot(_rollAlleyClip);
		}
	}
}
