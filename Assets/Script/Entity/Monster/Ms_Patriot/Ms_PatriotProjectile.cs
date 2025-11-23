using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))]
public class Ms_PatriotProjectile : MonoBehaviour, I_Projectile
{
    [Header("공격")]
    public float explosionRadius = 2.0f;
    public float damage = 1f;
    public float knockbackPower = 1f;
    public float knockbacktime = 0.1f;

    [Header("레이어")]
    public LayerMask explosionTargetLayer;

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

    bool isParried = false;
    bool isDead = false;
    bool wasHitPlayer = false;

    float currentLifeTime = 0;

    private Rigidbody2D rb;
    private Transform targetTransform;
    private AudioSource AS;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        AS = GetComponent<AudioSource>();
    }

    void Start()
    {
        targetTransform = PlayerController.instance.transform;

        AS.loop = true;
        AS.clip = thrusterSound;
        AS.Play();
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
            Explode();
        }
    }

    void RotateTowardsTarget()
    {
        Vector2 directionToTarget = (Vector2)targetTransform.position - rb.position;
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, trackingPower * Time.fixedDeltaTime);
    }

    void Explode()
    {
        isDead = true;

        AS.Stop();

        int randomIndex = Random.Range(0, explosionSound.Length);
        AudioClip clip = explosionSound[randomIndex];
        SoundManager.instance.PlaySoundAtPosition(transform.position, clip);


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

            I_Attackable attackableTarget = other.GetComponent<I_Attackable>();

            if (attackableTarget != null && !other.CompareTag("Player"))
            {
                attackableTarget.OnAttack(transform);
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
            }

            Explode();
        }
        else
        {
            if (other.CompareTag("Player"))
            {
                return;
            }

            Explode();
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
