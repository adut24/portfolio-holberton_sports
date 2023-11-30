using Photon.Pun;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
	[SerializeField] private InputActionReference _menuButtonPressed;
	public GameObject PauseMenu { get; set; }

	private void Start() => PauseMenu = GameObject.FindWithTag("Player").GetComponentInChildren<Canvas>(true).gameObject;

	public void ActivatePauseMenu()
	{
		ManagePauseMenu();
	}

	private void ManagePauseMenu()
	{ 
		PauseMenu.SetActive(!PauseMenu.activeSelf);
		if (PhotonNetwork.OfflineMode)
			Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
	}

	public void RestartGame()
	{
		ManagePauseMenu();
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void ReturnToMenu()
	{
		Destroy(GameObject.FindWithTag("GameManager"));
		SceneManager.LoadScene("MainMenu");
	}
}
