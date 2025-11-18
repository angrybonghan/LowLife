using System.Collections;
using UnityEngine;

public class AmbushMovement : MonoBehaviour, I_Attackable
{
    [Header("움직임")]
    public float maxSpeed = 5.5f; // 최대 움직임 속도
    public float moveRadius = 5; // 대기 상태에 들어간 위치로부터 최대 탐색 범위. 이 범위는 지형에 따라 조절될 수 있음.
    public float trunDuration = 0.5f;   // 회전 대기 시간
    public float acceleration = 2f; // 가속도

    [Header("도망")]
    public GameObject leafParticle;
    public float maxRunawaySpeed = 20f;
    public float runawayAcceleration = 2f;
    public float resetDistance = 20f;

    [Header("지형 감지")]
    public Transform wallCheckPos;  // 벽
    public Transform upperGroundCheckPos;    // 땅 위쪽
    public Transform lowerGroundCheckPos;    // 땅 아래쪽
    public float layerCheckRadius = 0.05f;  // 감지 위치 반경

    [Header("레이어, 캐스트")]
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어
    public LayerMask playerLayer;   // 플레이어 감지 레이어

    [Header("플레이어 감지")]
    public Transform exclamationMarkPos;    // 느낌표 위치
    public GameObject exclamationMarkObj;    // 느낌표 프리팹
    public float detectionTime = 0.2f;  // 발각도 충전 시간
    public float detectionCancelTime = 0.5f;  // 의심 해제 시간
    public float detectionDecayTime = 2.0f; // 발각도 감소 시간

    [Header("시야 범위")]
    public Vector2 viewOffset = new Vector2(0f, 0.5f); // 시야 중심의 오프셋
    public Vector2 viewSize = new Vector2(5f, 3f);     // 시야 영역의 가로/세로 크기

    [Header("공격")]
    public float readyToAttackTime = 0.5f;  // 공격으로 전환 시간
    public float selfKnockBackPower = 1f;
    public float selfKnockBackTime = 1f;

    [Header("대미지")]
    public float damage = 0.4f;  // 공격 대미지
    public float knockbackPower = 1f;
    public float knockbacktime = 0.05f;

    [Header("히트박스")]
    public Vector2 hitboxOffset = Vector2.zero;    // 히트박스 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)

    [Header("죽음")]
    public float deathDuration = 2; // 죽는 시간
    public float fallingOutPower = 15; // 죽었을 때 날아갈 힘

    [Header("사망 후 제외 레이어")]
    public LayerMask afterDeathLayer;

    private int facingSign = 1; // 바라보는 방향

    private float currentNormalizedSpeed = 0;   // 정규화된 속도
    private float detectionRate = 0;    // 발각의 정도
    private float selfKnockBackValue = 0;

    private bool isFacingRight = true;  // 오른쪽을 바라보는지 여부
    private bool canGoStraight = true;  // 직진 가능 여부 (벽이 없고 땅이 있어야 함)
    private bool isMoving = false;  // 움직이고 있는지 여부
    private bool isDead = false;    // 죽었는지 여부

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;
    Coroutine selfKnockBackCoroutine;

    public enum state { idle, doubt, attack, endAttack, peelOff, nothing }
    state currentState;

    private Rigidbody2D rb;
    private Animator anim;
    private ExclamationMarkHandler exclamationMark;

    GameObject playerObject;    // 플레이어 오브젝트

    private void Awake()
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

        UpdateStates();
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

                case state.attack:
                    AttackHandler();
                    break;

