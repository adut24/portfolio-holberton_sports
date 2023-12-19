using TMPro;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class ShowKeyboard : MonoBehaviour
{
	[SerializeField] private TMP_InputField _inputField;

	private void Start() => _inputField.onSelect.AddListener(str => OpenKeyboard());

	public void OpenKeyboard()
	{
		NonNativeKeyboard.Instance.InputField = _inputField;
		NonNativeKeyboard.Instance.PresentKeyboard(_inputField.text);
	}
}
