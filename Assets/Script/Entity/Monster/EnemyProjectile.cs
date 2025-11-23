using UnityEngine;

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

    [Header("패링 속도 배수")]
    public float ParrySpeedMultiplier = 1.5f;

    [Header("레이어")]
    public LayerMask afterParryCollisionMask;
    public LayerMask normalCollisionMask;

    bool isParried = false;
    bool isDead = false;

    float castRadius;

    BoxCollider2D boxCol;
    CircleCollider2D cirCol;

    private void Awake()
    {
        boxCol = GetComponent<BoxCollider2D>();
        cirCol = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        if (cirCol != null) castRadius = cirCol.radius;
        else if (boxCol != null) castRadius = Mathf.Min(boxCol.size.x, boxCol.size.y);
    }

    private void Update()
    {
        MoveAndCollide();
    }

    //void ApplyVelocityInFacingDirection()
    //{
    //    Vector2 direction = transform.right;
    //    Vector2 targetVelocity = direction * speed;
    //    rb.velocity = targetVelocity;
    //}

    void Parry()
    {
        speed *= ParrySpeedMultiplier;

        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = currentRotation * Quaternion.Euler(0, 0, 180f);
        transform.rotation = targetRotation;

        isParried = true;

        PlayerController.instance.ParrySuccess();
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
            if (other.CompareTag("Player"))
            {
                return;
            }

            I_Attackable attackableTarget = other.GetComponent<I_Attackable>();
            if (attackableTarget != null)
            {
                attackableTarget.OnAttack(transform);
            }
            isDead = true;
        }

        Destroy(gameObject);
    }

    void MoveAndCollide()
    {
        Vector3 targetDirection = transform.right;
        Vector3 movement = targetDirection * speed * Time.deltaTime;
        float distance = movement.magnitude;

        if (distance <= 0) return;

        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            castRadius,
            movement.normalized,
            distance,
            isParried ? afterParryCollisionMask : normalCollisionMask
        );

        if (hit.collider != null)
        {
            Collider2D other = hit.collider;

            if (!isParried)
            {
                if (hit.collider.gameObject.CompareTag("Player"))
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
                if (other.CompareTag("Player"))
                {
                    return;
                }

                I_Attackable attackableTarget = other.GetComponent<I_Attackable>();
                if (attackableTarget != null)
                {
                    attackableTarget.OnAttack(transform);
                }
                isDead = true;
            }

            Destroy(gameObject);
        }
        else
        {
            transform.position += movement;
        }
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
