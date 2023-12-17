using UnityEngine;

public class TutorialManager : MonoBehaviour
{
	public void CloseTutorial()
	{
		GameManager gameManager = GameManager.Instance;
		gameManager.Player.GetComponent<InteractorManager>().ToggleMenuBehavior(gameManager.AccessibilityManager.ReducedMobilityMode);
	}
}
