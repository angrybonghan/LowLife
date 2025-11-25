using System.Collections;
using TMPro;
using UnityEngine;

public class TMPugui_Animator : MonoBehaviour
{
    private TextMeshProUGUI tmpText;

    private Coroutine colorChangeCoroutine;
    private Coroutine sizeChangeCoroutine;
    
    // TMP가 무슨 뜻이게~
    // 'T'he 'M'ain 'P'roblem
    // 문제가 아주크다 이거야 ㅇㅇ

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        if (tmpText == null)
        {
            Debug.LogError($"TextMeshProUGUI 없음 : [{gameObject.name}]");
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

    IEnumerator ChangeColorCoroutine(Color targetColor, float duration)
    {
        if (duration > 0)
        {
            Color startColor = tmpText.color;
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                tmpText.color = Color.Lerp(startColor, targetColor, timeElapsed / duration);

                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        tmpText.color = targetColor;
        colorChangeCoroutine = null;
    }

    public void SetSize(float targetSize, float duration)
    {
        if (sizeChangeCoroutine != null)
        {
            StopCoroutine(sizeChangeCoroutine);
        }

        sizeChangeCoroutine = StartCoroutine(ChangeSizeCoroutine(targetSize, duration));
    }

    IEnumerator ChangeSizeCoroutine(float targetSize, float duration)
    {
        if (duration > 0)
        {
            float startSize = tmpText.fontSize;
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                tmpText.fontSize = Mathf.Lerp(startSize, targetSize, timeElapsed / duration);

                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        tmpText.fontSize = targetSize;
        sizeChangeCoroutine = null;
    }

    public void SetText(string newText)
    {
        tmpText.text = newText;
    }
}
