using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class DeadParts : MonoBehaviour
{
    [Header("파편 움직임")]
    public float thrust = 10f;
    public float rotationThrust = 5f;
    public float shrinkDuration = 3.0f;

    private Rigidbody2D rb;
    private Vector3 initialScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        LaunchRandomly();
        ApplyRandomTorque();

        initialScale = transform.localScale;
        StartCoroutine(ShrinkAndDestroySequence());
    }

    void LaunchRandomly()
    {
        float randomX = GetRandom(-1f, 1f);
        float randomY = GetRandom(0.5f, 1f);

        Vector2 randomDirection = new Vector2(randomX, randomY);
        rb.AddForce(randomDirection.normalized * thrust, ForceMode2D.Impulse);
    }

    void ApplyRandomTorque()
    {
        float randomTorque = GetRandom(-1f, 1f);
        rb.AddTorque(randomTorque * rotationThrust, ForceMode2D.Impulse);
    }

    float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

    private IEnumerator ShrinkAndDestroySequence()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shrinkDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / shrinkDuration);
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

            yield return null;
        }

        Destroy(gameObject);
    }
}