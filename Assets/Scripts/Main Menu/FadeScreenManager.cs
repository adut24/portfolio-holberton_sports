using System.Collections;
using UnityEngine;

public class FadeScreenManager : MonoBehaviour
{
	[SerializeField] private float _fadeDuration = 2.0f;
	[SerializeField] private Color _fadeColor;

	private Renderer _rend;

	public float FadeDuration { get => _fadeDuration; }

	private void Start() => _rend = GetComponent<Renderer>();

	public void FadeIn()
	{
		_rend.material.SetColor("_BaseColor", _fadeColor);
		StartCoroutine(Fade(1, 0));
	}

	public void FadeOut() => StartCoroutine(Fade(0, 1));

	private IEnumerator Fade(float alphaIn, float alphaOut)
	{
		Color newColor;
		float timer = 0;

		while (timer <= _fadeDuration)
		{
			newColor = _fadeColor;
			newColor.a = Mathf.Lerp(alphaIn, alphaOut, timer / _fadeDuration);
			_rend.material.SetColor("_BaseColor", newColor);
			timer += Time.deltaTime;
			yield return null;
		}

		newColor = _fadeColor;
		newColor.a = alphaOut;
		_rend.material.SetColor("_BaseColor", newColor);
	}
}
