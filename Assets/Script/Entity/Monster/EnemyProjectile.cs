using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("공격")]
    public float damage = 0.3f;
    public float knockbackPower = 1f;
    public float knockbacktime = 0.1f;

    [Header("이동")]
    public float speed = 5.0f;
    public float lifeTime = 10;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        ApplyVelocityInFacingDirection();
    }

    void ApplyVelocityInFacingDirection()
    {
        Vector2 direction = transform.right;
        Vector2 targetVelocity = direction * speed;
        rb.velocity = targetVelocity;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            pc.OnAttack(damage, knockbackPower, knockbacktime, transform);
        }

        Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    public void SetTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

         transform.rotation = targetRotation;
    }

    public void SetFacing(Vector2 target)
    {
        Vector2 direction = target - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = targetRotation;
    }

    public void SetRotationFrom(Transform sourceTransform)
    {
        transform.rotation = sourceTransform.rotation;
    }
}
