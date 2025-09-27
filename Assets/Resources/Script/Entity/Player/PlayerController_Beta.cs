using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController_Beta : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 15f;
    public float coyoteTime = 0.1f;
    public float quickTrunTime = 0.15f;
    [Header("움직임 상태 이상")]
    public bool enableReversal = false;
    public bool disableMove = false;
    public bool disableWallKick = false;
    public bool disableSlide = false;
    public bool disableJump = false;
    public float moveSpeedMultiplier = 1f;
    [Header("지상 체크 위치")]
    //public Transform groundCheck;
    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    [Header("벽 체크 위치")]
    public Transform wallCheckEyes;
    public Transform wallCheckFeet;
    public LayerMask wallLayer;
    public float wallCheckRadius = 0.2f;
    [Header("외부조작기")]
    public bool disableControl = false;

    private float wallRunStiffnessTimeCounter = 0;
    private float AirborneTimeCounter = 0;
    private float moveInput;
    private float afterMoveInput = 0f;
    private float inputHoldTime = 0f;
    private float quickTrunTimeCounter = 0f;
    private float coyoteTimeCounter;
    private bool isAirborne = false;
    private bool isGrounded = true;
    private bool canWallRun = false;
    private bool isTouchingWall = false;
    private bool isCrashingWall = false;
    private bool isFacingRight = true;
    private Rigidbody2D rb;
    private Animator anim;

    // 애니메이션 관련
    private bool isRunning = false;
    private bool isQuickTurning = false;
    private bool isSliding = false;
    private bool isInAir = false;
    private bool isWallKicking = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        UpdateStates();
        //isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        /*
        //moveInput = Input.GetAxisRaw("Horizontal");
        제 1대 방향키 동작 코드 (사망)
        사망 사유 : 개발자는 화살표 키를 감지하는 것을 원치 않음
        
        if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            moveInput = 0;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            moveInput = -1;
        }
        else
        {
            moveInput = 1;
        }
        제 2대 방향키 동작 코드 (사망)
        사망 사유 : 마음에 안 들음.

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.A))
            {
                moveInput = -1;
            }
            else
            {
                moveInput = 1;
            }
        }
        else
        {
            moveInput = 0;
        }
        제 3대 방향키 동작 코드 (사망)
        사망 사유 : 제 4대 방향키 동작 코드가 더 뛰어난 계산 효율을 가짐
        */

        if (isQuickTurning)
        {
            quickTrunTimeCounter += Time.deltaTime;
            if ((quickTrunTimeCounter >= quickTrunTime && isGrounded) || isCrashingWall)
            {
                isQuickTurning = false;
                anim.SetBool("isQuickTurning", false);
                inputHoldTime = 0;
                CheckFlip();
            }
            if (!isGrounded)
            {
                isQuickTurning = false;
                anim.SetBool("isQuickTurning", false);
                inputHoldTime = 0;
            }

            if (isQuickTurning) return;
        }

        if (!disableControl)
        {
            if (!disableMove)
            {
                if (Input.GetKey(KeyCode.A)) moveInput = -moveSpeedMultiplier;
                else if (Input.GetKey(KeyCode.D)) moveInput = moveSpeedMultiplier;
                else moveInput = 0;

                if (enableReversal) moveInput = -moveInput;
            }
            else moveInput = 0;
        }

        // 제 4대 방향키 동작 코드

        isRunning = Mathf.Abs(moveInput) > 0.1f;
        
        if (isRunning != anim.GetBool("isRunning")) anim.SetBool("isRunning", isRunning);

        if (isRunning)
        {
            // 퀵턴
            if (CheckFlipOutput() && afterMoveInput < 0.1f && !isQuickTurning && isGrounded && inputHoldTime > quickTrunTime * 2)
            {
                isQuickTurning = true;
                anim.SetBool("isQuickTurning", true);
                quickTrunTimeCounter = 0;
                return;
            }

            afterMoveInput = 0;
            inputHoldTime += Time.deltaTime;

            if (!isCrashingWall) anim.SetBool("isRunning", true);
            else anim.SetBool("isRunning", false);
        }
        else
        {
            afterMoveInput += Time.deltaTime;
            if (afterMoveInput > quickTrunTime)
            {
                inputHoldTime = 0;
            }

            anim.SetBool("isRunning", false);
        }

        if (wallRunStiffnessTimeCounter > 0)
        {
            canWallRun = false;
            wallRunStiffnessTimeCounter -= Time.deltaTime;
            if (wallRunStiffnessTimeCounter <= 0)
            {
                canWallRun = true;
            }
        }

        if (AirborneTimeCounter > 0)
        {
            isAirborne = true;
            AirborneTimeCounter -= Time.deltaTime;
            if (isCrashingWall)
            {
                isAirborne = false;
                AirborneTimeCounter = 0;
                CheckFlip();
            }
            if (AirborneTimeCounter <= 0 && isRunning)
            {
                isAirborne = false;
            }
        }
        else if ((isAirborne && (isRunning) || isCrashingWall || isGrounded))
        {
            isAirborne = false;
            CheckFlip();
        }

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            isInAir = false;
            anim.SetBool("isInAir", false);
        }
        else
        {
            if (coyoteTimeCounter > 0)
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            isInAir = true;
            anim.SetBool("isInAir", true);

            if (rb.velocity.y > 0) anim.SetBool("isGoingUp", true);
            else anim.SetBool("isGoingUp", false);
        }

    }

    private void LateUpdate()
    {
        if (isQuickTurning) return;

        // 방향 전환
        if (!isAirborne)
        {
            CheckFlip();
        }

        // 점프
        if (!disableJump && !disableControl)
        {
            if (Input.GetKey(KeyCode.Space) && coyoteTimeCounter > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                coyoteTimeCounter = 0f;
                SetWallRunStiffness(0.2f);

                if (isSliding)
                {
                    SetAirborne(0.05f);
                    isSliding = false;
                    anim.SetBool("isSliding", false);
                }

                isInAir = true;
                anim.SetBool("isInAir", true);
            }
        }

        // 슬라이딩
        if (!disableSlide)
        {
            if (isSliding)
            {
                if (isCrashingWall || coyoteTimeCounter <= 0f)
                {
                    // 슬라이딩 해제 조건 :
                    // 벽에 충돌 || 공중에 뜸 || Flip() 호출됨 (방향을 반대로 꺽음)
                    isSliding = false;
                    anim.SetBool("isSliding", false);
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftShift) && isRunning && !isCrashingWall && isGrounded)
                {
                    isSliding = true;
                    anim.SetBool("isSliding", true);
                }
            }
        }
        else if (isSliding)
        {
            isSliding = false;
            anim.SetBool("isSliding", false);
        }
        

        // 월 킥 (공중에서 벽에 붙기)
        if (!disableWallKick)
        {
            if (isTouchingWall && isInAir && !isWallKicking && !isAirborne && canWallRun)
            {
                isWallKicking = true;
                anim.SetBool("isWallKicking", true);
            }

            if (isWallKicking)
            {
                rb.velocity = new Vector2(0f, -1f);
                inputHoldTime = 0f;

                if (isGrounded || !isTouchingWall || (isFacingRight && moveInput < 0) || (!isFacingRight && moveInput > 0) || Input.GetKeyDown(KeyCode.S))
                {
                    // 월 킥 해제 조건 :
                    // 땅에 닿음 || 벽에서 떨어짐 || 오른쪽 벽에 붙어 A 누르기 || 왼쪽 벽에 붙어 D 누르기 || S 누르기
                    isWallKicking = false;
                    anim.SetBool("isWallKicking", false);
                    Flip();
                }
                else if (Input.GetKeyDown(KeyCode.Space))
                {
                    // 월 킥 발동 조건 : 벽에서 미끄러지는 도중 스페이스 바 누르기
                    isWallKicking = false;
                    anim.SetBool("isWallKicking", false);
                    SetAirborne(0.1f);

                    if (isFacingRight)
                    {
                        rb.velocity = new Vector2(jumpForce * -moveSpeedMultiplier, jumpForce);
                    }
                    else
                    {
                        rb.velocity = new Vector2(jumpForce * moveSpeedMultiplier, jumpForce);
                    }
                    Flip();
                }
            }
        }
        else if (isWallKicking)
        {
            isWallKicking = false;
            anim.SetBool("isWallKicking", false);
        }
    }

    void FixedUpdate()
    {

        if (isSliding)
        {
            if (isFacingRight)
            {
                rb.velocity = new Vector2((moveSpeedMultiplier * 1.2f) * moveSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2((moveSpeedMultiplier * -1.2f) * moveSpeed, 0);
            }
        }
        else if (isQuickTurning)
        {
            if (isFacingRight)
            {
                rb.velocity = new Vector2((moveSpeedMultiplier / 2) * moveSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2((moveSpeedMultiplier / -2) * moveSpeed, 0);
            }
        }
        else if(!isAirborne && !isWallKicking)
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        isSliding = false;
        anim.SetBool("isSliding", false);

        if (isWallKicking)
        {
            isWallKicking = false;
            anim.SetBool("isWallKicking", false);
            wallRunStiffnessTimeCounter = 0.1f;
        }

        UpdateStates();
    }

    void UpdateStates()
    {
        bool eyesTouchingWall = Physics2D.OverlapCircle(wallCheckEyes.position, wallCheckRadius, wallLayer);
        bool feetTouchingWall = Physics2D.OverlapCircle(wallCheckFeet.position, wallCheckRadius, wallLayer);
        isTouchingWall = eyesTouchingWall || feetTouchingWall;

        bool eyesCrashingWall = Physics2D.OverlapCircle(wallCheckEyes.position, wallCheckRadius, groundLayer);
        bool feetCrashingWall = Physics2D.OverlapCircle(wallCheckFeet.position, wallCheckRadius, groundLayer);
        isCrashingWall = eyesCrashingWall || feetCrashingWall;

        bool leftFoot = Physics2D.OverlapCircle(groundCheckLeft.position, groundCheckRadius, groundLayer);
        bool centerFoot = Physics2D.OverlapCircle(groundCheckCenter.position, groundCheckRadius, groundLayer);
        bool rightFoot = Physics2D.OverlapCircle(groundCheckRight.position, groundCheckRadius, groundLayer);
        isGrounded = leftFoot || centerFoot || rightFoot;
    }

    void SetAirborne(float time)
    {
        AirborneTimeCounter = AirborneTimeCounter < time ? time : AirborneTimeCounter;
        isAirborne = true;
    }

    void SetWallRunStiffness(float time)
    {
        if (time > 0)
        {
            canWallRun = false;

            if (time > wallRunStiffnessTimeCounter)
            {
                wallRunStiffnessTimeCounter = time;
            }
        }
    }

    void CheckFlip()
    {
        if ((moveInput > 0 && !isFacingRight) || (moveInput < 0 && isFacingRight)) Flip();
    }

    bool CheckFlipOutput()
    {
        return (moveInput > 0 && !isFacingRight) || (moveInput < 0 && isFacingRight);
    }

    public void DisableControl(bool LookRight, float playerHorizontalPosition)
    {
        if (disableControl)
        {
            return;
        }

        disableControl = true;
        moveInput = 0;

        if (isSliding)
        {
            isSliding = false;
            anim.SetBool("isSliding", false);
        }
        if (isWallKicking)
        {
            isWallKicking = false;
            anim.SetBool("isWallKicking", false);
            wallRunStiffnessTimeCounter = 0.1f;
        }

        if (LookRight)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            isFacingRight = true;
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            isFacingRight = false;
        }

        transform.position = new Vector3(playerHorizontalPosition, transform.position.y, transform.position.z);
    }

    public void EnableContorl()
    {
        disableControl = false;
    }

    public void RunTo(bool isRight)
    {
        disableControl = true;
        moveInput = 0;

        if (isSliding)
        {
            isSliding = false;
            anim.SetBool("isSliding", false);
        }
        if (isWallKicking)
        {
            isWallKicking = false;
            anim.SetBool("isWallKicking", false);
            wallRunStiffnessTimeCounter = 0.1f;
        }

        if (isRight)
        {
            moveInput = 1;
        }
        else
        {
            moveInput = -1;
        }
    }

    public void StopRunning()
    {
        moveInput = 0;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckLeft != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckLeft.position, groundCheckRadius);
        }
        if (groundCheckCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckCenter.position, groundCheckRadius);
        }
        if (groundCheckRight != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckRight.position, groundCheckRadius);
        }

        //if (groundCheck != null)
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        //}

        if (wallCheckEyes != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheckEyes.position, wallCheckRadius);
        }

        if (wallCheckFeet != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheckFeet.position, wallCheckRadius);
        }
    }
}