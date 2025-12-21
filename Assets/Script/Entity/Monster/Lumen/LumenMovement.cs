using System.Collections;
//using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CapsuleCollider2D))]
public class LumenMovement : MonoBehaviour, I_Attackable
{
    [Header("적 이름")]
    public string enemyType;

    [Header("움직임")]
    public float trunDuration = 0.5f;   // 회전 대기 시간

    [Header("팔, 머리")]
    public Transform headObj;
    public Transform armObj;

    [Header("공격")]
    public Transform firePoint;
    public GameObject projectile;
    public float readyToAttackTime = 0.5f;  // 공격의 준비 시간 (총 들기, 내리기)
    public float aimingTime = 2;    // 조준 시간
    public float timeToFire = 0.3f;          // 조준 이후 발사까지 시간
    public float reloadTime = 0.5f;  // 공격의 준비 시간 (재장전)

    [Header("카트리지 배출")]
    public Transform cartridgeSummonPos;
    public GameObject cartridge;

    [Header("대미지")]
    public float damage = 0.5f;
    public float knockbackPower = 1f;
    public float knockbacktime = 0.1f;

    [Header("시야 범위")]
    public Vector2 viewOffset = new Vector2(0f, 0.5f); // 시야 중심의 오프셋
    public Vector2 viewSize = new Vector2(5f, 3f);     // 시야 영역의 가로/세로 크기

    [Header("공격 범위")]
    public float attackRange;

    [Header("레이어, 캐스트")]
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어
    public LayerMask playerLayer;   // 감지 레이어

    [Header("플레이어 감지")]
    public Transform exclamationMarkPos;    // 느낌표 위치
    public GameObject exclamationMarkObj;    // 느낌표 프리팹
    public float detectionTime = 1.0f;  // 발각도 충전 시간
    public float detectionCancelTime = 0.5f;  // 의심 해제 시간
    public float detectionDecayTime = 2.0f; // 발각도 감소 시간

    [Header("죽음")]
    public float deathDuration = 2; // 죽는 시간
    public float fallingOutPower = 15; // 죽었을 때 날아갈 힘

    [Header("사망 후 제외 레이어")]
    public LayerMask afterDeathLayer;

    private int facingSign = 1; // 바라보는 방향
    private float detectionRate = 0;    // 발각의 정도

    private bool isFacingRight = true;  // 오른쪽을 바라보는지 여부
    private bool isDead = false;    // 죽었는지 여부
    private bool isAiming = false;  // 조준하고 있는지 여부

    public enum state { idle, doubt, track, attack, endAttack }
    state currentState;

    private Rigidbody2D rb;
    private Animator anim;
    private ExclamationMarkHandler exclamationMark;

    GameObject playerObject;    // 플레이어 오브젝트

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        playerObject = PlayerController.instance.gameObject;

