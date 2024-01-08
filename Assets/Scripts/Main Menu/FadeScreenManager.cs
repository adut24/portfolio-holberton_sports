using System.Collections;

using UnityEngine;

/// <summary>
/// Responsible for the screen fading animations.
/// </summary>
public class FadeScreenManager : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 2.0f;
    [SerializeField] private Color _fadeColor;

    private Renderer _rend;

    /// <summary>
    /// Gets the fade duration for the animation.
    /// </summary>
    public float FadeDuration { get => _fadeDuration; }

    /// <summary>
    /// Initializes the renderer component during the start of the script.
    /// </summary>
    private void Start() => _rend = GetComponent<Renderer>();

    /// <summary>
    /// Initiates a fade-in animation.
    /// </summary>
    public void FadeIn()
    {
        _rend.material.SetColor("_BaseColor", _fadeColor);
        StartCoroutine(Fade(1, 0));
    }

    /// <summary>
    /// Initiates a fade-out animation.
    /// </summary>
    public void FadeOut() => StartCoroutine(Fade(0, 1));

    /// <summary>
    /// Performs a fade animation between specified alpha values.
    /// </summary>
    /// <param name="alphaIn">The starting alpha value.</param>
    /// <param name="alphaOut">The target alpha value.</param>
    /// <returns>Null to wait for the next frame.</returns>
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
