using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Responsible for the different behaviour the tutorial GameObject can have.
/// </summary>
public class TutorialManager : MonoBehaviour
{
	[SerializeField] private GameObject _question;
	[SerializeField] private GameObject _bowlingTutorial;

	/// <summary>
	/// Opens the correct tutorial depending on the sport.
	/// </summary>
	public void OpenTutorial()
	{
		_question.SetActive(false);
		switch (SceneManager.GetActiveScene().name)
		{
			case "Bowling":
				_bowlingTutorial.SetActive(true);
				break;
		}
	}

	/// <summary>
	/// Changes the way the player interacts with the objects according to their settings.
	/// </summary>
	public void CloseTutorial()
	{
		GameManager.Instance.PauseMenuManager.IsNotMainMenu = true;
		GameObject.FindWithTag("Player").GetComponent<InteractorManager>().ToggleBehavior(GameManager.Instance.AccessibilityManager.ReducedMobilityMode);
	}
}
