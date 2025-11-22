using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CapsuleCollider2D))]
public class ShamblerMovement : MonoBehaviour, I_Attackable
{
    [Header("걷기")]
    public float maxSpeed = 8; // 최대 움직임 속도
    public float moveRadius; // 대기 상태에 들어간 위치로부터 최대 탐색 범위. 이 범위는 지형에 따라 조절될 수 있음.
    public float trunDuration = 0.5f;   // 회전 대기 시간
    public float acceleration = 0.1f; // 가속도
    [Header("달리기")]
    public float runningSpeedMultiplier = 3; // 달리기 속도 배수
    public float runningAcceleration = 0.5f; // 달리기 가속도

    [Header("지형 감지")]
    public Transform wallCheckPos;  // 벽
    public Transform upperGroundCheckPos;    // 땅 위쪽
    public Transform middleGroundCheckPos;    // 땅 위쪽
    public Transform lowerGroundCheckPos;    // 땅 아래쪽

    [Header("공격")]
    public float readyToAttackTime = 0.5f;  // 공격의 준비 시간 (이후 터짐)
    public float damage = 1f;
    public float knockbackPower = 1f;
    public float knockbacktime = 0.1f;

    [Header("팀 킬")]
    public float readyToAttackTimeAtTeamKill = 0.1f;

    [Header("공격 범위")]
    public float explosionRadius = 2.0f;    // 공격의 범위
    public float attackRadius = 2.0f;   // 공격 시작의 범위

    [Header("시야 범위")]
    public Vector2 viewOffset = new Vector2(0f, 0.5f); // 시야 중심의 오프셋
    public Vector2 viewSize = new Vector2(5f, 3f);     // 시야 영역의 가로/세로 크기

    [Header("레이어, 캐스트")]
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어
    public LayerMask playerLayer;   // 플레이어 감지 레이어
    public LayerMask attackLayer;   // 공격 레이어

    [Header("플레이어 감지")]
    public Transform exclamationMarkPos;    // 느낌표 위치
    public GameObject exclamationMarkObj;    // 느낌표 프리팹
    public float detectionTime = 0.1f;  // 발각도 충전 시간
    public float detectionCancelTime = 0.5f;  // 의심 해제 시간
    public float detectionDecayTime = 1.5f; // 발각도 감소 시간

    private int facingSign = 1; // 바라보는 방향

    private float currentNormalizedSpeed = 0;   // 정규화된 속도
    private float layerCheckRadius = 0.05f;  // 감지 위치 반경
    private float detectionRate = 0;    // 발각의 정도

    private bool isFacingRight = true;  // 오른쪽을 바라보는지 여부
    private bool canGoStraight = true;  // 직진 가능 여부 (벽이 없고 땅이 있어야 함)
    private bool isMoving = false;  // 움직이고 있는지 여부
    private bool isExploding = false;    // 폭발 중인지 여부
    private bool wasHitPlayer = false;  // 플레이어가 한번 맞았는지 여부

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    public enum state { idle, doubt, track, endAttack, exploding }
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
        if (isExploding) return;

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
                case state.track:
                    TrackHandler();

