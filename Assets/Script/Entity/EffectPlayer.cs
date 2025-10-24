using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EffectPlayer : MonoBehaviour
{
    public float fps = 0.1f;
    public Sprite[] frames;
    
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (sr == null)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(StartAnimation());
    }

    IEnumerator StartAnimation()
    {
        for (int i = 0; i < frames.Length; i++)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(fps);
        }

        Destroy(gameObject);
    }
}
