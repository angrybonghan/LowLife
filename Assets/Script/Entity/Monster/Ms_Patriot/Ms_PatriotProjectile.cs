using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ms_PatriotProjectile : MonoBehaviour
{
    [Header("공격")]
    public float explosionRadius = 2.0f;
    public float damage = 1f;
    public float knockbackPower = 1f;
    public float knockbacktime = 0.1f;

    [Header("레이어")]
    public LayerMask explosionTargetLayer;
    public LayerMask enemyExplosionTargetLayer;

    [Header("이동")]
    public float speed = 5.0f;
    public float lifeTime = 10;
    public float trackingPower = 1f;

    [Header("이펙트")]
    public GameObject explosionEffect;

    bool isParried = false;

    float currentLifeTime = 0;

    private Rigidbody2D rb;
    private Transform targetTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        targetTransform = PlayerController.instance.transform;
    }

    void Update()
    {
        if (!isParried && targetTransform != null)
        {
            RotateTowardsTarget();
        }

        Vector2 forwardDirection = transform.right;
        rb.velocity = forwardDirection * speed;

        currentLifeTime += Time.deltaTime;
        if (currentLifeTime >= lifeTime)
        {
            if (isParried) ParryExplode();
            else Explode();
        }
    }

    void RotateTowardsTarget()
    {
        Vector2 directionToTarget = (Vector2)targetTransform.position - rb.position;
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, trackingPower * Time.fixedDeltaTime);
    }

    void ParryExplode()
    {
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(
            transform.position,                 // 중심 위치
            explosionRadius,                    // 반경
            enemyExplosionTargetLayer                // 감지할 레이어 마스크
        );

        foreach (Collider2D other in hitTargets)
        {
            I_Attackable attackableTarget = other.GetComponent<I_Attackable>();

            if (attackableTarget != null && !other.CompareTag("Player"))
            {
                attackableTarget.OnAttack(transform);
            }
        }

        Destroy(gameObject);
    }

    void Explode()
    {
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(
            transform.position,                 // 중심 위치
            explosionRadius,                    // 반경
            explosionTargetLayer                // 감지할 레이어 마스크
        );

        foreach (Collider2D other in hitTargets)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController pc = other.GetComponent<PlayerController>();

                if (pc != null)
                {
                    pc.OnAttack(damage, knockbackPower, knockbacktime, transform);
                    break;
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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
            }

            Explode();
        }
        else
        {
            ParryExplode();
        }

        Destroy(gameObject);
    }

    void Parry()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = currentRotation * Quaternion.Euler(0, 0, 180f);
        transform.rotation = targetRotation;

        currentLifeTime = 0;
        isParried = true;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