                    break;
            }
        }
        else if (currentState != state.idle)
        {
            SetState(state.idle);
        }
    }

    void SetState(state targetState)
    {
        if (isExploding) return;

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
        LookPos(playerObject.transform.position);

        Vector2 checkPos = playerObject.transform.position;
        checkPos.y = transform.position.y;

        if (canGoStraight && !HasArrived(checkPos))
        {
            currentNormalizedSpeed = Mathf.Clamp(currentNormalizedSpeed + runningAcceleration * Time.deltaTime, 0.505f, 1f);
            rb.velocity = new Vector2(facingSign * (currentNormalizedSpeed/2) * maxSpeed * runningSpeedMultiplier, rb.velocity.y);
        }
        else
        {
            Vector2 originVelocity = rb.velocity;
            originVelocity.x = 0;
            rb.velocity = originVelocity;
            currentNormalizedSpeed = 0;
        }

        if (IsPlayerInRange())
        {
            StartExplosion();
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

    IEnumerator EndAttack()
    {
        anim.SetTrigger("endAttack");
        yield return new WaitForSeconds(readyToAttackTime);
        SetState(state.idle);
    }

    void StartExplosion()
    {
        if (isExploding) return;
        isExploding = true;
        StopAllCoroutines();
        GameManager.SwitchLayerTo("Particle", gameObject);
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        if (exclamationMark != null) Destroy(exclamationMark.gameObject);

        Vector2 originVelocity = rb.velocity;
        originVelocity.x = 0;
        rb.velocity = originVelocity;
        currentNormalizedSpeed = 0;

        anim.SetTrigger("explosionPreparation");
        yield return new WaitForSeconds(readyToAttackTime);
        anim.SetTrigger("explosion");
        ExplosionDamage();
    }

    void ExplosionDamage()
    {
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(
            transform.position,                 // 중심 위치
            explosionRadius,                    // 반경
            attackLayer                // 감지할 레이어 마스크
        );

        foreach (Collider2D other in hitTargets)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController pc = other.GetComponent<PlayerController>();

                if (pc != null && !wasHitPlayer)
                {
                    pc.OnAttack(damage, knockbackPower, knockbacktime, transform);
                    wasHitPlayer = true;
                }
                continue;
            }

            I_Attackable attackableTarget = other.GetComponent<I_Attackable>();

            if (attackableTarget != null && !other.CompareTag("Player"))
            {
                attackableTarget.OnAttack(transform);
            }
        }
    }

    public void EndExplosion()
    {
        Destroy(gameObject);
    }

    bool IsPlayerInRange()
    {
        float distanceToPlayer = Vector2.Distance(playerObject.transform.position, transform.position);
        return distanceToPlayer <= attackRadius;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        facingSign = isFacingRight ? 1 : -1;

        UpdateStates();
    }

    void LookPos(Vector2 targetPos)
    {
        float directionX = targetPos.x - transform.position.x;

        if (directionX != 0 && (directionX > 0) != isFacingRight)
        {
            Flip();
        }
    }

    bool HasArrived(Vector3 pos)
    {
        float distance = Vector3.Distance(transform.position, pos);
        return distance <= 0.1f;
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

    void SwitchPos()
    {
        Flip();

        targetPos = isFacingRight ? movePosRight : movePosLeft;
    }

    public bool CanAttack(Transform attackerPos)
    {
        return true;
    }

    public void OnAttack(Transform attacker)
    {
        if (ShieldMovement.shieldInstance != null)
        {
            if (ShieldMovement.shieldInstance.transform.position != attacker.position)
            {
                readyToAttackTime = readyToAttackTimeAtTeamKill;
            }
        }
        else
        {
            readyToAttackTime = readyToAttackTimeAtTeamKill;
        }
        
        StartExplosion();
    }

    void UpdateStates()
    {
        movePosRight.y = movePosLeft.y = targetPos.y = transform.position.y;

        bool upperGroundDetect = Physics2D.OverlapCircle(upperGroundCheckPos.position, layerCheckRadius, obstacleMask);
        bool middleGroundDetect = Physics2D.OverlapCircle(middleGroundCheckPos.position, layerCheckRadius, obstacleMask);
        bool lowerGroundDetect = Physics2D.OverlapCircle(lowerGroundCheckPos.position, layerCheckRadius, obstacleMask);

        bool isGrounded = upperGroundDetect || middleGroundDetect || lowerGroundDetect;
        bool isTouchingAnyWall = Physics2D.OverlapCircle(wallCheckPos.position, layerCheckRadius, obstacleMask);

        canGoStraight = isGrounded && !isTouchingAnyWall;

        isMoving = currentNormalizedSpeed > 0f;

        anim.SetBool("isMoving", isMoving);
        anim.SetFloat("moveSpeed", currentNormalizedSpeed);

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRadius);


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
        Gizmos.DrawWireSphere(middleGroundCheckPos.position, 0.05f);
        Gizmos.DrawWireSphere(lowerGroundCheckPos.position, 0.05f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckPos.position, 0.05f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(exclamationMarkPos.position, 0.05f);
    }
}
