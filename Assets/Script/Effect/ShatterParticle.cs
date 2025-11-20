using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class ShatterParticle : MonoBehaviour
{

    [Header("가속 X")]
    public float maxXRange = 5f;
    [Header("가속 Y")]
    public bool isYPositive = false;    // Y가 무조건 양수여야 하는지 여부
    public float maxYRange = 5f;

    [Header("회전")]
    public float AngularSpeed = 360f;

    [Header("크기")]
    public float maxSizeMultiples = 1.1f;
    public float minSizeMultiples = 0.9f;

    [Header("색상")]
    public bool canChangeColor = true;
    public Color minColor = Color.black;
    public Color maxColor = Color.black;

    [Header("사라짐")]
    public float minShrinkDuration = 1.0f;
    public float maxShrinkDuration = 3.0f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector3 initialScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        SetRandomVelocity();
        SetRandomAngularVelocity();
        SetRandomSize();
        if (canChangeColor) SetRandomColor();

        initialScale = transform.localScale;
        StartCoroutine(ShrinkAndDestroySequence(GetRandom(minShrinkDuration, maxShrinkDuration)));
    }

    void SetRandomVelocity()
    {
        float randomX = GetRandom(-maxXRange, maxXRange);
        float randomY = GetRandom(isYPositive ? 0 : -maxYRange, maxYRange); // 으악!!! 삼!항!연!산!!!!!!!!!!!!!!!!!!!!!

        Vector2 randomForce = new Vector2(randomX, randomY);

        rb.velocity = randomForce;
    }

    void SetRandomAngularVelocity()
    {
        float randomAngularVelocity = GetRandom(-AngularSpeed, AngularSpeed);
        rb.angularVelocity = randomAngularVelocity;
    }

    void SetRandomSize()
    {
        transform.localScale = transform.localScale * GetRandom(minSizeMultiples, maxSizeMultiples);
    }

    void SetRandomColor()
    {
        float lerpValue = GetRandom(0.0f, 1.0f);
        Color randomColor = Color.Lerp(minColor, maxColor, lerpValue);
        sr.color = randomColor;
    }


    IEnumerator ShrinkAndDestroySequence(float shrinkDuration)
    {
        if (shrinkDuration > 0)
        {
            float elapsedTime = 0f;

            while (elapsedTime < shrinkDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / shrinkDuration);
                transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

                yield return null;
            }
        }

        Destroy(gameObject);
    }


    float GetRandom(float min, float max)
    {
        if (max == min) return max;
        else if (min < max) return Random.Range(min, max);
        else return Random.Range(max, min);
    }
}
