using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GradientAfterimagePlayer : MonoBehaviour
{
    public float duration = 0.2f;

    public Color startColor;
    public Color endColor;


    private float startTransparency;
    private float targetTransparency;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = startColor;
    }
    void Start()
    {
        startTransparency = sr.color.a;
        targetTransparency = 0;
        StartCoroutine(AfterImage());
    }

    IEnumerator AfterImage()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float newTransparency = Mathf.Lerp(startTransparency, targetTransparency, elapsedTime / duration);
            Color newColor = sr.color;
            newColor.a = newTransparency;
            sr.color = newColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public void SetColorLevel(float value)
    {
        float t = Mathf.Clamp01(value);
        sr.color = Color.Lerp(startColor, endColor, t);
    }
}
