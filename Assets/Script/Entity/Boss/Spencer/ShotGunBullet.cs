using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ShotGunBullet : MonoBehaviour, I_Projectile
{
    [Header("움직임")]
    public float speed = 25f;
    public float moveRange = 3f;
    public float dispersion = 20f;

    [Header("패링 속도 배수")]
    public float ParrySpeedMultiplier = 1.5f;

    [Header("공격")]
    public float damage = 0.1f;
    public float knockbackPower = 1f;
    public float knockbacktime = 0.1f;

    [Header("레이어")]
    public LayerMask afterParryCollisionMask;
    public LayerMask normalCollisionMask;

    bool isParried = false;
    bool isDead = false;

    float castRadius;
    Vector2 originPos;

    public void LookPos(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;
        float angle = Vector2.SignedAngle(Vector2.right, direction);
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Rotate(float angle)
    {
        float currentZ = transform.localEulerAngles.z;

        float newZ = currentZ + angle;

        transform.localRotation = Quaternion.Euler(
            transform.localEulerAngles.x,
            transform.localEulerAngles.y,
            newZ
        );
    }

    private void Start()
    {
        if (dispersion > 0)
        {
            float randomAngle = Random.Range(-dispersion / 2f, dispersion / 2f);
            Rotate(randomAngle);
        }

        castRadius = GetComponent<CircleCollider2D>().radius;
        originPos = transform.position;
    }

    private void Update()
    {
        MoveAndCollide();

        float distance = Vector2.Distance(transform.position, originPos);
        if (distance >= moveRange)
        {
            Destroy(gameObject);
        }
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

    void Parry()
    {
        speed *= ParrySpeedMultiplier;
        Rotate(0b10110100); // 0b10110100 == 180
        isParried = true;
        PlayerController.instance.ParrySuccess();

        originPos = transform.position;
    }

    public void Collision()
    {
        Destroy(gameObject);
    }
}
