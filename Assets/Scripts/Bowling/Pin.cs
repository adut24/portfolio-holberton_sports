using UnityEngine;

public class Pin : MonoBehaviour
{
	private bool _disposed = false;
	private BowlingManager _bowlingManager;

	private void OnEnable()
	{
		_bowlingManager = GameManager.Instance.GetComponent<BowlingManager>();
	}

	private void Update()
	{
		if(name.Equals("Bowling Pin 10"))
			Debug.Log($"{name} = {transform.localEulerAngles}");
		if (!_disposed && transform.eulerAngles.z > 200f)
		{
			_disposed = true;
			_bowlingManager.Pins.Remove(gameObject);
			_bowlingManager.ScorePhase++;
			Destroy(gameObject, 2f);
		}
	}
}
