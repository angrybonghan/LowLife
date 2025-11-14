using System.Collections;
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
    public float detectionTime = 1.0f;  // 발각도 충전 시간
    public float detectionCancelTime = 0.5f;  // 의심 해제 시간
    public float detectionDecayTime = 2.0f; // 발각도 감소 시간

    [Header("시야 범위")]
    public Vector2 viewOffset = new Vector2(0f, 0.5f); // 시야 중심의 오프셋
    public Vector2 viewSize = new Vector2(5f, 3f);     // 시야 영역의 가로/세로 크기

    [Header("공격 범위")]
    public float attackRange;

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

    public enum state { idle, doubt, attack }
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



    }

    void SetState(state targetState)
    {
        if (targetState == state.idle || playerObject == null)
        {
            movePosRight = movePosLeft = transform.position;
            movePosRight.x += moveRadius;
            movePosLeft.x -= moveRadius;

            targetPos = isFacingRight ? movePosRight : movePosLeft;

            StartCoroutine(IdleMovement());
        }
    }

    IEnumerator IdleMovement()
    {
        while (true)
        {
            float sign = isFacingRight ? 1f : -1f;

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
