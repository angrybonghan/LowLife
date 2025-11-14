using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(BoxCollider2D))]
public class Ms_PatriotMovement : MonoBehaviour
{
    [Header("움직임")]
    public float maxSpeed = 5.5f; // 최대 움직임 속도
    public float moveRadius; // 대기 상태에 들어간 위치로부터 최대 탐색 범위. 이 범위는 지형에 따라 조절될 수 있음.
    public float trunDuration = 0.5f;   // 회전 대기 시간
    public float acceleration = 2f; // 가속도

    [Header("지형 감지")]
    public Transform wallCheckPos;  // 벽
    public Transform groundCheckPos;    // 땅
    public float layerCheckRadius = 0.05f;  // 감지 위치 반경

    [Header("레이어, 캐스트")]
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어
    public LayerMask playerLayer;   // 감지 레이어

    [Header("플레이어 감지")]
    public Transform exclamationMarkPos;    // 느낌표 위치
    public GameObject exclamationMarkObj;    // 느낌표 프리팹
    public float detectionTime = 0.2f;  // 발각도 충전 시간
    public float detectionCancelTime = 0.5f;  // 의심 해제 시간
    public float detectionDecayTime = 2.0f; // 발각도 감소 시간

    [Header("시야 범위")]
    public Vector2 viewOffset = new Vector2(0f, 0.5f); // 시야 중심의 오프셋
    public Vector2 viewSize = new Vector2(5f, 3f);     // 시야 영역의 가로/세로 크기

    [Header("공격 범위")]
    public float attackRange;

    [Header("공격")]
    public Transform firePoint;
    public GameObject projectile;
    public float readyToAttackTime = 0.6f;  // 공격의 준비 시간 (총 들기, 내리기)
    public float fireTime = 0.6666f;          // 조준 시간 (이후 발사)
    public float reloadTime = 1f;  // 공격의 준비 시간 (재장전)

    private float currentNormalizedSpeed = 0;   // 정규화된 속도
    private float detectionRate = 0;    // 발각의 정도

    private bool isFacingRight = true;  // 오른쪽을 바라보는지 여부
    private bool canGoStraight = true;  // 직진 가능 여부 (벽이 없고 땅이 있어야 함)
    private bool isMoving = false;  // 움직이고 있는지 여부
    private bool isDead = false;    // 죽었는지 여부

    private int facingSign = 1; // 바라보는 방향

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    public enum state { idle, doubt, attack, endAttack }
    state currentState;

    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D boxCol;
    private ExclamationMarkHandler exclamationMark;

    GameObject playerObject;    // 플레이어 오브젝트

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCol = GetComponent<BoxCollider2D>();
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
            if (currentState == state.idle)
            {
                if (IsPlayerInView())
                {
                    exclamationMark = Instantiate(exclamationMarkObj).GetComponent<ExclamationMarkHandler>();
                    exclamationMark.SetTargetPos(exclamationMarkPos);

                    SetState(state.doubt);
                }
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

    void SetState(state targetState)
    {
        StopAllCoroutines();
        currentState = targetState;

        rb.velocity = Vector3.zero;
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
            float sign = isFacingRight ? 1f : -1f;
            // 삼항으!!!!!!악!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // 너무 아름다워. 너무 고귀해. 내 그대를 위하여 국화꽃을 따리다.

            while (!HasArrived(targetPos) && canGoStraight)
            {
                currentNormalizedSpeed = Mathf.Min(currentNormalizedSpeed + acceleration * Time.deltaTime, 1f);
                rb.velocity = new Vector2(sign * currentNormalizedSpeed * maxSpeed, rb.velocity.y);
                yield return null;
            }
            rb.velocity = Vector3.zero;
            currentNormalizedSpeed = 0;

            yield return new WaitForSeconds(trunDuration);
            SwitchPos();
            yield return null;
        }
    }

    bool HasArrived(Vector3 pos)
    {
        float distance = Vector3.Distance(transform.position, pos);
        return distance <= 0.1f;
    }

    void SwitchPos()
    {
        Flip();
        targetPos = isFacingRight ? movePosRight : movePosLeft;
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        facingSign = isFacingRight ? 1 : -1;

        UpdateStates();
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
                SetState(state.attack);
            }

            if (detectionRate == 0)
            {
                TimeSincePlayerLost += Time.deltaTime;
                if (TimeSincePlayerLost >= detectionCancelTime)
                {
                    if (exclamationMark != null) Destroy(exclamationMark);

                    Destroy(exclamationMark.gameObject);
                    SetState(state.endAttack);
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

    IEnumerator AttackHandler()
    {
        anim.SetTrigger("readyToAttack");

        yield return new WaitForSeconds(readyToAttackTime);

        while (IsPlayerInRange())
        {
            LookPos(playerObject.transform.position);
            anim.SetTrigger("fire");
            yield return new WaitForSeconds(fireTime);
            Attack();
            yield return new WaitForSeconds(reloadTime);

        }

        SetState(state.doubt);
    }

    IEnumerator EndAttack()
    {
        anim.SetTrigger("endAttack");
        yield return new WaitForSeconds(readyToAttackTime);
        SetState(state.idle);
    }

    void Attack()
    {
        //EnemyProjectile ep = Instantiate(projectile, firePoint.position, Quaternion.identity).GetComponent<EnemyProjectile>();
        //ep.SetTarget(playerObject.transform);
    }

    bool IsPlayerInRange()
    {
        bool inRange = Vector3.Distance(transform.position, playerObject.transform.position) < attackRange;

        return CanSeePlayer() && inRange;
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

    void UpdateStates()
    {
        movePosRight.y = movePosLeft.y = targetPos.y = transform.position.y;

        bool isGrounded = Physics2D.OverlapCircle(groundCheckPos.position, layerCheckRadius, obstacleMask);
        bool isTouchingAnyWall = Physics2D.OverlapCircle(wallCheckPos.position, layerCheckRadius, obstacleMask);

        canGoStraight = isGrounded && !isTouchingAnyWall;

        isMoving = currentNormalizedSpeed > 0f;

        anim.SetBool("isMoving", isMoving);
        anim.SetFloat("moveSpeed", currentNormalizedSpeed);
    }

    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

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
        Gizmos.DrawWireSphere(groundCheckPos.position, layerCheckRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckPos.position, layerCheckRadius);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(exclamationMarkPos.position, 0.1f);
    }
}
