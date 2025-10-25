using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(BoxCollider2D))]
public class SlagMovement : MonoBehaviour
{
    [Header("움직임")]
    public float maxSpeed = 8; // 최대 움직임 속도
    public float moveRadius; // 대기 상태에 들어간 위치로부터 최대 탐색 범위. 이 범위는 지형에 따라 조절될 수 있음.
    public float trunDuration = 0.5f;   // 회전 대기 시간
    public float acceleration = 2f;

    [Header("레이어, 캐스트")]
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어

    [Header("지형 감지")]
    public Transform wallCheckPos;  // 벽
    public Transform groundCheckPos;    // 땅

    [Header("공격")]
    public float attackChargeTime;  // 공격의 준비 시간
    public float attackDuration;    // 공격의 유지 시간
    public float attackCooldown;    // 공격 대기시간 (공격 쿨타임)

    [Header("히트박스")]
    public Vector2 hitboxOffset = Vector2.zero;    // 히트박스 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask targetLayer;   // 감지 레이어

    [Header("죽음")]
    public float deathDuration = 2; // 죽는 시간
    public float fallingOutPower = 15; // 죽었을 때 날아갈 힘

    private float currentNormalizedSpeed = 0;
    private float layerCheckRadius = 0.05f;  // 감지 위치 반경

    private bool isFacingRight = true;
    private bool canGoStraight = true;
    private bool isMoving = false;

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    public enum state { idle, track, attack }
    state currentState;

    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D boxCol;

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

        SetState(state.idle);

        isFacingRight = true;

        movePosRight = movePosLeft = transform.position;
        movePosRight.x += moveRadius;
        movePosLeft.x -= moveRadius;

        targetPos = movePosRight;
    }

    void Update()
    {
        UpdateStates();

        //switch (currentState)
        //{

        //}
    }

    void SetState(state targetState)
    {
        StopAllCoroutines();
        currentState = targetState;

        if (targetState == state.idle)
        {
            StartCoroutine(IdleMovement());
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

        if (isFacingRight)
        {
            targetPos = movePosRight;
        }
        else
        {
            targetPos = movePosLeft;
        }

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

    bool HasArrived()
    {
        float distance = Vector3.Distance(transform.position, targetPos);
        return distance <= 0.1f;
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

        float offsetX = hitboxOffset.x;
        if (!isFacingRight) offsetX *= -1;
        Vector2 localAdjustedOffset = new Vector2(offsetX, hitboxOffset.y);
        Vector2 gizmoCenter = (Vector2)transform.position + localAdjustedOffset;

        Gizmos.DrawWireCube(gizmoCenter, new Vector3(hitboxSize.x, hitboxSize.y, 0f));

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

    }

}
