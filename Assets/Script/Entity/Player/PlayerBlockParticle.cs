using System.Collections;
using UnityEngine;

public class PlayerBlockParticle : MonoBehaviour
{
    [Header("동작 시간")]
    public float duration = 0.2f;

    [Header("목표 크기")]
    public float targetScale = 1.5f;

    [Header("회전 속도(도/초)")]
    public float rotationSpeed = 360f;

    [Header("색상")]
    public Color startColor = Color.white;
    public Color endColor = new Color(1f, 1f, 1f, 0f);

    SpriteRenderer _spriteRenderer;
    float _currentZAngle;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = Vector3.one * targetScale;

        float elapsed = 0f;
        _currentZAngle = transform.eulerAngles.z;

        transform.localScale = startScale;
        transform.eulerAngles = new Vector3(0f, 0f, _currentZAngle);
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = startColor;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            _currentZAngle += rotationSpeed * Time.deltaTime;
            transform.eulerAngles = new Vector3(0f, 0f, _currentZAngle);

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.Lerp(startColor, endColor, t);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