        isFacingRight = true;
        SetState(state.idle);
    }

    void Update()
    {
        if (isDead) return;

        if (playerObject != null)
        {
            switch (currentState)
            {
                case state.idle:
                    if (IsPlayerInView())
                    {
                        SetState(state.doubt);
                    }

                    break;

                case state.track:
                    TrackHandler();

                    break;
            }

            if (isAiming) RotatePartsToLookAtPlayer();
        }
        else if (currentState != state.idle)
        {
            if (currentState == state.attack)
            {
                anim.SetTrigger("endAttack");
                Destroy(firePoint.gameObject);
            }

            SetState(state.idle);

            if (exclamationMark != null)
            {
                Destroy(exclamationMark.gameObject);
            }
        }
    }

    void SetState(state targetState)
    {
        StopAllCoroutines();
        currentState = targetState;

        if (targetState == state.idle || playerObject == null)
        {
            StartCoroutine(IdleMovement());
        }
        else if (targetState == state.doubt)
        {
            exclamationMark = Instantiate(exclamationMarkObj).GetComponent<ExclamationMarkHandler>();
            exclamationMark.SetTargetPos(exclamationMarkPos);

            StartCoroutine(DoubtHandler());
        }
        else if (targetState == state.attack)
        {
            StartCoroutine(AttackHandler());
        }
        else if (targetState == state.endAttack)
        {
            StartCoroutine(EndAttack());
        }
    }
    IEnumerator IdleMovement()
    {
        while (true)
        {
            yield return new WaitForSeconds(trunDuration);
            Flip();
            yield return null;
        }
    }

    IEnumerator DoubtHandler()
    {
        float TimeSincePlayerLost = 0;
        while (true)
        {
            if (playerObject == null) yield break;

            if (playerObject != null)
            {
                if (IsPlayerInView()) detectionRate += Time.deltaTime / detectionTime;
                else detectionRate -= Time.deltaTime / detectionDecayTime;
            }

            detectionRate = Mathf.Clamp01(detectionRate);
            exclamationMark.SetGaugeValue(detectionRate);

            if (detectionRate == 1)
            {
                anim.SetTrigger("readyToAttack");
                yield return new WaitForSeconds(readyToAttackTime);

                SetState(state.track);
            }

            if (detectionRate == 0)
            {
                TimeSincePlayerLost += Time.deltaTime;
                if (TimeSincePlayerLost >= detectionCancelTime)
                {
                    Destroy(exclamationMark.gameObject);
                    SetState(state.idle);
                }
            }
            else
            {
                TimeSincePlayerLost = 0;
                LookPos(playerObject.transform.position);
            }


            yield return null;
        }
    }

    void TrackHandler()
    {
        if (playerObject != null) LookPos(playerObject.transform.position);

        if (IsPlayerInRange())
        {
            SetState(state.attack);
            detectionRate = 1;
            exclamationMark.SetGaugeValue(detectionRate);
            return;
        }

        if (IsPlayerInView())
        {
            detectionRate += Time.deltaTime / detectionTime;
        }
        else
        {
            detectionRate -= Time.deltaTime / detectionDecayTime;
        }

        detectionRate = Mathf.Clamp01(detectionRate);
        exclamationMark.SetGaugeValue(detectionRate);

        if (detectionRate == 0)
        {
            SetState(state.endAttack);
            Destroy(exclamationMark.gameObject);
        }
    }

    IEnumerator AttackHandler()
    {
        while (true)
        {
            anim.SetTrigger("aiming");
            SummonLaser();
            isAiming = true;
            yield return new WaitForSeconds(aimingTime);
            isAiming = false;
            yield return new WaitForSeconds(timeToFire);
            anim.SetTrigger("fire");
            yield return new WaitForSeconds(reloadTime);

            if (!IsPlayerInRange()) break;
        }

        SetState(state.track);
    }

    void SummonLaser()
    {
        EnemyLaser ep = Instantiate(projectile, firePoint.position, Quaternion.identity).GetComponent<EnemyLaser>();
        ep.SetOrigin(firePoint);
        ep.SetDamage(damage, knockbackPower, knockbacktime);
        ep.SetTarget(playerObject.transform);
        ep.aimingTime = aimingTime;
        ep.timeToFire = timeToFire;
    }

    public void EmissionCartridge()
    {
        LumenCartridge lc = Instantiate(cartridge, cartridgeSummonPos.position, Quaternion.identity).GetComponent<LumenCartridge>();
        lc.flyToRight = !isFacingRight;
    }

    IEnumerator EndAttack()
    {
        anim.SetTrigger("endAttack");
        yield return new WaitForSeconds(readyToAttackTime);
        SetState(state.idle);
    }


    void RotatePartsToLookAtPlayer()
    {
        LookPos(playerObject.transform.position);

        RotateObjTo(armObj.gameObject, playerObject.transform.position);

        float verticalProximity = GetVerticalProximity();
        float targetHeadAngle = verticalProximity * 45;
        Quaternion headRotation = Quaternion.Euler(0, 0, targetHeadAngle);
        headObj.localRotation = headRotation;
    }

    void RotateObjTo(GameObject obj, Vector2 pos)
    {
        Vector2 direction = pos - (Vector2)obj.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle + (isFacingRight ? 0 : 180));
        obj.transform.rotation = rotation;
    }

    float GetVerticalProximity()
    {
        Vector3 direction = playerObject.transform.position - transform.position;
        Vector2 normalizedDirection = direction.normalized;
        float verticalComponent = normalizedDirection.y;

        return verticalComponent;
    }

    bool IsPlayerInView()
    {
        if (!CanSeePlayer())
        {
            return false;
        }

        Vector2 localAdjustedOffset = new Vector2(viewOffset.x * facingSign, viewOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // 중심 위치
            viewSize,             // 크기
            0f,                     // 회전 각도
            playerLayer             // 감지할 레이어
        );

        if (hitTargets.Length > 0)
        {
            foreach (Collider2D targetCollider in hitTargets)
            {
                if (targetCollider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool IsPlayerInRange()
    {
        bool inRange = Vector3.Distance(transform.position, playerObject.transform.position) < attackRange;

        return CanSeePlayer() && inRange;
    }

    bool CanSeePlayer()
    {
        Vector2 startPos = transform.position;
        Vector2 endPos = playerObject.transform.position;
        Vector2 direction = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, distance, obstacleMask);
        if (hit.collider != null) return false;
        return true;
    }

    void LookPos(Vector2 targetPos)
    {
        float directionX = targetPos.x - transform.position.x;

        if (directionX != 0 && (directionX > 0) != isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        facingSign = isFacingRight ? 1 : -1;
    }

    public void OnAttack(Transform attackerTransform)
    {
        if (isDead) return;
        isDead = true;
        if (firePoint != null) Destroy(firePoint.gameObject);

        AudioManager.instance.PlayEntityHitSound(transform.position);

        Vector2 direction = (transform.position - attackerTransform.position).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * fallingOutPower, ForceMode2D.Impulse);
        rb.freezeRotation = false;
        rb.gravityScale = 1f;
        rb.AddTorque(GetRandom(-20, 20));
        if (exclamationMark != null) Destroy(exclamationMark.gameObject);

        CapsuleCollider2D capsuleCol = GetComponent<CapsuleCollider2D>();
        capsuleCol.excludeLayers = afterDeathLayer;

        GameManager.SwitchLayerTo("Particle", gameObject);

        anim.SetTrigger("die");
        StopAllCoroutines();
        StartCoroutine(Dead());
    }

    public bool CanAttack(Transform attackerPos)
    {
        return true;
    }

    float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

    IEnumerator Dead()
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

        AchievementManager.Instance?.OnEnemyKilled(enemyType);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;

        Vector2 viewLocalAdjustedOffset = new Vector2(viewOffset.x * facingSign, viewOffset.y);
        Vector2 viewGizmoCenter = (Vector2)transform.position + viewLocalAdjustedOffset;

        Gizmos.DrawWireCube(viewGizmoCenter, new Vector3(viewSize.x, viewSize.y, 0f));


        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(exclamationMarkPos.position, 0.1f);
    }
}