                case state.peelOff:
                    PelloffMovementHandler();
                    break;
            }
        }
        else if (currentState != state.idle)
        {
            SetState(state.idle);

            if (exclamationMark != null)
            {
                Destroy(exclamationMark.gameObject);
            }
        }
    }

    public void SetState(state targetState)
    {
        StopAllCoroutines();
        currentState = targetState;

        Vector2 originVelocity = rb.velocity;
        originVelocity.x = 0;
        rb.velocity = originVelocity;
        currentNormalizedSpeed = 0;

        if (targetState == state.idle || playerObject == null)
        {
            movePosRight = movePosLeft = transform.position;
            movePosRight.x += moveRadius;
            movePosLeft.x -= moveRadius;

            targetPos = isFacingRight ? movePosRight : movePosLeft;

            StartCoroutine(IdleMovement());
        }
        else if (targetState == state.doubt)
        {
            exclamationMark = Instantiate(exclamationMarkObj).GetComponent<ExclamationMarkHandler>();
            exclamationMark.SetTargetPos(exclamationMarkPos);

            StartCoroutine(DoubtHandler());
        }
        else if (targetState == state.endAttack)
        {
            StartCoroutine(EndAttack());
        }
    }

    void SwitchPos()
    {
        Flip();

        targetPos = isFacingRight ? movePosRight : movePosLeft;
    }

    IEnumerator IdleMovement()
    {
        while (true)
        {
            float sign = isFacingRight ? 1f : -1f;

            while (!HasArrived(targetPos) && canGoStraight)
            {
                currentNormalizedSpeed = Mathf.Min(currentNormalizedSpeed + acceleration * Time.deltaTime, 0.5f);
                rb.velocity = new Vector2(sign * currentNormalizedSpeed * maxSpeed, rb.velocity.y);
                yield return null;
            }
            Vector2 originVelocity = rb.velocity;
            originVelocity.x = 0;
            rb.velocity = originVelocity;
            currentNormalizedSpeed = 0;

            yield return new WaitForSeconds(trunDuration);
            SwitchPos();
            yield return null;
        }
    }

    IEnumerator DoubtHandler()
    {
        float TimeSincePlayerLost = 0;
        while (true)
        {
            if (IsPlayerInView()) detectionRate += Time.deltaTime / detectionTime;
            else detectionRate -= Time.deltaTime / detectionDecayTime;

            detectionRate = Mathf.Clamp01(detectionRate);
            exclamationMark.SetGaugeValue(detectionRate);

            if (detectionRate == 1)
            {
                anim.SetTrigger("readyToAttack");
                yield return new WaitForSeconds(readyToAttackTime);

                SetState(state.attack);
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

    bool HasArrived(Vector3 pos)
    {
        float distance = Vector3.Distance(transform.position, pos);
        return distance <= 0.1f;
    }

    void AttackHandler()
    {
        LookPos(playerObject.transform.position);

        if (currentNormalizedSpeed >= 0.505f)
        {
            if (IsPlayerInRange())
            {
                Attack();

                if (selfKnockBackCoroutine != null) StopCoroutine(selfKnockBackCoroutine);
                selfKnockBackCoroutine = StartCoroutine(SelfKnockBack(selfKnockBackTime, selfKnockBackPower));
            }
        }

        Vector2 checkPos = playerObject.transform.position;
        checkPos.y = transform.position.y;

        if (canGoStraight && !HasArrived(checkPos))
        {
            currentNormalizedSpeed = Mathf.Clamp(currentNormalizedSpeed + acceleration * Time.deltaTime, 0.505f, 1f);
            currentNormalizedSpeed -= selfKnockBackValue;
            rb.velocity = new Vector2(facingSign * currentNormalizedSpeed * maxSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector3.zero;
            currentNormalizedSpeed = 0;
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

    void PelloffMovementHandler()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerObject.transform.position);
        if (distanceToPlayer > resetDistance)
        {
            SetState(state.idle);
            anim.SetTrigger("reset");
            return;
        }

        LookPosOpposite(playerObject.transform.position);

        if (canGoStraight)
        {
            currentNormalizedSpeed = Mathf.Clamp(currentNormalizedSpeed + runawayAcceleration * Time.deltaTime, 0, 1f);
            rb.velocity = new Vector2(facingSign * currentNormalizedSpeed * maxRunawaySpeed, rb.velocity.y);
        }
        else
        {
            Vector2 originVelocity = rb.velocity;
            originVelocity.x = 0;
            rb.velocity = originVelocity;
            currentNormalizedSpeed = 0;
        }
    }

    void LookPos(Vector2 targetPos)
    {
        float directionX = targetPos.x - transform.position.x;

        if (directionX != 0 && (directionX > 0) != isFacingRight)
        {
            Flip();
        }
    }

    void LookPosOpposite(Vector2 targetPos)
    {
        float directionX = targetPos.x - transform.position.x;

        if (directionX != 0 && (directionX > 0) == isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        facingSign = isFacingRight ? 1 : -1;

        if (currentState == state.attack)
        {
            Vector2 originVelocity = rb.velocity;
            originVelocity.x = 0;
            rb.velocity = originVelocity;
            currentNormalizedSpeed = 0;
        }

        UpdateStates();
    }


    bool IsPlayerInRange()
    {
        Vector2 localAdjustedOffset = new Vector2(hitboxOffset.x * facingSign, hitboxOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // 중심 위치
            hitboxSize,             // 크기
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

    void Attack()
    {
        Vector2 localAdjustedOffset = new Vector2(hitboxOffset.x * facingSign, hitboxOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // 중심 위치
            hitboxSize,             // 크기
            0f,                     // 회전 각도
            playerLayer             // 감지할 레이어
        );

        if (hitTargets.Length > 0)
        {
            foreach (Collider2D targetCollider in hitTargets)
            {
                if (targetCollider.CompareTag("Player"))
                {
                    if (targetCollider.TryGetComponent<PlayerController>(out PlayerController playerScript))
                    {
                        playerScript.OnAttack(damage, knockbackPower, knockbacktime, transform);
                        break;
                    }
                    else
                    {
                        Debug.LogWarning($"플레이어 태그를 가졌지만 스크립트가 없는 대상: {targetCollider.gameObject.name}");
                    }
                }
            }
        }
    }

    IEnumerator SelfKnockBack(float duraton, float power)
    {
        if (duraton > 0)
        {
            float elapsedTime = 0f;
            selfKnockBackValue = power;

            while (elapsedTime < duraton)
            {
                elapsedTime += Time.deltaTime;
                selfKnockBackValue = Mathf.Lerp(power, 0, elapsedTime / duraton);
                yield return null;
            }
        }

        selfKnockBackValue = 0f;
        selfKnockBackCoroutine = null;
    }

    IEnumerator EndAttack()
    {
        anim.SetTrigger("endAttack");

        yield return new WaitForSeconds(readyToAttackTime);

        SetState(state.idle);
    }

    bool IsPlayerInView()
    {
        Vector2 localAdjustedOffset = new Vector2(viewOffset.x * facingSign, viewOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // 중심 위치
            viewSize,             // 크기
            0f,                     // 회전 각도
            playerLayer             // 감지할 레이어
        );

        bool isPlayerInView = false;

        if (hitTargets.Length > 0)
        {
            foreach (Collider2D targetCollider in hitTargets)
            {
                if (targetCollider.CompareTag("Player"))
                {
                    isPlayerInView = true;
                    break;
                }
            }
        }

        if (!isPlayerInView) return false;

        Vector2 startPos = transform.position;
        Vector2 endPos = playerObject.transform.position;
        Vector2 direction = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, distance, obstacleMask);
        if (hit.collider != null) return false;
        return true;
    }

    void UpdateStates()
    {
        movePosRight.y = movePosLeft.y = targetPos.y = transform.position.y;

        bool upperGroundDetect = Physics2D.OverlapCircle(upperGroundCheckPos.position, layerCheckRadius, obstacleMask);
        bool lowerGroundDetect = Physics2D.OverlapCircle(lowerGroundCheckPos.position, layerCheckRadius, obstacleMask);

        bool isGrounded = upperGroundDetect || lowerGroundDetect;
        bool isTouchingAnyWall = Physics2D.OverlapCircle(wallCheckPos.position, layerCheckRadius, obstacleMask);

        canGoStraight = isGrounded && !isTouchingAnyWall;

        isMoving = currentNormalizedSpeed > 0f;

        anim.SetBool("isMoving", isMoving);
        anim.SetFloat("moveSpeed", currentNormalizedSpeed);

    }

    public void OnAttack(Transform attackerTransform)
    {
        if (isDead) return;

        if (currentState == state.peelOff || currentState == state.nothing)
        {
            Dead(attackerTransform);
        }
        else
        {
            SetState(state.nothing);
            anim.SetTrigger("pelloff");
            if (exclamationMark != null) Destroy(exclamationMark.gameObject);

            Instantiate(leafParticle, transform.position, Quaternion.identity);
        }
    }

    public bool CanAttack()
    {
        return true;
    }

    float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

    void Dead(Transform attackerTransform)
    {
        isDead = true;

        Vector2 direction = (transform.position - attackerTransform.position).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * fallingOutPower, ForceMode2D.Impulse);
        rb.freezeRotation = false;
        rb.gravityScale = 1f;
        rb.AddTorque(GetRandom(-20, 20));
        if (exclamationMark != null) Destroy(exclamationMark.gameObject);

        CapsuleCollider2D capsuleCol = GetComponent<CapsuleCollider2D>();
        capsuleCol.excludeLayers = afterDeathLayer;

        anim.SetTrigger("die");
        StopAllCoroutines();
        StartCoroutine(Co_Dead());
    }

    IEnumerator Co_Dead()
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

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 hitboxLocalAdjustedOffset = new Vector2(hitboxOffset.x * facingSign, hitboxOffset.y);
        Vector2 hitboxGizmoCenter = (Vector2)transform.position + hitboxLocalAdjustedOffset;

        Gizmos.DrawWireCube(hitboxGizmoCenter, new Vector3(hitboxSize.x, hitboxSize.y, 0f));

        Gizmos.color = Color.blue;

        Vector2 viewLocalAdjustedOffset = new Vector2(viewOffset.x * facingSign, viewOffset.y);
        Vector2 viewGizmoCenter = (Vector2)transform.position + viewLocalAdjustedOffset;

        Gizmos.DrawWireCube(viewGizmoCenter, new Vector3(viewSize.x, viewSize.y, 0f));

        Gizmos.color = Color.cyan;

        if (Application.isPlaying)
        {
            if (currentState == state.idle)
            {
                Gizmos.DrawWireSphere(movePosRight, 0.25f);
                Gizmos.DrawWireSphere(movePosLeft, 0.25f);
                Gizmos.DrawLine(movePosRight, movePosLeft);
            }
        }
        else
        {
            Vector3 gizmosMovePosRight = transform.position;
            Vector3 gizmosMovePosLeft = transform.position;
            gizmosMovePosRight.x += moveRadius;
            gizmosMovePosLeft.x -= moveRadius;

            Gizmos.DrawWireSphere(gizmosMovePosRight, 0.25f);
            Gizmos.DrawWireSphere(gizmosMovePosLeft, 0.25f);
            Gizmos.DrawLine(gizmosMovePosRight, gizmosMovePosLeft);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(upperGroundCheckPos.position, 0.05f);
        Gizmos.DrawWireSphere(lowerGroundCheckPos.position, 0.05f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckPos.position, 0.05f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(exclamationMarkPos.position, 0.05f);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, resetDistance);
    }

}
