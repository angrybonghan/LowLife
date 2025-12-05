using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpritePlayer : MonoBehaviour
{
    public float fps = 0.05f;
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
        while (true)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                sr.sprite = frames[i];
                yield return new WaitForSeconds(fps);
            }
        }
    }

    public void SetSize(float size)
    {
        Vector2 newSize = new Vector2(size, size);
        transform.localScale = newSize;
    }
}
