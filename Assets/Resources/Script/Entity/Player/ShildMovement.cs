using TMPro.EditorUtilities;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(Animator))]
public class ShildMovement : MonoBehaviour
{
    [Header("참조 및 설정")]
    public float maxShieldFlightTime = 1;    // 방패가 날아갈 수 있는 최대 시간
    public float throwSpeed = 25f;  // 방패가 날아가는 속도
    public float returnSpeed = 20f; // 방패가 플레이어에게 돌아오는 속도
    public float catchDistance = 0.75f; // 플레이어에게 잡혀질 거리
    [Header("외부 조작 설정")]
    public Transform playerPostion;    // 방패를 던진 플레이어 위치
    public Vector3 throwDirection; // 던지는 방향
    public bool isShieldDropped = false;
    public bool isReturning = false;

    private float currentFlightTime = 0;    // 현재 날아가는 시간 (시간 계산용)

    // 상태 확인용 열거형
    private enum ShieldState { THROWN, RETURN, DROPPED }
    private ShieldState currentState = ShieldState.THROWN;

    // 참조용 변수
    private Rigidbody2D rb;
    private CircleCollider2D boxCol;
    private Animator anim;
    private PlayerController playerController;

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

            currentFlightTime += Time.deltaTime;
            if (currentFlightTime >= maxShieldFlightTime)
            {
                SetState(ShieldState.RETURN);
            }
        }
        else if (currentState == ShieldState.RETURN)
        {
            Vector3 directionToPlayer = (playerPostion.position - transform.position).normalized;
            rb.velocity = directionToPlayer * returnSpeed;

            if (Vector2.Distance(playerPostion.position, transform.position) <= catchDistance)
            {
                playerController.CatchShield();
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// 인수 : 방향 - 플레이어 스크립트
    /// </summary>
    public void InitializeThrow(Vector3 direction, PlayerController script)
    {
        throwDirection = direction.normalized;
        playerPostion = script.transform;
        playerController = script;
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
    }
}
