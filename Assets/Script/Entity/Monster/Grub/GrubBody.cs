using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PolygonCollider2D))]
public class GrubBody : MonoBehaviour
{
    [Header("컨트롤러")]
    public GrubMovement movementScript;

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

    PolygonCollider2D polyCol;
    Rigidbody2D rb;

    GrubBodyController myLord; // 예쓰 마이 로드!!!, 예쓰!!!!!!!!!!!!!!!!

    private void Awake()
    {
        polyCol = GetComponent<PolygonCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        polyCol.enabled = false;
        rb.gravityScale = 0;
    }

    private void Update()
    {
        if (canAttack) AttackHander();
    }

    void AttackHander()
    {
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange,
            targetLayer
            );

        if (hitTargets.Length > 0)
        {
            foreach (Collider2D targetCollider in hitTargets)
            {
                if (targetCollider.TryGetComponent<PlayerController>(out PlayerController pc))
                {
                    if (pc.IsParried(transform))
                    {
                        movementScript.Parried();
                        myLord.EnableAttack(false);
                        return;
                    }
                    else
                    {
                        pc.OnAttack(damage, knockbackPower, knockbacktime, transform);
                    }
                }
                else
                {
                    Debug.LogWarning($"플레이어 태그를 가졌지만 스크립트가 없는 대상: {targetCollider.gameObject.name}");
                }
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
        polyCol.enabled = true;
        polyCol.excludeLayers = afterDeathLayer;

        rb.gravityScale = 1f;
        rb.velocity = flyVelocity;
        rb.freezeRotation = false;

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
