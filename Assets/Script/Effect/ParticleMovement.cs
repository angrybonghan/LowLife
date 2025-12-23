using System.Collections;
using UnityEngine;

public class ParticleMovement : MonoBehaviour
{
    [Header("위치")]
    public bool canMove = false;
    public Vector3 targetMove;

    [Header("크기")]
    public bool canScaleSize = false;
    public Vector3 targetSize;

    [Header("회전")]
    public bool canRotate = false;
    public Vector3 targetRotation;

    [Header("스프라이트")]
    public bool canUseSprite = false;
    public Color targetColor;

    [Header("시간")]
    public float duration = 0.5f;

    SpriteRenderer SR;

    void Start()
    {
        if (duration <= 0)
        {
            Destroy(gameObject);
            return;
        }

        if (canMove)
        {
            Vector3 endpos = transform.position + targetMove;
            StartCoroutine(Move(transform.position, endpos));
        }
        if (canScaleSize)
        {
            StartCoroutine(Size(transform.localScale, targetSize));
        }
        if (canRotate)
        {
            StartCoroutine(Rotate(transform.rotation.eulerAngles, targetRotation));
        }
        if (canUseSprite)
        {
            if (TryGetComponent<SpriteRenderer>(out SR))
            {
                StartCoroutine(Sprite(SR.color, targetColor));
            }
        }

        StartCoroutine(Destroy());
    }

    IEnumerator Move(Vector3 start, Vector3 end)
    {
        float time = 0;
        while (time < duration)
        {
            transform.position = Vector3.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Size(Vector3 start, Vector3 end)
    {
        float time = 0;
        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Rotate(Vector3 start, Vector3 end)
    {
        float time = 0;
        while (time < duration)
        {
            Vector3 currentEuler = Vector3.Lerp(start, end, time / duration);
            transform.rotation = Quaternion.Euler(currentEuler);
            time += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Sprite(Color start, Color end)
    {
        float time = 0;
        while (time < duration)
        {
            SR.color = Color.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(duration);
        yield return null;
        Destroy(gameObject);
    }
}
