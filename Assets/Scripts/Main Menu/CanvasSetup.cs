using UnityEngine;

public class CanvasSetup : MonoBehaviour
{
	private void Start()
	{
		Canvas canvas = GetComponent<Canvas>();

		if (canvas.worldCamera == null)
			canvas.worldCamera = FindObjectOfType<Camera>();
	}
}
