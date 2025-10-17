using UnityEngine;
using System.Collections;

public class StingSoldierMovement : MonoBehaviour
{
    [Header("움직임")]
    public float moveSpeed = 5; // 움직임 속도
    public float moveRange; // 대기 상태에 들어간 위치로부터 최대 이동할 수 있는 범위
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어
    public float entityRadius;  // 엔티티의 크기 (CircleCast 용도)

    [Header("공격")]
    public float attackRange;   // 공격의 범위
    public float detectionRange;    // 감지의 범위
    public float attackCooldown;    // 공격 대기시간 (공격 쿨타임)
    public float attackDuration;    // 공격 모션의 유지 시간
    public float ㅁ;    // 공격 모션 이후 대기 시간

    [Header("히트박스")]
    public Vector2 hitboxOffset = Vector2.zero;    // 히트박스 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask targetLayer;   // 감지 레이어


    float idleMovingStepDelay = 0.75f;

    bool isFacingRight = true;  // 오른쪽을 바라보고 있는지 여부

    Coroutine IdleMovementCoroutine;

    Vector2 idleStartPos;   // 대기가 시작된 위치
    public enum state { idle, track, attack }
    state currentState;


    void Start()
    {
        SetState(state.idle);
    }

    void Update()
    {
        
    }

    void SetState(state targetState)
    {
        StopAllCoroutines();
        IdleMovementCoroutine = null;

        if (targetState == state.idle)
        {
            currentState = state.idle;
            idleStartPos = transform.position;
            StartIdleMovement();
        }
        else if (targetState == state.track)
        {
            currentState = state.track;
        }
        else
        {
            currentState = state.attack;
        }
    }

    void StartIdleMovement()
    {
        if (IdleMovementCoroutine != null)
        {
            StopCoroutine(IdleMovementCoroutine);
        }

        IdleMovementCoroutine = StartCoroutine(IdleMovement());
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
                yield break;
            }

            LookPos(targetPos);

            while (Vector2.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );

                yield return null;
            }

            yield return new WaitForSeconds(idleMovingStepDelay);
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
        Vector2 worldCenter = (Vector2)transform.position + hitboxOffset;

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
                        playerScript.OnAttack(0.35f, 1, transform);
                    }
                    else
                    {
                        Debug.LogWarning($"플레이어 태그를 가졌지만 스크립트가 없는 대상: {targetCollider.gameObject.name}");
                    }

                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 gizmoCenter = (Vector2)transform.position + hitboxOffset;
        Gizmos.DrawWireCube(gizmoCenter, new Vector3(hitboxSize.x, hitboxSize.y, 0f));

        Gizmos.DrawWireSphere(transform.position, detectionRange);
        

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, moveRange);
    }

    
    float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }
}
