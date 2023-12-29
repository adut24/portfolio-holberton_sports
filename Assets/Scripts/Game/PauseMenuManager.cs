using Photon.Pun;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Responsible for managing the pause menu behaviour.
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
	/// <summary>
	/// Gets or sets if the client is in the main menu scene or not.
	/// </summary>
	public bool IsNotMainMenu { get; set; }

	/// <summary>
	/// Gets or sets the component to write the title on the pause menu.
	/// </summary>
	public TextMeshProUGUI TitleText { get; set; }

	/// <summary>
	/// Gets or sets the child containing the accessibility menu.
	/// </summary>
	public GameObject AccessibilityMenu { get; set; }

	/// <summary>
	/// Gets or sets the child containing the options menu.
	/// </summary>
	public GameObject OptionsMenu { get; set; }

	/// <summary>
	/// Gets or sets the child containing the buttons displayed in the pause menu.
	/// </summary>
	public GameObject PauseButtons { get; set; }

	/// <summary>
	/// Gets or sets the pause menu GameObject.
	/// </summary>
	public GameObject PauseMenu { get; set; }

	/// <summary>
	/// Gets or sets the component managing the fade in / fade out system.
	/// </summary>
	public InteractorManager InteractorManager { get; set; }

	[SerializeField] private GameObject _menuCanvas;
	[SerializeField] private TextMeshProUGUI _titleText;
	[SerializeField] private GameObject _accessibilityMenu;
	[SerializeField] private GameObject _optionsMenu;
	[SerializeField] private GameObject _pauseMenu;
	[SerializeField] private GameObject _pauseButtons;
	[SerializeField] private InteractorManager _interactorManager;
	[SerializeField] private InputActionReference _menuButton;

	private bool _isPaused = false;
	private RectTransform _menuRectTransform;
	private Vector3 _originalPosition;
	private const string PAUSE = "PAUSE";

	/// <summary>
	/// Called when the object becomes enabled and active. Enables the possibility to press the start button.
	/// </summary>
	private void OnEnable() => _menuButton.action.Enable();

	/// <summary>
	/// Called when the object becomes disabled or inactive. Disables the possibility to press the start button.
	/// </summary>
	private void OnDisable() => _menuButton.action.Disable();

	/// <summary>
	/// Called when the instance is being loaded.
	/// </summary>
	private void Awake()
	{
		TitleText = _titleText;
		AccessibilityMenu = _accessibilityMenu;
		OptionsMenu = _optionsMenu;
		PauseMenu = _pauseMenu;
		PauseButtons = _pauseButtons;
		InteractorManager = _interactorManager;
	}

	/// <summary>
	/// Called every frame. Checks if the player pressed the start button or not and if they aren't in the main menu scene.
	/// </summary>
	private void Update()
	{
		if (_menuButton.action.triggered && IsNotMainMenu)
			ManagePauseMenu();
	}

	/// <summary>
	/// Manages the state of the pause menu depending on if the game is paused or not.
	/// </summary>
	public void ManagePauseMenu()
	{
		if (!_isPaused)
		{
			_isPaused = true;
			PauseMenu.SetActive(true);
			InteractorManager.ToggleMenuBehavior();
			if (PhotonNetwork.OfflineMode)
				Time.timeScale = 0f;
		}
		else
		{
			_isPaused = false;
			HideMenu();
			InteractorManager.ToggleMenuBehavior(GameManager.Instance.AccessibilityManager.ReducedMobilityMode);
			if (PhotonNetwork.OfflineMode)
				Time.timeScale = 1f;
		}
	}

	/// <summary>
	/// Reloads the scene to restart the game that was in progress.
	/// </summary>
	public void RestartGame()
	{
		if (_menuRectTransform == null)
		{
			_menuRectTransform = PauseMenu.transform as RectTransform;
			_originalPosition = _menuRectTransform.anchoredPosition3D;
		}

		ManagePauseMenu();
		PauseMenu.transform.SetParent(null);
		DontDestroyOnLoad(PauseMenu);

		StartCoroutine(GameManager.Instance.NetworkManager.LoadPlayer());
		_menuRectTransform.anchoredPosition3D = _originalPosition;
		_menuRectTransform.rotation = Quaternion.Euler(0, 0, 0);
	}

	/// <summary>
	/// Returns to the main menu. If the player leaving the room is the one that created it, the room is closed.
	/// </summary>
	public void ReturnToMenu()
	{
		if (PhotonNetwork.OfflineMode)
			Time.timeScale = 1f;
		if (PhotonNetwork.IsMasterClient)
			PhotonNetwork.CurrentRoom.IsOpen = false;
		PhotonNetwork.LeaveRoom();
	}
	
	/// <summary>
	/// Sets the return from the options menu correctly depending on where the player is.
	/// </summary>
	public void ReturnFromOptions()
	{
		if (_isPaused)
		{
			OptionsMenu.SetActive(false);
			PauseButtons.SetActive(true);
			TitleText.text = PAUSE;
		}
		else
		{
			OptionsMenu.SetActive(false);
			PauseButtons.SetActive(true);
			TitleText.text = PAUSE;
			PauseMenu.SetActive(false);
			_menuCanvas.SetActive(true);
		}
	}

	/// <summary>
	/// Resets the GameObject to its base state and hides it.
	/// </summary>
	private void HideMenu()
	{
		TitleText.text = PAUSE;
		AccessibilityMenu.SetActive(false);
		OptionsMenu.SetActive(false);
		PauseButtons.SetActive(true);
		PauseMenu.SetActive(false);
	}
}
