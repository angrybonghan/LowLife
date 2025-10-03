using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("기본 이동 설정")]
    public float acceleration = 12;  // 플레이어 가속도
    public float deceleration = 50;    // 플레이어 감속도
    public float maxSpeed = 12; // 최고 속도
    public float jumpForce = 10;    // 점프 힘
    public float quickTrunDuration = 0.3f;    // 퀵턴 최대 길이
    public float shieldSpeed​Multiplier = 1;   // 방패로 인한 감속도 배율
    [Header("대쉬 설정")]
    public float dashDuration = 0.25f;  // 대쉬 유지 시간
    public float dashSpeedMultiplier = 1.4f;   // 현재 속도에서 배수로 가속할 대쉬 속도
    public float dashCooldown = 0.5f;  // 대쉬 재사용 대기시간
    public GameObject dashEffect;   // 대쉬 잔상 이펙트 프리팹
    [Header("방패 설정")]
    public float shieldEquipDuration = 0.35f;
    public float parryDuration = 0.4f;


    [Header("지상 감지 위치")]    // Transform 3개 중 하나라도 groundLayer에 닿았으면 땅인 것으로 봄
    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public LayerMask groundLayer;

    [Header("벽 감지 위치")]
    public Transform wallCheckTop;
    public Transform wallCheckBottom;
    public LayerMask wallLayer;

    [Header("이펙트 위치 / 프리팹")]
    public GameObject wallKickEffect;   // 이펙트 생성 위치
    public Transform wallKickEffectPos; // 이펙트 프리팹

    [Header("공격 - 히트박스")]
    public LayerMask attackableLayer;  // 히트박스가 감지할 레이어
    public Vector2 hitBoxCenter;    // 히트박스의 중심. 위치는 현재 위치 기준 오프셋으로 작동
    public float hitBoxSize;    // 정사각형 히트박스의 한 변의 길이

    [Header("공격 - 설정")]
    public float attackCooldown = 0.35f;
    public float comboMaxDuration = 0.3f;
    public int maxAttackMotions = 2;

    private int currentAttackMotionNumber = 1;  // 공격 애니메이션 번호
    

    private float moveInput = 0;    // 현재 방향 입력 (A : -1 , D : 1)
    private float lastMoveInput = 1;    // 마지막으로 누른 방향키
    private float currentMoveSpeed = 0; // 현재 움직임 속도 (가속도 반영)
    private float normalizedSpeed = 0;  // 정규화된 속도
    private float layerCheckRadius = 0.05f;  // 감지 위치 반경
    private float quickTrunTime = 0; // 현재 퀵턴 길이
    private float quickTurnDirection = 1;   // 퀵 턴의 방향
    private float coyoteTimeDuration = 0.1f; // 코요테 타임 길이
    private float coyoteTime = 0; // 현재 코요테타임
    private float timeSinceLastJump = 0; // 마지막 점프로부터 지난 시간
    private float controlDisableDuration = 0;   // 조작 비활성화 유지 시간 (0보다 작거나 같으면 비활성화)
    private float currentWallSlidingSpeed = 0;  // 현재 월 슬라이딩 속도
    private float WallSlidingSpeed = -0.1f;  // 목표 월 슬라이딩 속도
    private float WallKickDisableDuration = 0;
    private float dashSpeed;    // 대쉬 속도 (대쉬한 시점의 속도에 비례하여 빨라짐)
    private float dashDirection = 0;    // 대쉬 방향 (대쉬할 때마다 업데이트)
    private float currentDashDuration = 0;  // 현재 대쉬 시간
    private float currentDashCooldown = 0;  // 대쉬 이후 흐른 시간 (쿨다운 계산용)
    private float dashVerticalVelocity = 0; // 대쉬 수직 속도
    private float currentShieldEquipTime = 0; // 현재 방패 든 시간 (쿨다운 계산용)
    private float currentParryDuration = 0; // 현재 패링 시간 (지속시간 계산용)
    private float timeSinceLastAttack = 0; // 마지막 공격으로부터 흐른 시간 (콤보 시간 계산용)
    private float attackStartDirection = 0; // 공격을 시작한 시점의 방향


    private bool isControlDisabled = false; // 조작을 비활성화할지 여부 (월킥에 사용)
    private bool isFacingRight = true;  // 오른쪽을 바라보고 있는가? (방향 전환에 필요함)
    private bool isRunning = false; // 움직이는 중인가?
    private bool hasInput = false; // 입력이 있는가? (A 또는 D를 눌렀을 때 true. 단, 동시에 누르는 것을 제외함.)
    private bool isGrounded = false;    // 땅에 닿았는가? (점프, 월런 해제에 이용)
    private bool isFalling = false; // 떨어지고 있는가? (올라가는 중이라면 false)
    private bool isQuickTurning = false;    // 퀵 턴 도중인가?
    private bool isTouchingClimbableWall = false;   // 붙을 수 있는 벽에 닿아 있는가?
    private bool isTouchingAnyWall = false; // 벽에 닿아 있는가? (아무 벽이나 붙어있으면 true. 붙을 수 있는 벽 포함)
    private bool isWallSliding = false; // 월 슬라이딩 도중인가?
    private bool isDashing = false; // 대쉬 중인가?
    private bool canDash = false; // 대쉬 가능한가? (땅에 닿거나 월 슬라이딩 시 재충전)
    private bool isShielding = false; // 방패를 들고있는 중인가?
    private bool isEquippingShield = false; // 방패를 꺼내는 중인가?
    private bool isParrying = false; // 패링 중인가?
    private bool isAttacking = false; // 공격 중인가?
    private bool allowDashCancel = false;   // 공격 도중 대쉬로 공격을 취소할 수 있는지 여부
    private bool IsThrowMode = false;   // 투척 모드인지 여부


    // 속성, 스크립트 참조
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
        PlayerControlDisableHandler();  // 조작 중단 시간 계산
        CoyoteTimeHandler(); // 코요테 타임 시간 계산
        MoveInputHandler(); // 조작 키 감지

        CheckFlip();    // 캐릭터 좌우 회전, 퀵턴 작동
        WallSlidingHandler(); // 월 슬라이딩, 월 킥 애니메이션 트리거
        DashHandler(); // 대쉬 트리거, 대쉬 애니메이션 트리거
        AttackHandler(); // 공격 작동, 공격 애니메이션 트리거
        JumpHandler();  // 점프 작동, 점프 애니메이션 트리거
        ParryHandler(); // 패링 작동, 애니메이션 트리거
        ShildHandler(); // 방패 전개, 방패 해제, 방패 애니메이션 트리거
        HandleMovement();   // 감지된 키를 기반으로 움직임 (달리기, 월 슬라이딩, 대쉬, 퀵턴, 방패 들고 이동, 공격 중 이동)
        UpdateAnimation(); // 애니메이션 업데이트 (달리기, 퀵턴, 공중 상태, 움직임 속도, 추락 감지)
    }

    void CheckFlip()
    {
        if (isControlDisabled || isDashing || isParrying) return;

        if ((moveInput > 0 && !isFacingRight) || (moveInput < 0 && isFacingRight)) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        if (normalizedSpeed >= 0.7f && isGrounded && !isQuickTurning && !isAttacking)
        {
            // 최고 속도 기준 70% 이상의 속도에서 퀵턴이 작동할 수 있음
            // 비활성화 조건 : 공격 중일 때
            isQuickTurning = true;
            quickTrunTime = 0;
            quickTurnDirection = -moveInput;
        }

        hitBoxCenter.x = -hitBoxCenter.x;
    }

    void MoveInputHandler()
    {
        if (IsSingleInput())
        {
            if (Input.GetKey(KeyCode.A)) moveInput = -1;
            else moveInput = 1;

            hasInput = true;
            if (!isControlDisabled && !isDashing) lastMoveInput = moveInput;
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
        if (isAttacking)    // 공격 감속 (+ 에어브레이크)
        {
            AttackingMovement();
            return;
        }

        if (isShielding)    // 방패 도중 움직임
        {
            ShildMovement();
            return;
        }
        
        // 기본 이동 도중에는 대쉬가 가장 높은 우선순위를 가짐
        if (isDashing)
        {
            DashMovement();
            return;
        }

        // 조작 비활성화 상태에서 감속이나 가속하지 않음
        if (isControlDisabled) return;

        // 월 슬라이딩
        if (isWallSliding)
        {
            WallSlidingMovement();
            return;
        }

        // 퀵 턴
        if (isQuickTurning)
        {
            QuickTurnMovement();
            return;
        }

        // 모든 특수 이동이 아닐 때, 기본 이동 로직 실행
        DefaultMovement();
    }

    void QuickTurnMovement()
    {
        quickTrunTime += Time.deltaTime;
        rb.velocity = new Vector2(quickTurnDirection * currentMoveSpeed * (1 - (quickTrunTime / quickTrunDuration)), rb.velocity.y);

        // 퀵 턴 해제 조건
        // 퀵턴 시간 초과, 퀵턴 도중 방향을 다시 전환, 추락 (땅에서 떨어짐)
        if (quickTrunTime >= quickTrunDuration)
        {
            isQuickTurning = false;
            if (!hasInput)   // 입력하지 않으면 정지
            {
                currentMoveSpeed = 0;
            }
        }
        // 방향을 다시 전환하거나 땅에서 떨어지면, 현재 속도를 절반 감소
        if (quickTurnDirection == moveInput || !isGrounded)
        {
            isQuickTurning = false;
            currentMoveSpeed *= 0.5f;
        }
    }

    void DashMovement()
    {
        rb.velocity = new Vector2(dashDirection * dashSpeed, dashVerticalVelocity);
        
        GameObject newDashEffect = Instantiate(dashEffect, transform.position, quaternion.identity);
        newDashEffect.transform.localScale = new Vector3(
            newDashEffect.transform.localScale.x * dashDirection,
            newDashEffect.transform.localScale.y,
            newDashEffect.transform.localScale.z);

        GradientAfterimagePlayer gradientDashEffect = newDashEffect.GetComponent<GradientAfterimagePlayer>();
        if (gradientDashEffect != null)
        {
            gradientDashEffect.SetColorLevel(currentDashDuration / dashDuration);
        }
    }

    void WallSlidingMovement()
    {
        if (currentWallSlidingSpeed > WallSlidingSpeed) // 월 슬라이딩 관성 계산
        {
            currentWallSlidingSpeed = rb.velocity.y;
        }
        else
        {
            if (currentWallSlidingSpeed < WallSlidingSpeed)
            {
                currentWallSlidingSpeed += (Mathf.Abs(currentWallSlidingSpeed) * Time.deltaTime * deceleration) / 2;
                currentWallSlidingSpeed = Mathf.Min(currentWallSlidingSpeed, WallSlidingSpeed);
            }

            rb.velocity = new Vector2(0, currentWallSlidingSpeed);
        }
    }

    void DefaultMovement()
    {
        if (hasInput)   // 입력에 의한 가속
        {
            currentMoveSpeed = Mathf.Min(currentMoveSpeed + acceleration * Time.deltaTime, maxSpeed);
        }
        else  // 입력 없으면 감속
        {
            currentMoveSpeed = Mathf.Max(currentMoveSpeed - deceleration * Time.deltaTime, 0);
        }

        if (isTouchingAnyWall)  // 벽과 충돌하면 속도 없어짐
        {
            currentMoveSpeed = 0;
        }
        rb.velocity = new Vector2(lastMoveInput * currentMoveSpeed, rb.velocity.y);

        normalizedSpeed = currentMoveSpeed / maxSpeed;
    }

    void AttackingMovement()
    {
        currentMoveSpeed = Mathf.Max(currentMoveSpeed - (deceleration * Time.deltaTime * 0.75f), 0);

        rb.velocity = new Vector2(currentMoveSpeed * attackStartDirection, rb.velocity.y);

        normalizedSpeed = currentMoveSpeed / maxSpeed;
    }

    void ShildMovement()
    {
        if (hasInput && !isEquippingShield)
        {
            currentMoveSpeed = maxSpeed * shieldSpeedMultiplier;
        }
        else
        {
            currentMoveSpeed = 0;
        }

        if (isTouchingAnyWall || isParrying)  // 벽과 충돌하거나 패링하면 멈춤
        {
            currentMoveSpeed = 0;
        }

        rb.velocity = new Vector2(lastMoveInput * currentMoveSpeed, rb.velocity.y);

        normalizedSpeed = currentMoveSpeed / maxSpeed;
    }

    void JumpHandler()
    {
        timeSinceLastJump += Time.deltaTime;

        if (!isGrounded || isDashing) return;

        if (Input.GetKey(KeyCode.Space))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            anim.SetTrigger("trigger_jump");
            coyoteTime = coyoteTimeDuration;
            timeSinceLastJump = 0;
        }
    }

    void WallSlidingHandler()
    {
        if (WallKickDisableDuration > 0)
        {
            WallKickDisableDuration -= Time.deltaTime;
        }

        if (isWallSliding)
        {
            if (isGrounded) // 땅에 닿았을 때 월 슬라이딩 해제
            {
                isWallSliding = false;
                Flip();

                return;
            }

            if (!isTouchingClimbableWall)   // 벽이 사라졌을 때 월 슬라이딩 해제
            {
                isWallSliding = false;

                return;
            }

            if (Input.GetKey(KeyCode.Space) && !isControlDisabled && WallKickDisableDuration <= 0)    // 월 킥 작동
            {
                isWallSliding = false;
                currentMoveSpeed = maxSpeed;
                normalizedSpeed = currentMoveSpeed / maxSpeed;
                lastMoveInput = -lastMoveInput;
                rb.velocity = new Vector2(jumpForce * lastMoveInput, jumpForce);
                SetPlayerControlDisableDuration(0.15f);
                anim.SetTrigger("trigger_wallKick");

                GameObject newWallKickEffect = Instantiate(wallKickEffect, wallKickEffectPos.position, Quaternion.identity);
                newWallKickEffect.transform.localScale = new Vector3(
                    newWallKickEffect.transform.localScale.x * lastMoveInput,
                    newWallKickEffect.transform.localScale.y,
                    newWallKickEffect.transform.localScale.z);

                Flip();
            }
        }
        else
        {
            if (!isGrounded && isTouchingClimbableWall && timeSinceLastJump > 0.3f) // 월 슬라이딩 트리거
            {
                WallKickDisableDuration = 0.15f;
                isWallSliding = true;
                currentMoveSpeed = 0;
                normalizedSpeed = currentMoveSpeed / maxSpeed;
                currentWallSlidingSpeed = rb.velocity.y;
                anim.SetTrigger("trigger_wallSliding");
            }
        }
    }

    void PlayerControlDisableHandler()
    {
        if (!isControlDisabled) return;

        if (isTouchingAnyWall && !isWallSliding)
        {
            currentMoveSpeed = 0;
            isControlDisabled = false;
            return;
        }

        controlDisableDuration -= Time.deltaTime;
        if (controlDisableDuration <= 0)
        {
            if (hasInput || isGrounded || isWallSliding)
            {
                isControlDisabled = false;
            }
        }
    }

    void SetPlayerControlDisableDuration(float time)
    {
        isControlDisabled = true;
        controlDisableDuration = time;
    }

    void DashHandler()
    {
        if (!canDash && !isDashing)
        {
            if (isWallSliding)
            {
                canDash = true;
                currentDashCooldown = dashCooldown;
            }

            if (isGrounded)
            {
                canDash=true;
            }
        }

        if (!isDashing)
        {
            //DashOperation1();
            DashOperation2();
        }
        else
        {
            currentDashDuration += Time.deltaTime;

            if (currentDashDuration >= dashDuration || isTouchingAnyWall)
            {
                isDashing = false;
                SetPlayerControlDisableDuration(0);
            }

            if (isTouchingAnyWall)
            {
                isDashing = false;
                return;
            }
        }
    }

    void DashOperation1()
    {
        if (currentDashCooldown < dashCooldown)
        {
            currentDashCooldown += Time.deltaTime;
            return;
        }

        if (isTouchingAnyWall || isQuickTurning || isWallSliding || normalizedSpeed < 0.5f || !canDash || isShielding) return;
        // 대쉬 불가능 조건 : 벽에 닿음, 월 슬라이딩 도중, 퀵턴 도중, 속도가 전체의 50%를 넘지 못함, 공중에서 이미 대쉬 씀, 방패 사용 도중

        if (Input.GetKey(KeyCode.LeftShift))    // 대쉬 작동
        {
            isDashing = true;
            canDash = false;
            anim.SetTrigger("trigger_dash");
            dashDirection = lastMoveInput;
            currentDashDuration = 0;
            currentDashCooldown = 0;
            currentMoveSpeed = dashSpeed = Mathf.Min(currentMoveSpeed * dashSpeedMultiplier, maxSpeed * dashSpeedMultiplier);
            dashVerticalVelocity = rb.velocity.y;
        }
    }

    void DashOperation2()
    {
        if (currentDashCooldown < dashCooldown)
        {
            currentDashCooldown += Time.deltaTime;
            return;
        }

        if (isTouchingAnyWall || isQuickTurning || isWallSliding || !canDash || isShielding || (isAttacking && !allowDashCancel)) return;
        // 대쉬 불가능 조건 : 벽에 닿음, 월 슬라이딩 도중, 퀵턴 도중, 공중에서 이미 대쉬 씀, 방패 사용 도중, 공격 중

        if (Input.GetKey(KeyCode.LeftShift))    // 대쉬 작동
        {
            isDashing = true;
            canDash = false;
            anim.SetTrigger("trigger_dash");
            dashDirection = lastMoveInput;
            currentDashDuration = 0;
            currentDashCooldown = 0;
            dashSpeed = Mathf.Min(currentMoveSpeed * dashSpeedMultiplier, maxSpeed * dashSpeedMultiplier);  // 최고 속도 제한
            currentMoveSpeed = dashSpeed = Mathf.Max(dashSpeed, maxSpeed * 0.7f);   // 최소 속도 제한
            dashVerticalVelocity = rb.velocity.y;
        }
    }

    void ShildHandler()
    {
        if (isEquippingShield)
        {
            if (currentShieldEquipTime >= shieldEquipDuration)
            {
                isEquippingShield = false;
            }
            else
            {
                currentShieldEquipTime += Time.deltaTime;
            }
        }

        if (isShielding)
        {
            if (isParrying) return;

            if (Input.GetMouseButtonUp(1) || !isGrounded)
            {
                isShielding = isEquippingShield = false;
                currentShieldEquipTime = shieldEquipDuration;
            }
        }
        else if (!isDashing && !isQuickTurning && isGrounded)
        {
            if (Input.GetMouseButtonDown(1))
            {
                isShielding = isEquippingShield = true;
                currentShieldEquipTime = 0;

                anim.SetTrigger("trigger_shieldOn");
            }
        }
    }

    void ParryHandler()
    {
        if (isParrying)
        {
            currentParryDuration += Time.deltaTime;
            if (currentParryDuration >= parryDuration)
            {
                isParrying = false;
                isShielding = false;
            }
        }
        else
        {
            if (!isShielding || isEquippingShield) return;

            if (Input.GetMouseButtonDown(0))
            {
                isParrying = true;
                currentParryDuration = 0;
                anim.SetTrigger("trigger_parry");
            }
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

        // 하강 중인지 (가속 y가 0 이하라면 하강)
        isFalling = (rb.velocity.y < 0);


        // 지상인지 확인
        bool isGroundedLeftFoot = Physics2D.OverlapCircle(groundCheckLeft.position, layerCheckRadius, groundLayer);
        bool isGroundedCenterFoot = Physics2D.OverlapCircle(groundCheckCenter.position, layerCheckRadius, groundLayer);
        bool isGroundedRightFoot = Physics2D.OverlapCircle(groundCheckRight.position, layerCheckRadius, groundLayer);

        if (isGroundedLeftFoot || isGroundedCenterFoot || isGroundedRightFoot)
        {
            coyoteTime = 0;
            isGrounded = true;
        }

        // 탈 수 있는 벽에 붙어있는지 확인
        bool isClimbableWallDetectedTop = Physics2D.OverlapCircle(wallCheckTop.position, layerCheckRadius, wallLayer);
        bool isClimbableWallDetectedBottom = Physics2D.OverlapCircle(wallCheckBottom.position, layerCheckRadius, wallLayer);
        isTouchingClimbableWall = isClimbableWallDetectedTop || isClimbableWallDetectedBottom;

        // 아무 벽에 붙어있는지 확인
        bool isWallDetectedTop = Physics2D.OverlapCircle(wallCheckTop.position, layerCheckRadius, groundLayer);
        bool isWallDetectedBottom = Physics2D.OverlapCircle(wallCheckBottom.position, layerCheckRadius, groundLayer);
        isTouchingAnyWall = isWallDetectedTop || isWallDetectedBottom || isTouchingClimbableWall;
    }

    void CoyoteTimeHandler()
    {
        if (!isGrounded) return;

        coyoteTime += Time.deltaTime;

        if (coyoteTime >= coyoteTimeDuration)
        {
            isGrounded = false;
        }
    }

    void AttackHandler()
    {
        if (ShouldCancelAttack())
        {
            isAttacking = false;
            return;
        }

        if (isAttacking)
        {
            if (ShouldCancelAttack())
            {
                isAttacking = false;
                return;
            }

            timeSinceLastAttack += Time.deltaTime;

            if (timeSinceLastAttack >= attackCooldown)
            {
                allowDashCancel = true;
                if (Input.GetMouseButton(0))
                {
                    allowDashCancel = false;
                    timeSinceLastAttack = 0;

                    currentAttackMotionNumber ++;
                    if (currentAttackMotionNumber > maxAttackMotions)
                    {
                        currentAttackMotionNumber = 1;
                    }

                    Attack();
                    anim.SetTrigger("trigger_attack");
                }
                else if (timeSinceLastAttack >= attackCooldown + comboMaxDuration)
                {
                    isAttacking = false;
                    anim.SetTrigger("trigger_attackEnd");
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                isAttacking = true;
                allowDashCancel = false;
                currentAttackMotionNumber = 1;
                timeSinceLastAttack = 0;
                attackStartDirection = lastMoveInput;

                Attack();
                anim.SetTrigger("trigger_attack");
            }
        }
    }

    bool ShouldCancelAttack()
    {
        return isDashing || isQuickTurning || isWallSliding || isShielding;
    }


    void Attack()
    {
        Vector2 actualCenter = (Vector2)transform.position + hitBoxCenter;

        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
            actualCenter,
            new Vector2(hitBoxSize, hitBoxSize),
            0f,
            attackableLayer // 공격 가능한 객체만 감지하도록 LayerMask 사용
        );

        foreach (Collider2D hit in hitColliders)
        {
            GameObject target = hit.gameObject;
            target.SendMessage("Attacked", SendMessageOptions.DontRequireReceiver);
        }
    }

    void UpdateAnimation()
    {
        anim.SetBool("bool_isRunning", isRunning);
        anim.SetBool("bool_isFalling", isFalling);
        anim.SetBool("bool_isGrounded", isGrounded);
        anim.SetBool("bool_isQuickTurning", isQuickTurning);
        anim.SetBool("bool_isWallSliding", isWallSliding);
        anim.SetBool("bool_isDashing", isDashing);
        anim.SetBool("bool_isShielding", isShielding);

        anim.SetInteger("int_attackType", currentAttackMotionNumber);

        anim.SetFloat("float_moveSpeed", normalizedSpeed);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckLeft.position, layerCheckRadius);
        Gizmos.DrawWireSphere(groundCheckCenter.position, layerCheckRadius);
        Gizmos.DrawWireSphere(groundCheckRight.position, layerCheckRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckTop.position, layerCheckRadius);
        Gizmos.DrawWireSphere(wallCheckBottom.position, layerCheckRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(wallKickEffectPos.position, layerCheckRadius);

        Gizmos.color = Color.red;
        Vector3 actualCenter = transform.position + (Vector3)hitBoxCenter;
        Gizmos.DrawWireCube(actualCenter, new Vector3(hitBoxSize, hitBoxSize, 0.1f));
    }
}
