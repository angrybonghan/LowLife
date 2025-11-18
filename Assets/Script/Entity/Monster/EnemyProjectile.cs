using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour, I_Projectile
{
    [Header("공격")]
    public float damage = 0.3f;
    public float knockbackPower = 1f;
    public float knockbacktime = 0.1f;

    [Header("이동")]
    public float speed = 5.0f;
    public float lifeTime = 10;

    [Header("패링 후 제외 레이어")]
    public LayerMask afterParryLayer;

    bool isParried = false;
    bool isDead = false;

    Rigidbody2D rb;
    BoxCollider2D boxCol;
    CircleCollider2D cirCol;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCol = GetComponent<BoxCollider2D>();
        cirCol = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        ApplyVelocityInFacingDirection();
    }

    void ApplyVelocityInFacingDirection()
    {
        Vector2 direction = transform.right;
        Vector2 targetVelocity = direction * speed;
        rb.velocity = targetVelocity;
    }

    void Parry()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = currentRotation * Quaternion.Euler(0, 0, 180f);
        transform.rotation = targetRotation;

        isParried = true;

        if (boxCol != null) boxCol.excludeLayers = afterParryLayer;
        if (cirCol != null) cirCol.excludeLayers = afterParryLayer;
    }

    public void Collision()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (!isParried)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController pc = other.GetComponent<PlayerController>();
                if (pc.IsParried(transform))
                {
                    Parry();
                    return;
                }
                else
                {
                    pc.OnAttack(damage, knockbackPower, knockbacktime, transform);
                    isDead = true;
                }
            }
        }
        else
        {
            I_Attackable attackableTarget = other.GetComponent<I_Attackable>();
            if (attackableTarget != null)
            {
                attackableTarget.OnAttack(transform);
            }
            isDead = true;
        }

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
