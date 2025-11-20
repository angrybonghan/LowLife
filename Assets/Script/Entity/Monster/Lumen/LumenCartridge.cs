using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class LumenCartridge : MonoBehaviour
{
    [Header("속도")]
    public float emissionSpeed = 4;

    [Header("회전")]
    public float AngularSpeed = 360f;

    [Header("방향")]
    public bool flyToRight;

    [Header("사라짐")]
    public float existenceTime = 3.0f;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // 가속
        int sigh = flyToRight ? 1 : -1;
        rb.velocity = new Vector2(emissionSpeed * sigh, emissionSpeed);
        // 속도
        float randomAngularVelocity = GetRandom(-AngularSpeed, AngularSpeed);
        rb.angularVelocity = randomAngularVelocity;


        Destroy(gameObject, existenceTime);
    }

    float GetRandom(float min, float max)
    {
        if (max == min) return max;
        else if (min < max) return Random.Range(min, max);
        else return Random.Range(max, min);
    }
}
