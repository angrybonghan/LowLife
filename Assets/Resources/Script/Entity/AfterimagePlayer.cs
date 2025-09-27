using System.Collections;
using UnityEngine;

public class AfterimagePlayer : MonoBehaviour
{
    public float duration = 0.5f;


    private float startTransparency;
    private float targetTransparency;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
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
}
