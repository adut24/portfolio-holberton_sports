using UnityEngine;

/// <summary>
/// Responsible for the different behaviour the tutorial GameObject can have.
/// </summary>
public class TutorialManager : MonoBehaviour
{
	/// <summary>
	/// Changes the way the player interacts with the objects according to their settings.
	/// </summary>
	public void CloseTutorial()
	{
		GameManager.Instance.PauseMenuManager.IsNotMainMenu = true;
		GameObject.FindWithTag("Player").GetComponent<InteractorManager>().ToggleBehavior(GameManager.Instance.AccessibilityManager.ReducedMobilityMode);
	}
}
