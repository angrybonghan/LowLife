using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(BoxCollider2D))]
public class StingSoldierMovement : MonoBehaviour, I_Attackable
{
    [Header("움직임")]
    public float maxSpeed = 5; // 최대 움직임 속도
    public float accelerationRate = 50f; // 가속도
    public float moveRange; // 대기 상태에 들어간 위치로부터 최대 이동할 수 있는 범위

    [Header("레이어, 캐스트")]
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어
    public float entityRadius;  // 엔티티의 크기 (CircleCast 용도)

    [Header("공격")]
    public float attackRange;   // 공격의 범위
    public float detectionRange;    // 감지의 범위
    public float attackChargeTime;  // 공격의 준비 시간
    public float attackDuration;    // 공격의 유지 시간
    public float attackCooldown;    // 공격 대기시간 (공격 쿨타임)
    public float attackLength;  // 공격의 길이
    
    [Header("대미지")]
    public float damage = 0.3f;  // 공격 대미지


    [Header("히트박스")]
    public Vector2 hitboxOffset = Vector2.zero;    // 히트박스 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask targetLayer;   // 감지 레이어

    [Header("죽음")]
    public float deathDuration = 2; // 죽는 시간
    public float fallingOutPower = 15; // 죽었을 때 날아갈 힘

    [Header("사망 후 제외 레이어")]
    public LayerMask afterDeathLayer;


    float idleMovingStepDelay = 0.75f;  // Idle 상태에서 이동의 주기

    bool isFacingRight = true;  // 오른쪽을 바라보고 있는지 여부
    bool isDead = false;    // 죽었는지 여부
    bool wasHitPlayer;  // 플레이어를 한번 쳐봤는지 여부 (공격 1번 당 리셋)
    bool isAttacking = false;   // 공격 중인지 여부

    Coroutine IdleMovementCoroutine;
    Coroutine AttackMovementCoroutine;
    
    GameObject playerObject;    // 플레이어 오브젝트

