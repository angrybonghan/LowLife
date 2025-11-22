using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PolygonCollider2D), typeof(Animator))]
public class GrubBody : MonoBehaviour, I_Attackable
{
    [Header("히트박스")]
    public float attackRange = 0.075f;
    public LayerMask targetLayer;

    [Header("대미지")]
    public float damage = 0.4f;
    public float knockbackPower = 1f;
    public float knockbacktime = 0.05f;

    [Header("사망 후 제외 레이어")]
    public LayerMask afterDeathLayer;

    bool canAttack = false;
    bool isDead = false;

    PolygonCollider2D polyCol;
    Rigidbody2D rb;
    Animator anim;

    GrubBodyController myLord; // 예쓰 마이 로드!!!, 예쓰!!!!!!!!!!!!!!!!

    private void Awake()
    {
        polyCol = GetComponent<PolygonCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        polyCol.enabled = false;
        rb.gravityScale = 0;
    }

    private void Update()
    {
        if (isDead) return;
        if (canAttack) AttackHander();
    }

    void AttackHander()
    {
        Collider2D hitTargets = Physics2D.OverlapCircle(
            transform.position,
            attackRange,
            targetLayer
            );

        if (hitTargets != null)
        {
            if (hitTargets.TryGetComponent<PlayerController>(out PlayerController pc))
            {
                if (pc.IsParried(transform))
                {
                    if (isDead) return;
                    isDead = true;
                    myLord.Dead(PlayerController.instance.transform);
                    return;
                }
                else
                {
                    pc.OnAttack(damage, knockbackPower, knockbacktime, transform);
                    canAttack = false;
                }
            }
            else
            {
                Debug.LogWarning($"플레이어 태그를 가졌지만 스크립트가 없는 대상: {hitTargets.gameObject.name}");
            }
        }
    }

    // 그들의 신을 지정하다
    public void AppointTheirGod(GrubBodyController lord)
    {
        // 오 하느님!!
        myLord = lord;
    }

    public void AttackEnable(bool isEnable)
    {
        polyCol.enabled = isEnable;
        canAttack = isEnable;
    }


    public void Dead(Vector3 flyVelocity, float deathDuration)
    {
        isDead = true;

        anim.SetTrigger("die");

        polyCol.enabled = true;
        polyCol.excludeLayers = afterDeathLayer;

        rb.gravityScale = 1f;
        rb.freezeRotation = false;
        rb.velocity = flyVelocity;
        rb.AddTorque(Random.Range(-20.0f, 20.0f));

        GameManager.SwitchLayerTo("Particle", gameObject);

        StartCoroutine(Co_Dead(deathDuration));
    }

    IEnumerator Co_Dead(float deathDuration)
    {
        float timer = 0f;
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;

        while (timer < deathDuration)
        {
            timer += Time.deltaTime;

            float t = timer / deathDuration;

            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            yield return null;
        }

        myLord.bodyCount--;
        Destroy(gameObject);
    }

    public bool CanAttack(Transform attackerPos)
    {
        return true;
    }

    public void OnAttack(Transform attackerTransform)
    {
        if (isDead) return;
        isDead = true;

        myLord.Dead(attackerTransform);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
