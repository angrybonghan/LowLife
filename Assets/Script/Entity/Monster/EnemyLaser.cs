using System.Collections;
using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
    [Header("레이저")]
    public GameObject Laser;

    [Header("조준")]
    public float aimingTime = 2;
    public float timeToFire = 0.3f;
    public Color aimingColor = Color.red;
    public Color lockOnColor = Color.red;

    [Header("공격")]
    public float damage = 0.3f;
    public float knockbackPower = 1f;
    public float knockbacktime = 0.1f;

    [Header("크기")]
    public float laserThickness = 0.2f;

    [Header("사라짐")]
    public float shrinkTime = 0.1f;
    public Color startColor = Color.red;
    public Color endColor = Color.white;

    [Header("레이어")]
    public LayerMask collisionMask;

    private const float maxRayDistance = 100f;

    bool hasOrigin = false;
    bool hasTarget = false;

    Transform originTransform;
    Transform target;
    Vector2 targetPos;

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = Laser.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        transform.localScale = new Vector2(transform.localScale.x, laserThickness);
        spriteRenderer.color = aimingColor;

        if (aimingTime > 0) StartCoroutine(Co_Aiming());
        else StartCoroutine(Co_Fire());
    }

    void Update()
    {
        if (hasOrigin)
        {
            if (originTransform != null) transform.position = originTransform.position;
            else Destroy(gameObject);
        }

        if (hasTarget)
        {
            if (target == null) Destroy(gameObject);
        }
    }

    public void SetOrigin(Transform target)
    {
        if (target != null) originTransform = target;
        hasOrigin = true;
    }

    public void SetTarget(Transform target)
    {
        if (target != null) this.target = target;
        hasTarget = true;
    }

    public void SetDamage(float damage, float knockbackPower, float knockbacktime)
    {
        this.damage = damage;
        this.knockbackPower = knockbackPower;
        this.knockbacktime = knockbacktime;
    }


    IEnumerator Co_Aiming()
    {
        float elapsedTime = 0f;

        while (elapsedTime < aimingTime)
        {
            if (target != null) targetPos = target.position;
            LookPos(targetPos);
            RaycastResize();

            float currentThickness = Mathf.Lerp(0, laserThickness, elapsedTime / aimingTime);
            transform.localScale = new Vector2(transform.localScale.x, currentThickness);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(Co_Fire());
    }

    IEnumerator Co_Fire()
    {
        if (target != null) targetPos = target.position;
        LookPos(targetPos);

        if (timeToFire > 0)
        {
            spriteRenderer.color = lockOnColor;
            float elapsedTime = 0f;
            while (elapsedTime < timeToFire)
            {
                RaycastResize();
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        RaycastResize();
        ShootLaser();
    }

    void ShootLaser()
    {
        spriteRenderer.color = startColor;

        Vector2 origin = transform.position;
        Vector2 direction = transform.right;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxRayDistance, collisionMask);

        if (hit)
        {
            Collider2D other = hit.collider;

            if (other.CompareTag("Player"))
            {
                PlayerController pc = other.GetComponent<PlayerController>();
                if (pc.IsParried(transform))
                {
                    return;
                }
                else
                {
                    pc.OnAttack(damage, knockbackPower, knockbacktime, transform);
                }
            }
            else
            {
                I_Attackable attackableTarget = other.GetComponent<I_Attackable>();
                if (attackableTarget != null)
                {
                    attackableTarget.OnAttack(transform);
                }
            }
        }

        StartCoroutine(Co_ShrinkAndDestroy());
    }

    IEnumerator Co_ShrinkAndDestroy()
    {
        float elapsedTime = 0f;
        float startThickness = transform.localScale.y;

        while (elapsedTime < shrinkTime)
        {
            float t = elapsedTime / shrinkTime;

            float currentThickness = Mathf.Lerp(startThickness, 0f, t);
            transform.localScale = new Vector2(transform.localScale.x, currentThickness);

            spriteRenderer.color = Color.Lerp(startColor, endColor, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    void LookPos(Vector2 pos)
    {
        Vector2 direction = targetPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        transform.rotation = rotation;
    }

    void RaycastResize()
    {
        Vector2 origin = transform.position;
        Vector2 direction = transform.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxRayDistance, collisionMask);
        float distanceToSet = maxRayDistance;

        if (hit)
        {
            distanceToSet = hit.distance;
        }

        Vector3 newScale = transform.localScale;
        newScale.x = distanceToSet;
        transform.localScale = newScale;
    }

    
}