    Vector2 idleStartPos;   // 대기가 시작된 위치
    public enum state { idle, track, attack }
    state currentState;

    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D boxCol;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCol = GetComponent<BoxCollider2D>();
    }


    void Start()
    {
        playerObject = PlayerController.instance.gameObject;

        SetState(state.idle);
    }

    void Update()
    {
        if (isDead) return;

        if (currentState == state.idle)
        {
            if (playerObject == null) return;

            float distanceToPlayer = Vector3.Distance(idleStartPos, playerObject.transform.position);

            if (distanceToPlayer <= detectionRange)
            {
                SetState(state.track);
            }
        }
        else if (playerObject == null)
        {
            SetState(state.idle);
            return;
        }
        else if (currentState == state.track)
        {
            Vector2 trackTargetPos = GetOptimalAttackPosition();
            float distanceToTargetPos = Vector3.Distance(transform.position, trackTargetPos);
            float distanceToPlayer = Vector3.Distance(transform.position, playerObject.transform.position);

            TrackPos(trackTargetPos);
            LookPos(playerObject.transform.position);

            if (distanceToTargetPos <= attackRange || IsPlayerInRange())
            {
                SetState(state.attack);
            }
            else if (distanceToPlayer > detectionRange)
            {
                SetState(state.idle);
            }
        }
        else if (currentState == state.attack)
        {
            Vector2 trackTargetPos = GetOptimalAttackPosition();
            float distanceToTargetPos = Vector3.Distance(transform.position, trackTargetPos);

            if (distanceToTargetPos > attackRange && !isAttacking && !IsPlayerInRange())
            {
                SetState(state.track);
            }
        }
    }


    void SetState(state targetState)
    {
        StopAllCoroutines();
        IdleMovementCoroutine = null;
        AttackMovementCoroutine = null;

        currentState = targetState;
        rb.velocity = Vector2.zero;

        if (targetState == state.idle || playerObject == null)
        {
            idleStartPos = transform.position;

            if (IdleMovementCoroutine != null)
            {
                StopCoroutine(IdleMovementCoroutine);
            }

            IdleMovementCoroutine = StartCoroutine(IdleMovement());
        }
        else if (targetState == state.attack)
        {
            if (AttackMovementCoroutine != null)
            {
                StopCoroutine(AttackMovementCoroutine);
            }

            AttackMovementCoroutine = StartCoroutine(AttackMovement());
        }
    }

    public void TrackPos(Vector2 targetPos)
    {
        LookPos(targetPos);

        Vector2 targetDirection = accelerationRate * (targetPos - (Vector2)transform.position).normalized;
        rb.AddForce(targetDirection, ForceMode2D.Force);

        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    IEnumerator IdleMovement()
    {
        const int MAX_ATTEMPTS = 100;

        while (true)
        {
            Vector2 targetPos = Vector2.zero;
            bool foundTarget = false;
            int attemptCount = 0;

            while (attemptCount < MAX_ATTEMPTS)
            {
                Vector2 randomDirection = Random.insideUnitCircle * GetRandom(0, moveRange);
                Vector2 proposedTarget = idleStartPos + randomDirection;

                Vector2 currentPos = transform.position;

                Vector2 direction = proposedTarget - currentPos;
                float distance = direction.magnitude;

                if (distance > 0.01f)
                {
                    direction.Normalize();

                    RaycastHit2D hit = Physics2D.CircleCast(
                        currentPos,     // 시작 지점
                        entityRadius,   // 원의 반지름
                        direction,      // 방향
                        distance,       // 최대 거리
                        obstacleMask    // 감지할 레이어
                    );

                    if (hit.collider == null)
                    {
                        targetPos = proposedTarget;
                        foundTarget = true;
                        break;
                    }
                }

                attemptCount++;
            }

            if (!foundTarget)
            {
                IdleMovementCoroutine = null;
                yield break;
            }

            LookPos(targetPos);

            while (Vector2.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPos,
                    maxSpeed * Time.deltaTime
                );

                yield return null;
            }

            rb.velocity = Vector2.zero;
            yield return new WaitForSeconds(idleMovingStepDelay);
        }
    }

    IEnumerator AttackMovement()
    {
        while (true)
        {
            anim.SetTrigger("attack");
            wasHitPlayer = false;
            isAttacking = true;
            LookPos(playerObject.transform.position);
            yield return new WaitForSeconds(attackChargeTime);

            float attackEndTime = Time.time + attackDuration;
            while (Time.time < attackEndTime)
            {
                if (!wasHitPlayer)
                {
                    Attack();
                }
                rb.velocity = Vector2.zero;
                yield return null;
            }

            isAttacking = false;
            yield return new WaitForSeconds(attackCooldown);
        }
    }


    public Vector2 GetOptimalAttackPosition()
    {   
        Vector2 myPos = transform.position;
        Vector2 playerPos = playerObject.transform.position;

        
        Vector2 leftAttackPos = new Vector2(playerPos.x - attackLength, playerPos.y);
        Vector2 rightAttackPos = new Vector2(playerPos.x + attackLength, playerPos.y);

        float distanceToLeft = Vector2.Distance(myPos, leftAttackPos);
        float distanceToRight = Vector2.Distance(myPos, rightAttackPos);

        if (distanceToLeft < distanceToRight)
        {
            return leftAttackPos;
        }
        else
        {
            return rightAttackPos;
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    void LookPos(Vector2 targetPos)
    {
        float directionX = targetPos.x - transform.position.x;

        if (directionX != 0 && (directionX > 0) != isFacingRight)
        {
            Flip();
        }
    }

    void Attack()
    {
        float offsetX = hitboxOffset.x;
        if (!isFacingRight) offsetX *= -1;
        Vector2 localAdjustedOffset = new Vector2(offsetX, hitboxOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // 중심 위치
            hitboxSize,             // 크기
            0f,                     // 회전 각도
            targetLayer             // 감지할 레이어
        );

        if (hitTargets.Length > 0)
        {
            foreach (Collider2D targetCollider in hitTargets)
            {
                if (targetCollider.CompareTag("Player"))
                {
                    if (targetCollider.TryGetComponent<PlayerController>(out PlayerController playerScript))
                    {
                        wasHitPlayer = true;
                        playerScript.OnAttack(damage, 1, 0.1f, transform);
                        return;
                    }
                    else
                    {
                        Debug.LogWarning($"플레이어 태그를 가졌지만 스크립트가 없는 대상: {targetCollider.gameObject.name}");
                    }

                }
            }
        }
    }

    bool IsPlayerInRange()
    {
        float offsetX = hitboxOffset.x;
        if (!isFacingRight) offsetX *= -1;
        Vector2 localAdjustedOffset = new Vector2(offsetX, hitboxOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // 중심 위치
            hitboxSize,             // 크기
            0f,                     // 회전 각도
            targetLayer             // 감지할 레이어
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float offsetX = hitboxOffset.x;
        if (!isFacingRight) offsetX *= -1;
        Vector2 localAdjustedOffset = new Vector2(offsetX, hitboxOffset.y);
        Vector2 gizmoCenter = (Vector2)transform.position + localAdjustedOffset;

        Gizmos.DrawWireCube(gizmoCenter, new Vector3(hitboxSize.x, hitboxSize.y, 0f));

        Gizmos.color = Color.blue;
        if (Application.isPlaying)
        {
            if (currentState == state.idle)
            {
                Gizmos.DrawWireSphere(idleStartPos, detectionRange);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(idleStartPos, moveRange);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.position, detectionRange);
            }
            
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, moveRange);
        }


    }

    
    float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

    public void OnAttack(Transform attackerTransform)
    {
        if (isDead) return;
        isDead = true;

        SoundManager.instance.PlayEntityHitSound(transform.position);

        Vector2 direction = (transform.position - attackerTransform.position).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * fallingOutPower, ForceMode2D.Impulse);
        rb.freezeRotation = false;
        rb.gravityScale = 1f;
        rb.AddTorque(GetRandom(-20, 20));

        boxCol.excludeLayers = afterDeathLayer;

        GameManager.SwitchLayerTo("Particle", gameObject);

        anim.SetTrigger("die");
        StopAllCoroutines();
        StartCoroutine(Dead());
    }

    public bool CanAttack(Transform attackerPos)
    {
        return true;
    }

    IEnumerator Dead()
    {
        float timer = 0f;
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;
        boxCol.isTrigger = false;

        while (timer < deathDuration)
        {
            timer += Time.deltaTime;

            float t = timer / deathDuration;

            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            yield return null;
        }

        Destroy(gameObject);
    }
}