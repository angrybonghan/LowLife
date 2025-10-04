using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(Animator))]
public class ShildMovement : MonoBehaviour
{
    [Header("참조 및 설정")]
    public float throwSpeed = 25f;  // 방패가 날아가는 속도
    public float returnSpeed = 20f; // 방패가 플레이어에게 돌아오는 속도
    public float shieldRecallDistance = 2f; // 목표 위치로부터 떨어짐 상태로 전환될 방패 위치 거리.
    [Header("외부 조작 설정")]
    public Transform playerPostion;    // 방패를 던진 플레이어 위치
    public Vector3 throwDirection; // 던지는 방향
    public bool isShieldDropped = false;
    public bool isReturning = false;

    // 상태 확인용 열거형
    private enum ShieldState { THROWN, RETURN, DROPPED }
    private ShieldState currentState = ShieldState.THROWN;

    // 참조용 변수
    private Rigidbody2D rb;
    private CircleCollider2D boxCol;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCol = rb.GetComponent<CircleCollider2D>();
        anim = rb.GetComponent<Animator>();

        boxCol.isTrigger = true;
        SetState(ShieldState.THROWN);
    }

    void Update()
    {
        if (currentState == ShieldState.THROWN)
        {
            rb.velocity = throwDirection * throwSpeed;
        }
        else if (currentState == ShieldState.RETURN)
        {
            Vector3 directionToPlayer = (playerPostion.position - transform.position).normalized;
            rb.velocity = directionToPlayer * returnSpeed;

            if (Vector2.Distance(playerPostion.position, transform.position) <= shieldRecallDistance)
            {
                SetState(ShieldState.DROPPED);
            }
        }
    }

    /// <summary>
    /// 인수 : 방향 - 플레이어 트랜스폼
    /// </summary>
    public void InitializeThrow(Vector3 direction, Transform playerTr)
    {
        throwDirection = direction.normalized;
        playerPostion = playerTr;
    }

    private void SetState(ShieldState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case ShieldState.THROWN:
                rb.gravityScale = 0f;
                break;

            case ShieldState.RETURN:
                rb.gravityScale = 0f;

                if (playerPostion == null)
                {
                    SetState(ShieldState.DROPPED);
                }
                break;

            case ShieldState.DROPPED:
                rb.gravityScale = 1f;
                rb.velocity = new Vector2(rb.velocity.x * 0.2f, rb.velocity.y);
                boxCol.isTrigger = false;
                anim.SetBool("bool_isShieldDropped", true);
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState == ShieldState.THROWN)
        {
            SetState(ShieldState.RETURN);
        }
        else if (currentState == ShieldState.RETURN)
        {
            SetState(ShieldState.DROPPED);
        }
    }
}
