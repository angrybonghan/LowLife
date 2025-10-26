using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(BoxCollider2D))]
public class SlagMovement : MonoBehaviour
{
    [Header("움직임")]
    public float maxSpeed = 8; // 최대 움직임 속도
    public float moveRadius; // 대기 상태에 들어간 위치로부터 최대 탐색 범위. 이 범위는 지형에 따라 조절될 수 있음.
    public float trunDuration = 0.5f;   // 회전 대기 시간
    public float acceleration = 2f; // 가속도

    [Header("지형 감지")]
    public Transform wallCheckPos;  // 벽
    public Transform groundCheckPos;    // 땅

    [Header("공격")]
    public float attackChargeTime = 0.42f;  // 공격의 준비 시간
    public float attackDuration = 0.25f;    // 공격의 유지 시간
    public float attackCooldown = 0.1f;    // 공격 대기시간 (공격 쿨타임)

    [Header("히트박스")]
    public Vector2 hitboxOffset = Vector2.zero;    // 히트박스 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)

    [Header("시야 범위")]
    public Vector2 viewOffset = new Vector2(0f, 0.5f); // 시야 중심의 오프셋
    public Vector2 viewSize = new Vector2(5f, 3f);     // 시야 영역의 가로/세로 크기

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

    private int offsetSign = 1;

    private float currentNormalizedSpeed = 0;   // 정규화된 속도
    private float layerCheckRadius = 0.05f;  // 감지 위치 반경
    private float detectionRate = 0;    // 발각의 정도

    private float x;

    private bool isFacingRight = true;
    private bool canGoStraight = true;
    private bool isMoving = false;

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    public enum state { idle, doubt, track, attack }
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
        playerObject = GameObject.FindWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("플레이어 없음");
        }

        isFacingRight = true;
        SetState(state.idle);
    }

    void Update()
    {
        UpdateStates();
        switch (currentState)
        {
            case state.idle:
                if (IsPlayerInView())
                {
                    SetState(state.doubt);
                }

                break;

            case state.doubt:
                DoubtHandler();

                break;
        }
    }

    void SetState(state targetState)
    {
        StopAllCoroutines();
        currentState = targetState;

        rb.velocity = Vector3.zero;
        currentNormalizedSpeed = 0;

        if (targetState == state.idle)
        {
            movePosRight = movePosLeft = transform.position;
            movePosRight.x += moveRadius;
            movePosLeft.x -= moveRadius;

            targetPos = movePosRight;

            StartCoroutine(IdleMovement());
        }
        else if (targetState == state.doubt)
        {
            exclamationMark = Instantiate(exclamationMarkObj).GetComponent<ExclamationMarkHandler>();
            exclamationMark.SetTargetPos(exclamationMarkPos);

            StartCoroutine(DoubtHandler());
        }
        else if (targetState == state.track)
        {
            
        }
        else if (targetState == state.attack)
        {
            
        }
    }

    void SwitchPos()
    {
        isFacingRight = !isFacingRight;

        targetPos = isFacingRight ? movePosRight : movePosLeft;
        offsetSign = isFacingRight ? 1 : -1;

        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1f;
        transform.localScale = currentScale;
    }

    void SetIdlePos()
    {

    }

    IEnumerator IdleMovement()
    {
        while (true)
        {
            float sign = isFacingRight ? 1f : -1f;

            while (!HasArrived() && canGoStraight)
            {
                currentNormalizedSpeed = Mathf.Min(currentNormalizedSpeed + acceleration * Time.deltaTime, 0.5f);
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
            }

            
            yield return null;
        }
    }

    bool HasArrived()
    {
        float distance = Vector3.Distance(transform.position, targetPos);
        return distance <= 0.1f;
    }

    bool IsPlayerInRange()
    {
        Vector2 localAdjustedOffset = new Vector2(hitboxOffset.x * offsetSign, hitboxOffset.y);
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

    bool IsPlayerInView()
    {
        Vector2 localAdjustedOffset = new Vector2(viewOffset.x * offsetSign, viewOffset.y);
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

        Vector2 hitboxLocalAdjustedOffset = new Vector2(hitboxOffset.x * offsetSign, hitboxOffset.y);
        Vector2 hitboxGizmoCenter = (Vector2)transform.position + hitboxLocalAdjustedOffset;

        Gizmos.DrawWireCube(hitboxGizmoCenter, new Vector3(hitboxSize.x, hitboxSize.y, 0f));

        Gizmos.color = Color.blue;

        Vector2 viewLocalAdjustedOffset = new Vector2(viewOffset.x * offsetSign, viewOffset.y);
        Vector2 viewGizmoCenter = (Vector2)transform.position + viewLocalAdjustedOffset;

        Gizmos.DrawWireCube(viewGizmoCenter, new Vector3(viewSize.x, viewSize.y, 0f));

        Gizmos.color = Color.cyan;

        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(movePosRight, 0.25f);
            Gizmos.DrawWireSphere(movePosLeft, 0.25f);
            Gizmos.DrawLine(movePosRight, movePosLeft);
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
        Gizmos.DrawWireSphere(groundCheckPos.position, 0.05f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckPos.position, 0.05f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(exclamationMarkPos.position, 0.05f);


    }

}
