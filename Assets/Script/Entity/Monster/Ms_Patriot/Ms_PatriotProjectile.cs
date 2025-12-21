using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(BoxCollider2D))]
public class Ms_PatriotProjectile : MonoBehaviour, I_Projectile
{
    [Header("공격")]
    public float explosionRadius = 2.0f;
    public float damage = 1f;
    public float knockbackPower = 1f;
    public float knockbacktime = 0.1f;

    [Header("레이어")]
    public LayerMask explosionTargetLayer;
    public LayerMask afterParryCollisionMask;
    public LayerMask normalCollisionMask;

    [Header("이동")]
    public float speed = 5.0f;
    public float lifeTime = 10;
    public float trackingPower = 1f;

    [Header("패링 속도 배수")]
    public float ParrySpeedMultiplier = 1.5f;

    [Header("이펙트")]
    public GameObject explosionEffect;

    [Header("사운드")]
    public AudioClip thrusterSound;
    public AudioClip[] explosionSound;

    [Header("팀 킬")]
    public bool canHitEntity = true;

    bool isParried = false;
    bool isDead = false;
    bool wasHitPlayer = false;

    float currentLifeTime = 0;
    float castRadius;

    private Transform targetTransform;
    private AudioSource AS;

    private void Awake()
    {
        AS = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (PlayerController.instance != null) targetTransform = PlayerController.instance.transform;

        AS.loop = true;
        AS.clip = thrusterSound;
        AS.Play();

        BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
        castRadius = Mathf.Min(boxCol.size.x, boxCol.size.y);
    }

    void Update()
    {
        if (!isParried && targetTransform != null)
        {
            RotateTowardsTarget();
        }

        MoveAndCollide();

        currentLifeTime += Time.deltaTime;
        if (currentLifeTime >= lifeTime)
        {
            Explode();
        }
    }

    void RotateTowardsTarget()
    {
        Vector2 directionToTarget = (Vector2)(targetTransform.position - transform.position);
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, trackingPower * Time.deltaTime);
    }

    void Explode()
    {
        if (isDead) return;
        isDead = true;

        AS.Stop();

        int randomIndex = Random.Range(0, explosionSound.Length);
        AudioClip clip = explosionSound[randomIndex];
        AudioManager.instance.PlaySoundAtPosition(transform.position, clip);


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

                if (pc != null && !wasHitPlayer)
                {
                    pc.OnAttack(damage, knockbackPower, knockbacktime, transform);
                    wasHitPlayer = true;
                }
                continue;
            }

            if (canHitEntity)
            {
                I_Attackable attackableTarget = other.GetComponent<I_Attackable>();

                if (attackableTarget != null && !other.CompareTag("Player"))
                {
                    attackableTarget.OnAttack(transform);
                }
            }
            
        }

        EffectPlayer ep = Instantiate(explosionEffect, transform.position, Quaternion.identity).GetComponent<EffectPlayer>();
        ep.SetSize(2f);

        Destroy(gameObject);
    }

    public void Collision()
    {
        Explode();
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
                }
            }
            else
            {
                if (other.CompareTag("Player"))
                {
                    return;
                }

                if (canHitEntity)
                {
                    I_Attackable attackableTarget = other.GetComponent<I_Attackable>();
                    if (attackableTarget != null)
                    {
                        attackableTarget.OnAttack(transform);
                    }
                }

            }

            Explode();
        }
        else
        {
            transform.position += movement;
        }
    }

    void Parry()
    {
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = currentRotation * Quaternion.Euler(0, 0, 180f);
        transform.rotation = targetRotation;

        speed *= ParrySpeedMultiplier;
        currentLifeTime = 0;
        isParried = true;

        PlayerController.instance.ParrySuccess();
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
