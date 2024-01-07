using Microsoft.MixedReality.Toolkit.Experimental.UI;

using TMPro;

using UnityEngine;

/// <summary>
/// Reponsible for showing the keyboard to enter the room code.
/// </summary>
public class ShowKeyboard : MonoBehaviour
{
	[SerializeField] private TMP_InputField _codeField;

	/// <summary>
	/// Adds the event to the input field for opening the keyboard.
	/// </summary>
	private void Start() => _codeField.onSelect.AddListener(x => OpenKeyboard());

	/// <summary>
	/// Opens the keyboard to write the code of the room.
	/// </summary>
	private void OpenKeyboard()
	{
		NonNativeKeyboard.Instance.InputField = _codeField;
		NonNativeKeyboard.Instance.PresentKeyboard(_codeField.text);
	}
}
