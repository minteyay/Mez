using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI : MonoBehaviour
{
    /// <summary>
    /// Image used to fade the camera in and out of a colour.
    /// </summary>
	public Image fadePanel = null;
    /// <summary>
    /// Time to take fading in and out of the specified colour.
    /// </summary>
	public float fadeTime = 0.0f;

	public delegate void OnComplete();

	void Update()
	{

	}

	public void FadeIn(OnComplete onComplete)
	{
		StartCoroutine(FadeInRoutine(onComplete));
	}

	public void FadeOut(OnComplete onComplete)
	{
		StartCoroutine(FadeOutRoutine(onComplete));
	}

	private IEnumerator FadeInRoutine(OnComplete onComplete)
	{
		float actualFadeTime = (fadeTime > 0f) ? fadeTime : Mathf.Epsilon;
		for (float a = 1f; a >= 0f; a -= Time.deltaTime / actualFadeTime)
		{
			Color c = fadePanel.color;
			c.a = a;
			fadePanel.color = c;
			yield return null;
		}

		Color finalColor = fadePanel.color;
		finalColor.a = 0f;
		fadePanel.color = finalColor;

		onComplete.Invoke();
	}

	private IEnumerator FadeOutRoutine(OnComplete onComplete)
	{
		float actualFadeTime = (fadeTime > 0f) ? fadeTime : Mathf.Epsilon;
		for (float a = 0f; a <= 1f; a += Time.deltaTime / actualFadeTime)
		{
			Color c = fadePanel.color;
			c.a = a;
			fadePanel.color = c;
			yield return null;
		}

		Color finalColor = fadePanel.color;
		finalColor.a = 1f;
		fadePanel.color = finalColor;

		onComplete.Invoke();
	}
}
