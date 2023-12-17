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
			Destroy(gameObject, 1f);
		}
	}
}
