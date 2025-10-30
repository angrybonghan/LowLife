using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class ShatterParticle : MonoBehaviour
{

    [Header("���� X")]
    public float maxXRange = 5f;
    [Header("���� Y")]
    public bool isYPositive = false;
    public float maxYRange = 5f;

    [Header("ȸ��")]
    public float AngularSpeed = 360f;

    [Header("����")]
    public bool canChangeColor = true;
    public Color minColor = Color.black;
    public Color maxColor = Color.black;

    [Header("�����")]
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
        if (canChangeColor) SetRandomColor();

        initialScale = transform.localScale;
        StartCoroutine(ShrinkAndDestroySequence(GetRandom(minShrinkDuration, maxShrinkDuration)));
    }

    void SetRandomVelocity()
    {
        float randomX = GetRandom(-maxXRange, maxXRange);
        float randomY = GetRandom(isYPositive ? 0 : -maxYRange, maxYRange); // ����!!! ��!��!��!��!!!!!!!!!!!!!!!!!!!!!

        Vector2 randomForce = new Vector2(randomX, randomY);

        rb.velocity = randomForce;
    }

    void SetRandomAngularVelocity()
    {
        float randomAngularVelocity = GetRandom(-AngularSpeed, AngularSpeed);
        rb.angularVelocity = randomAngularVelocity;
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
        if (max == min)
        {
            return max;
        }
        else if (min < max)
        {
            return Random.Range(min, max);
        }
        else
        {
            return Random.Range(max, min);
        }
    }
}
