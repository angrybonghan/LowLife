using UnityEngine;

public class SpencerWebProjectile : MonoBehaviour, I_Projectile
{
    [Header("이동")]
    public float speed = 5.0f;
    public float lifeTime = 10;

    [Header("레이어")]
    public LayerMask CollisionMask;

    bool isDead = false;

    float castRadius;

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        CircleCollider2D cirCol = GetComponent<CircleCollider2D>();

        if (cirCol != null) castRadius = cirCol.radius;
        else if (boxCol != null) castRadius = Mathf.Min(boxCol.size.x, boxCol.size.y);
    }

    private void Update()
    {
        MoveAndCollide();
    }

    public void Collision()
    {
        Destroy(gameObject);
    }

    void MoveAndCollide()
    {
        if (isDead) return;

        Vector3 targetDirection = transform.right;
        Vector3 movement = targetDirection * speed * Time.deltaTime;
        float distance = movement.magnitude;

        if (distance <= 0) return;

        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            castRadius,
            movement.normalized,
            distance,
            CollisionMask
        );

        if (hit.collider != null)
        {
            Collider2D other = hit.collider;

            if (hit.collider.gameObject.CompareTag("Player"))
            {
                PlayerController pc = other.GetComponent<PlayerController>();
                if (!pc.IsParried(transform) && !pc.IsBlocked(transform))
                {
                    SpencerManager.Instance.StartSlownessPlayer();
                    isDead = true;
                }
            }

            Collision();
        }
        else
        {
            transform.position += movement;
        }
    }

    public void LookPos(Vector2 target)
    {
        Vector2 direction = target - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = targetRotation;
    }

    public void DontLookPos(Vector2 target)
    {
        Vector2 direction = (Vector2)transform.position - target;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = targetRotation;
    }

    public void SetRotationFrom(Transform sourceTransform)
    {
        transform.rotation = sourceTransform.rotation;
    }
}
