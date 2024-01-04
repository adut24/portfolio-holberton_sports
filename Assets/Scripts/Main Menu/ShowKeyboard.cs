using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using UnityEngine;

public class ShowKeyboard : MonoBehaviour
{
	[SerializeField] private TMP_InputField _codeField;

	private void Start() => _codeField.onSelect.AddListener(x => OpenKeyboard());

	public void OpenKeyboard()
	{
		NonNativeKeyboard.Instance.InputField = _codeField;
		NonNativeKeyboard.Instance.PresentKeyboard(_codeField.text);
	}
}
