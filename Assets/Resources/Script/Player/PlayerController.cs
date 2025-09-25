using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float acceleration = 5;  // 플레이어 가속도
    public float decelerationMultiplier = 5;    // 가속도에 배수로 작용하는 감속도
    public float maxSpeed = 15; // 최고 속도
    public float jumpForce = 10;    // 점프 힘

    [Header("지상 감지 위치")]
    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public LayerMask groundLayer;

    private float moveInput = 0;    // 현재 방향 입력 (A : -1 , D : 1)
    private float lastMoveInput = 1;    // 마지막으로 누른 방향키
    private float currentMoveSpeed = 1; // 현재 움직임 속도 (가속도 반영)
    private float layerCheckRadius = 0.05f;  // 감지 위치 반경

    private bool isFacingRight = true;  // 오른쪽을 바라보고 있는가? (방향 전환에 필요함)
    private bool isRunning = false; // 움직이는 중인가?
    private bool hasInput = false; // 입력이 있는가? (A 또는 D를 눌렀을 때 true. 단, 동시에 누르는 것을 제외함.)
    private bool isGrounded = false;    // 땅에 닿았는가? (점프, 월런 해제에 이용)

    private Rigidbody2D rb;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }



    void Update()
    {
        UpdateStates(); // 상태 업데이트
        MoveInputHandler(); // 조작 키 감지

        CheckFlip();    // 캐릭터 좌우 회전
        HandleMovement();   // 감지된 키를 기반으로 움직임
        JumpHandler();  // 점프
        UpdateAnimation(); // 애니메이션 업데이트
    }

    void CheckFlip()
    {
        if ((moveInput > 0 && !isFacingRight) || (moveInput < 0 && isFacingRight)) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    void MoveInputHandler()
    {
        if (IsSingleInput())
        {
            if (Input.GetKey(KeyCode.A)) moveInput = -1;
            else moveInput = 1;

            lastMoveInput = moveInput;
            hasInput = true;
        }
        else
        {
            moveInput = 0;
            hasInput = false;
        }
    }

    public bool IsSingleInput()
    {
        bool notBothKeys = !(Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D));
        bool atLeastOneKey = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D));

        return notBothKeys && atLeastOneKey;
    }

    void HandleMovement()
    {
        if (hasInput)
        {
            currentMoveSpeed = Mathf.Min(currentMoveSpeed + acceleration * Time.deltaTime, maxSpeed);
        }
        else
        {
            currentMoveSpeed = Mathf.Max(currentMoveSpeed - (acceleration * Time.deltaTime) * decelerationMultiplier, 0);
        }

        rb.velocity = new Vector2(lastMoveInput * currentMoveSpeed, rb.velocity.y);
    }

    void JumpHandler()
    {
        if (!isGrounded) return;

        if (Input.GetKey(KeyCode.Space))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    void UpdateStates()
    {
        // 움직이는지 확인
        if (currentMoveSpeed <= 0)
        {
            isRunning = false;
        }
        else
        {
            isRunning = true;
        }
        

        // 지상인지 확인
        bool leftFoot = Physics2D.OverlapCircle(groundCheckLeft.position, layerCheckRadius, groundLayer);
        bool centerFoot = Physics2D.OverlapCircle(groundCheckCenter.position, layerCheckRadius, groundLayer);
        bool rightFoot = Physics2D.OverlapCircle(groundCheckRight.position, layerCheckRadius, groundLayer);
        isGrounded = leftFoot || centerFoot || rightFoot;
    }

    void UpdateAnimation()
    {
        anim.SetBool("isRunning", isRunning);
        anim.SetFloat("moveSpeed", currentMoveSpeed / maxSpeed);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckLeft.position, layerCheckRadius);
        Gizmos.DrawWireSphere(groundCheckCenter.position, layerCheckRadius);
        Gizmos.DrawWireSphere(groundCheckRight.position, layerCheckRadius);
    }
}
