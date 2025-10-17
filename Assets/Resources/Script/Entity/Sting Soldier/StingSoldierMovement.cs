using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class StingSoldierMovement : MonoBehaviour, I_Attackable
{
    [Header("������")]
    public float maxSpeed = 5; // �ִ� ������ �ӵ�
    public float accelerationRate = 50f; // ���ӵ�
    public float STOPPING_DISTANCE = 0.1f;

    public float moveRange; // ��� ���¿� �� ��ġ�κ��� �ִ� �̵��� �� �ִ� ����
    public LayerMask obstacleMask;  // ��ֹ��� ������ ���̾�
    public float entityRadius;  // ��ƼƼ�� ũ�� (CircleCast �뵵)

    [Header("����")]
    public float attackRange;   // ������ ����
    public float detectionRange;    // ������ ����
    public float attackChargeTime;  // ������ �غ� �ð�
    public float attackDuration;    // ������ ���� �ð�
    public float attackCooldown;    // ���� ���ð� (���� ��Ÿ��)
    public float attackLength;  // ������ ����

    [Header("��Ʈ�ڽ�")]
    public Vector2 hitboxOffset = Vector2.zero;    // ��Ʈ�ڽ� ������
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // ũ�� (width, height)
    public LayerMask targetLayer;   // ���� ���̾�

    [Header("����")]
    public float deathDuration; // �״� �ð�


    float idleMovingStepDelay = 0.75f;  // Idle ���¿��� �̵��� �ֱ�

    bool isFacingRight = true;  // �������� �ٶ󺸰� �ִ��� ����
    bool isDead = false;    // �׾����� ����
    bool wasHitPlayer;  // �÷��̾ �ѹ� �ĺô��� ���� (���� 1�� �� ����)

    Coroutine IdleMovementCoroutine;
    Coroutine AttackMovementCoroutine;
    
    GameObject playerObject;    // �÷��̾� ������Ʈ


    Vector2 currentVelocity = Vector2.zero; // ���� ����
    Vector2 idleStartPos;   // ��Ⱑ ���۵� ��ġ
    public enum state { idle, track, attack }
    state currentState;

    private Rigidbody2D rb;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }


    void Start()
    {
        playerObject = GameObject.FindWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("�÷��̾� ����");
        }

        SetState(state.idle);
    }

    void Update()
    {
        if (isDead) return;

        if (currentState == state.idle)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerObject.transform.position);

            if (distanceToPlayer <= detectionRange)
            {
                SetState(state.track);
            }
        }
        else if (currentState == state.track)
        {
            Vector2 trackTargetPos = GetOptimalAttackPosition();
            float distanceToTargetPos = Vector3.Distance(transform.position, trackTargetPos);
            float distanceToPlayer = Vector3.Distance(transform.position, playerObject.transform.position);

            TrackPos(trackTargetPos);
            LookPos(playerObject.transform.position);

            if (distanceToTargetPos <= attackRange)
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

            if (distanceToTargetPos > attackRange)
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

        if (targetState == state.idle)
        {
            currentState = state.idle;
            idleStartPos = transform.position;

            if (IdleMovementCoroutine != null)
            {
                StopCoroutine(IdleMovementCoroutine);
            }

            IdleMovementCoroutine = StartCoroutine(IdleMovement());
        }
        else if (targetState == state.track)
        {
            currentState = state.track;
        }
        else
        {
            currentState = state.attack;
            rb.velocity = Vector2.zero;

            if (AttackMovementCoroutine != null)
            {
                StopCoroutine(IdleMovementCoroutine);
            }

            AttackMovementCoroutine = StartCoroutine(AttackMovement());
        }
    }

    public void TrackPos(Vector2 targetPos)
    {
        LookPos(targetPos);

        Vector2 targetDirection = (targetPos - (Vector2)transform.position).normalized;
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
                        currentPos,     // ���� ����
                        entityRadius,   // ���� ������
                        direction,      // ����
                        distance,       // �ִ� �Ÿ�
                        obstacleMask    // ������ ���̾�
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
                    maxSpeed * Time.deltaTime
                );

                yield return null;
            }

            yield return new WaitForSeconds(idleMovingStepDelay);
        }
    }

    IEnumerator AttackMovement()
    {
        while (true)
        {
            anim.SetTrigger("attack");
            wasHitPlayer = false;
            yield return new WaitForSeconds(attackChargeTime);

            float attackEndTime = Time.time + attackDuration;
            while (Time.time < attackEndTime)
            {
                if (!wasHitPlayer)
                {
                    Attack();
                    LookPos(playerObject.transform.position);
                }
                
                yield return null;
            }

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

        if (!isFacingRight)
        {
            offsetX *= -1;
        }

        Vector2 localAdjustedOffset = new Vector2(offsetX, hitboxOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // �߽� ��ġ
            hitboxSize,             // ũ��
            0f,                     // ȸ�� ����
            targetLayer             // ������ ���̾�
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
                        playerScript.OnAttack(0.35f, 1, transform);
                    }
                    else
                    {
                        Debug.LogWarning($"�÷��̾� �±׸� �������� ��ũ��Ʈ�� ���� ���: {targetCollider.gameObject.name}");
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

    public void OnAttack(Transform attackerTransform)
    {
        isDead = true;
        anim.SetTrigger("die");
        StopAllCoroutines();
        StartCoroutine(Dead());
    }

    IEnumerator Dead()
    {
        yield return new WaitForSeconds(deathDuration);
        Destroy(gameObject);
    }
}
