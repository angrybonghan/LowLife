using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class DirectionalParticle : MonoBehaviour
{
    [Header("속도 크기")]
    public float minSpeed = 3f;
    public float maxSpeed = 7f;

    [Header("회전")]
    public float AngularSpeed = 360f;

    [Header("색상")]
    public bool canChangeColor = true;
    public Color minColor = Color.black;
    public Color maxColor = Color.black;

    [Header("크기")]
    public float maxSizeMultiples = 1.1f;
    public float minSizeMultiples = 0.9f;

    [Header("사라짐")]
    public float minShrinkDuration = 1.0f;
    public float maxShrinkDuration = 3.0f;

    [HideInInspector] public Vector2 initialDirection; // 생성자에서 설정될 초기 발사 방향
    [HideInInspector] public float spreadAngle = 30f; // 생성자에서 설정될 분산 각도 (총 각도 범위)


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
        // 1. 발사 방향 (initialDirection)을 기준으로 랜덤한 각도를 계산합니다.
        float baseAngle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg; // Radian -> Degree

        // 2. 분산 각도 범위 내에서 랜덤 각도를 선택합니다. (-spreadAngle/2 부터 +spreadAngle/2)
        float randomAngleOffset = GetRandom(-spreadAngle / 2f, spreadAngle / 2f);
        float finalAngle = baseAngle + randomAngleOffset;

        // 3. 최종 각도(Degree)를 다시 Vector2 방향으로 변환합니다.
        // 유니티의 삼각함수는 Radian을 사용하므로 Degree -> Radian으로 변환
        float finalAngleRad = finalAngle * Mathf.Deg2Rad;

        Vector2 randomDirection = new Vector2(
            Mathf.Cos(finalAngleRad),
            Mathf.Sin(finalAngleRad)
        ).normalized; // 단위 벡터 (크기 1)로 정규화

        // 4. 랜덤 속도 크기를 구하고 적용합니다.
        float randomSpeed = GetRandom(minSpeed, maxSpeed);
        rb.velocity = randomDirection * randomSpeed;
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
        if (min < max) return Random.Range(min, max);
        else return Random.Range(max, min);
    }
}
