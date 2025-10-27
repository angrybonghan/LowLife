using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(BoxCollider2D))]
public class MsMovement : MonoBehaviour
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
    public float readyToAttackTime = 0.5f;  // 공격의 준비 시간 (총 들기, 내리기)
    public float aimingTime = 0.2f;          // 조준 시간 (이후 발사)
    public float reloadTime = 0.5f;  // 공격의 준비 시간 (재장전)

    [Header("대미지")]
    public float damage = 0.4f;  // 공격 대미지

    [Header("시야 범위")]
    public Vector2 viewOffset = new Vector2(0f, 0.5f); // 시야 중심의 오프셋
    public Vector2 viewSize = new Vector2(5f, 3f);     // 시야 영역의 가로/세로 크기

    [Header("공격 범위")]
    public float attackRange;

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

    private int facingSign = 1; // 바라보는 방향

    private float currentNormalizedSpeed = 0;   // 정규화된 속도
    private float layerCheckRadius = 0.05f;  // 감지 위치 반경
    private float detectionRate = 0;    // 발각의 정도

    private bool isFacingRight = true;  // 오른쪽을 바라보는지 여부
    private bool canGoStraight = true;  // 직진 가능 여부 (벽이 없고 땅이 있어야 함)
    private bool isMoving = false;  // 움직이고 있는지 여부
    private bool isDead = false;    // 죽었는지 여부

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    public enum state { idle, doubt, track, attack, endAttack }
    state currentState;

    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D boxCol;
    private ExclamationMarkHandler exclamationMark;

    GameObject playerObject;    // 플레이어 오브젝트

    void Awake()
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

            targetPos = isFacingRight ? movePosRight : movePosLeft;

        }
        else if (targetState == state.doubt)
        {
            exclamationMark = Instantiate(exclamationMarkObj).GetComponent<ExclamationMarkHandler>();
            exclamationMark.SetTargetPos(exclamationMarkPos);

        }
        else if (targetState == state.attack)
        {

        }
        else if (targetState == state.endAttack)
        {

        }
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
        Gizmos.DrawWireSphere(groundCheckPos.position, 0.05f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckPos.position, 0.05f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(exclamationMarkPos.position, 0.05f);
    }
}
