using Photon.Pun;

using TMPro;

using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
	public TextMeshProUGUI TitleText { get; set; }
	public GameObject AccessibilityMenu { get; set; }
	public GameObject OptionsMenu { get; set; }
	public GameObject PauseMenu { get; set; }
	public GameObject PauseButtons { get; set; }
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

	private void OnEnable() => _menuButton.action.Enable();

	private void OnDisable() => _menuButton.action.Disable();

	private void Awake()
	{
		TitleText = _titleText;
		AccessibilityMenu = _accessibilityMenu;
		OptionsMenu = _optionsMenu;
		PauseMenu = _pauseMenu;
		PauseButtons = _pauseButtons;
		InteractorManager = _interactorManager;
	}

	private void Update()
	{
		if (_menuButton.action.triggered)
			ManagePauseMenu();
	}

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

	public void ReturnToMenu()
	{
		if (PhotonNetwork.OfflineMode)
			Time.timeScale = 1f;
		if (PhotonNetwork.IsMasterClient)
			PhotonNetwork.CurrentRoom.IsOpen = false;
		PhotonNetwork.LeaveRoom();
	}

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

	private void HideMenu()
	{
		TitleText.text = PAUSE;
		AccessibilityMenu.SetActive(false);
		OptionsMenu.SetActive(false);
		PauseButtons.SetActive(true);
		PauseMenu.SetActive(false);
	}
}
