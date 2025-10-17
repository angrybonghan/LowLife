using UnityEngine;
using System.Collections;

public class StingSoldierMovement : MonoBehaviour
{
    [Header("������")]
    public float moveSpeed = 5; // ������ �ӵ�
    public float moveRange; // ��� ���¿� �� ��ġ�κ��� �ִ� �̵��� �� �ִ� ����
    public LayerMask obstacleMask;  // ��ֹ��� ������ ���̾�
    public float entityRadius;  // ��ƼƼ�� ũ�� (CircleCast �뵵)

    [Header("����")]
    public float attackRange;   // ������ ����
    public float detectionRange;    // ������ ����
    public float attackCooldown;    // ���� ���ð� (���� ��Ÿ��)
    public float attackDuration;    // ���� ����� ���� �ð�
    public float ��;    // ���� ��� ���� ��� �ð�

    [Header("��Ʈ�ڽ�")]
    public Vector2 hitboxOffset = Vector2.zero;    // ��Ʈ�ڽ� ������
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // ũ�� (width, height)
    public LayerMask targetLayer;   // ���� ���̾�


    float idleMovingStepDelay = 0.75f;

    bool isFacingRight = true;  // �������� �ٶ󺸰� �ִ��� ����

    Coroutine IdleMovementCoroutine;

    Vector2 idleStartPos;   // ��Ⱑ ���۵� ��ġ
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
}
