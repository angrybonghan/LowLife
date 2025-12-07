using System.Collections;
using UnityEngine;

public class DiffuseLaser : MonoBehaviour
{
    [Header("레이저")]
    public GameObject Laser;

    [Header("조준")]
    public float timeToFire = 0.3f;
    public Color lockOnColor = Color.red;

    [Header("분산")]
    public float laserDispersion = 0f;

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
    public LayerMask afterParryCollisionMask;

    [Header("소리")]
    public AudioClip fireSound;
    public AudioClip aimingSound;

    private const float maxRayDistance = 100f;

    bool isParryed = false;

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = Laser.GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        transform.localScale = new Vector2(transform.localScale.x, laserThickness);
        spriteRenderer.color = lockOnColor;

        StartCoroutine(Co_Fire());
    }

    public void SetDamage(float damage, float knockbackPower, float knockbacktime)
    {
        this.damage = damage;
        this.knockbackPower = knockbackPower;
        this.knockbacktime = knockbacktime;
    }

    IEnumerator Co_Fire()
    {
        if (timeToFire > 0)
        {
            SoundManager.instance.PlaySoundAtPosition(transform.position, aimingSound);

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
        if (isParryed) startColor = InvertColor(startColor);
        else spriteRenderer.color = startColor;

        Vector2 origin = transform.position;
        Vector2 direction = transform.right;
        LayerMask layer = isParryed ? afterParryCollisionMask : collisionMask;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxRayDistance, layer);

        if (hit)
        {
            Collider2D other = hit.collider;

            if (other.CompareTag("Player"))
            {
                PlayerController pc = other.GetComponent<PlayerController>();
                if (pc.IsParried(transform))
                {
                    isParryed = true;

                    transform.localScale = new Vector2(transform.localScale.x, laserThickness * 1.1f);

                    transform.position = hit.point;
                    Rotate(180);
                    RaycastResize();
                    ShootLaser();   // 재귀함수
                    PlayerController.instance.ParrySuccess();
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
                    if (attackableTarget.CanAttack(transform))
                    {
                        attackableTarget.OnAttack(transform);
                    }
                }
            }
        }

        SoundManager.instance.PlaySoundAtPositionWithPitch(transform.position, fireSound, Random.Range(0.9f, 1.1f));
        StartCoroutine(Co_ShrinkAndDestroy());
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

    public Color InvertColor(Color color)
    {
        Color inverted = new Color(
            1.0f - color.r,
            1.0f - color.g,
            1.0f - color.b,
            color.a
        );

        return inverted;
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

    public void LookPos(Vector2 originPos, Vector2 targetPos)
    {
        Vector2 randomOffset = Random.insideUnitCircle.normalized * laserDispersion;
        transform.position = originPos + randomOffset;

        Vector2 randomOffsetTarget = Random.insideUnitCircle.normalized * laserDispersion;
        Vector2 dispersedTargetPos = targetPos + randomOffsetTarget;
        Vector2 direction = dispersedTargetPos - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        transform.rotation = rotation;
    }

    void RaycastResize()
    {
        Vector2 origin = transform.position;
        Vector2 direction = transform.right;

        LayerMask layer = isParryed ? afterParryCollisionMask : collisionMask;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxRayDistance, layer);
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
