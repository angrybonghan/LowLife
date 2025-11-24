using UnityEngine;
using System.Collections;

public class Sprite_Animator : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private Coroutine colorChangeCoroutine;
    private Coroutine scaleChangeCoroutine;
    private Coroutine alphaChangeCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"SpriteRenderer 컴포넌트 없음 : [{gameObject.name}]");
            enabled = false;
        }
    }

    public void SetColor(Color targetColor, float duration)
    {
        if (colorChangeCoroutine != null)
        {
            StopCoroutine(colorChangeCoroutine);
        }

        colorChangeCoroutine = StartCoroutine(ChangeColorCoroutine(targetColor, duration));
    }

    private IEnumerator ChangeColorCoroutine(Color targetColor, float duration)
    {
        if (duration > 0)
        {
            Color startColor = spriteRenderer.color;
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                spriteRenderer.color = Color.Lerp(startColor, targetColor, timeElapsed / duration);

                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        spriteRenderer.color = targetColor;
        colorChangeCoroutine = null;
    }

    public void SetScale(float targetScale, float duration)
    {
        if (scaleChangeCoroutine != null)
        {
            StopCoroutine(scaleChangeCoroutine);
        }

        scaleChangeCoroutine = StartCoroutine(ChangeScaleCoroutine(targetScale, duration));
    }

    private IEnumerator ChangeScaleCoroutine(float targetScale, float duration)
    {
        if (duration > 0)
        {
            Vector3 startScale = transform.localScale;
            Vector3 targetVectorScale = new Vector3(targetScale, targetScale, targetScale);
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                transform.localScale = Vector3.Lerp(startScale, targetVectorScale, timeElapsed / duration);

                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        transform.localScale = new Vector3(targetScale, targetScale, targetScale);
        scaleChangeCoroutine = null;
    }

    public void SetAlpha(float targetAlpha, float duration)
    {
        if (alphaChangeCoroutine != null)
        {
            StopCoroutine(alphaChangeCoroutine);
        }

        alphaChangeCoroutine = StartCoroutine(ChangeAlphaCoroutine(targetAlpha, duration));
    }

    private IEnumerator ChangeAlphaCoroutine(float targetAlpha, float duration)
    {
        if (duration > 0)
        {
            Color currentColor = spriteRenderer.color;
            float startAlpha = currentColor.a;
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                currentColor.a = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / duration);
                spriteRenderer.color = currentColor;

                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        Color finalColor = spriteRenderer.color;
        finalColor.a = targetAlpha;
        spriteRenderer.color = finalColor;
        alphaChangeCoroutine = null;
    }

    public void SetSprite(Sprite newSprite)
    {
        spriteRenderer.sprite = newSprite;
    }
}