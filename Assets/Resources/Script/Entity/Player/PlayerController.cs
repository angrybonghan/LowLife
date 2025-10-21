using Unity.Mathematics;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("기본 이동")]
    public float acceleration = 12;  // 플레이어 가속도
    public float deceleration = 50;    // 플레이어 감속도
    public float maxSpeed = 12; // 최고 속도
    public float jumpForce = 10;    // 점프 힘
    public float quickTrunDuration = 0.3f;    // 퀵턴 최대 길이
    public float shieldSpeed​Multiplier = 1;   // 방패로 인한 감속도 배율
    [Header("대쉬")]

    public float dashDuration = 0.25f;  // 대쉬 유지 시간
    public float dashSpeedMultiplier = 1.4f;   // 현재 속도에서 배수로 가속할 대쉬 속도
    public float dashCooldown = 0.5f;  // 대쉬 재사용 대기시간
    public GameObject dashEffect;   // 대쉬 잔상 이펙트 프리팹

    [Header("방패")]
    public float shieldEquipDuration = 0.35f;   // 방패를 꺼내는 시간
    public float parryDuration = 0.4f;  // 방패로 튕겨내는 시간
    
    [Header("방패 게이지")]
    public float shieldRechargeDuration = 3;    // 방패가 완전 방전에서 완전 충전으로 회복되는 시간
    public Color maxShieldGaugeColor = Color.green; // 방패 완충에서 방패 게이지 색상
    public Color minShieldGaugeColor = Color.red; // 방패 방전에서 방패 게이지 색상
    public GameObject shieldGaugeUI;    // 방패 게이지 UI 게임오브젝트
    public float defaultShieldUIFadeDelay = 1f;


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

    [Header("원거리 공격")]
    public float shieldThrowDuration = 0.25f;   // 방패를 던지는 시간
    public float shieldThrowInterval = 0.45f;   // 방패를 잡고 다시 던지기까지의 시간
    public Transform shieldSummonPos;    // 방패 소환 위치
    public GameObject shieldPrefabs;     // 방패 프리팹

    [Header("원거리 공격 모드")]
    public GameObject crosshairPrefabs; // 조준점 프리팹
    public Transform crosshairSummonPos;    // 원거리 모드를 켰을 때 크로스헤어(조준점) 이 소환될 위치.
    public GameObject ShieldSprite;  // 방패 스프라이트
    public GameObject postProcessVolumeObject;  // 원거리 모드에서 켜질 화면 필터 (Post-process Volume) 오브젝트

    [Header("방패 도약")]
    public float shieldLeapShieldGaugeDecrease = 0.5f;
    public GameObject shieldLeapAirEffectPrefab; // 방패 도약 이펙트 프리팹 - 공중
    public GameObject shieldLeapGroundImpactPrefab; // 방패 도약 이펙트 프리팹 - 지상
    public GameObject shieldLeapWallSlideEffectPrefab; // 방패 도약 이펙트 프리팹 - 월 슬라이딩

    [Header("test")]
    public GameObject testBlackScreenUI;    // (테스트용) 사망 시 보여줄 검정색 UI 패널


    // =========================================================================
    // 1. 이동 및 방향 상태 (Movement & Direction)
    // =========================================================================
    private float moveInput = 0;             // 현재 방향 입력 (A: -1, D: 1)
    private float lastMoveInput = 1;         // 마지막으로 누른 방향키
    private float currentMoveSpeed = 0;      // 현재 움직임 속도 (가속도 반영)
    private float normalizedSpeed = 0;       // 정규화된 속도 (0~1)
    private float knockbackPower = 0;       // 넉백의 밀림 정도
    private float currentKnockbacktime = 0; // 넉백의 길이 (시간)

    private bool isFacingRight = true;      // 오른쪽을 바라보고 있는가? (방향 전환)
    private bool isRunning = false;         // 움직이는 중인가?
    private bool hasInput = false;          // 유효한 입력이 있는가? (A 또는 D)
    private bool hasKnockback = false;      // 넉백 값이 있는지

    // =========================================================================
    // 2. 점프 및 공중 상태 (Jump & Air)
    // =========================================================================
    private float coyoteTimeDuration = 0.1f; // 코요테 타임 길이
    private float coyoteTime = 0;            // 현재 코요테타임 잔여 시간
    private float timeSinceLastJump = 0;     // 마지막 점프로부터 지난 시간

    private bool isGrounded = false;        // 땅에 닿았는가?
    private bool isFalling = false;         // 떨어지고 있는가? (상승 중이라면 false)

    // =========================================================================
    // 3. 대쉬 상태 (Dash)
    // =========================================================================
    private float dashSpeed;                // 대쉬 속도
    private float dashDirection = 0;        // 대쉬 방향
    private float currentDashDuration = 0;  // 현재 대쉬 진행 시간
    private float currentDashCooldown = 0;  // 대쉬 이후 흐른 시간 (쿨다운 계산용)
    private float dashVerticalVelocity = 0; // 대쉬 수직 속도
    private float dashFixedYPosition = 0;   // 수평 대쉬 도중 고정될 Y좌표

    private bool isDashing = false;         // 대쉬 중인가?
    private bool canDash = false;           // 대쉬 가능한가? (재충전 여부)

    // =========================================================================
    // 4. 벽 관련 상태 (Wall Interaction)
    // =========================================================================
    private float currentWallSlidingSpeed = 0; // 현재 월 슬라이딩 속도
    private float WallSlidingSpeed = -0.1f;    // 목표 월 슬라이딩 속도
    private float WallKickDisableDuration = 0; // 월 킥 비활성화 잔여 시간

    private bool isTouchingClimbableWall = false; // 붙을 수 있는 벽에 닿아 있는가?
    private bool isTouchingAnyWall = false;     // 벽에 닿아 있는가? (모든 벽)
    private bool isWallSliding = false;         // 월 슬라이딩 도중인가?

    // =========================================================================
    // 5. 공격 및 방패 상태 (Attack & Shield)
    // =========================================================================
    private float currentShieldEquipTime = 0;  // 현재 방패 든 시간 (쿨다운 계산용)
    private float currentParryDuration = 0;    // 현재 패링 시간 (지속시간 계산용)
    private float attackStartDirection = 0;    // 공격을 시작한 시점의 방향
    private float shieldPitchNormalized = 0;   // 방패의 수직 각도(Pitch) 정규화 값
    private float currentShieldThrowDuration = 0;     // 현재 방패를 던지고 있는 시간 (쿨다운 계산용)
    private float currentShieldThrowInterval = 0;      // 방패를 잡은 직후 지난 시간 (쿨다운 계산용)
    private float shieldGauge = 1; //방패 게이지 값
    private float shieldUIFadeDelay = 0;    // 방패가 투명해지기 시작할 때까지 지연 시간

    private bool isShielding = false;         // 방패를 들고있는 중인가?
    private bool isEquippingShield = false;   // 방패를 꺼내는 중인가?
    private bool isThrowingShield = false;    // 방패를 던지는 중인가?
    private bool isParrying = false;          // 패링 중인가?
    private bool isAttacking = false;         // 공격 중인가?
    private bool allowDashCancel = false;     // 공격 도중 대쉬로 취소 가능 여부
    private bool hasShield = true;             // 방패를 가지고 있는지 여부
    private bool canThrow = true;             // 방패를 던질 수 있는지 여부
    private bool isFlippedAfterThrowShield = false; // 방패를 투척했을 때, 뒤로 던졌는지에 대한 여부
    private bool isShieldGaugeFadingOut = false;  // 방패 UI가 페이드 아웃 중인지에 대한 여부
    private bool isShieldGaugeHidden = true;    // 방패 게이지가 숨겨져 있는지에 대한 여부

    private GameObject shieldInstance;     // 조준점 게임오브젝트 레퍼런스
    private ShieldMovement shieldScript;    // 방패 움직임 스크립트
    private SpriteRenderer shieldGaugeRenderer;  // 방패 게이지 UI 이미지 스프라이트 렌더러
    private Coroutine shieldGaugeFadeoutCoroutine;  // 쉴드 게이지 페이드 아웃 코루틴

    // =========================================================================
    // 6. 특수 동작 및 제어 (Special & Control)
    // =========================================================================
    private float quickTrunTime = 0;         // 현재 퀵턴 길이
    private float quickTurnDirection = 1;    // 퀵 턴의 방향
    private float layerCheckRadius = 0.05f;  // 감지 위치 반경
    private float controlDisableDuration = 0;  // 조작 비활성화 유지 잔여 시간

    private bool isControlDisabled = false; // 조작을 비활성화할지 여부
    private bool isQuickTurning = false;    // 퀵 턴 도중인가?

    // =========================================================================
    // 7. 기타 오브젝트 레퍼런스, 변수
    // =========================================================================
    private GameObject crosshairInstance;     // 조준점 게임오브젝트 레퍼런스
    private float startGravityScale;    // 시작 당시의 중력 값


    // 속성, 스크립트 참조
    private Rigidbody2D rb;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        startGravityScale = rb.gravityScale;
    }

    private void Start()
    {
        AimOn();
        if (shieldGaugeUI != null)
        {
            shieldGaugeRenderer = shieldGaugeUI.GetComponent<SpriteRenderer>();

            if (shieldGaugeRenderer == null)
            {
                Debug.LogError("방패 게이지가 스프라이트 렌더러가 달려 있지 않음");
            }
        }
        else Debug.LogError("방패 게이지가 없음");

        ShieldGaugeHide();
    }

    void Update()
    {
        UpdateStates(); // 상태 업데이트
        PlayerControlDisableHandler();  // 조작 중단 시간 계산
        CoyoteTimeHandler(); // 코요테 타임 시간 계산
        MoveInputHandler(); // 조작 키 감지

        AttackHandler(); // 근접 공격 작동, 근접 공격 애니메이션 트리거
        CheckFlip();    // 캐릭터 좌우 회전, 퀵턴 작동
        WallSlidingHandler(); // 월 슬라이딩, 월 킥 애니메이션 트리거
        JumpHandler();  // 점프 작동, 점프 애니메이션 트리거
        DashHandler(); // 대쉬 트리거, 대쉬 애니메이션 트리거
        ParryHandler(); // 패링 작동, 애니메이션 트리거
        ShieldHandler(); // 방패 전개, 방패 해제, 방패 애니메이션 트리거
        UpdateAnimation(); // 애니메이션 업데이트 (달리기, 퀵턴, 공중 상태, 움직임 속도, 추락 감지)


        if (Input.GetKeyDown(KeyCode.Q))
        {
            DepleteShieldGauge(0.1f);
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();   // 모든 상태에 대한 움직임
    }

    void CheckFlip()
    {
        if (isControlDisabled || isDashing || isParrying || isThrowingShield) return;

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
        if (hasKnockback)
        {
            KnockbackHandler();
            return;
        }

        if (isAttacking)    // 공격 감속 (+ 에어브레이크)
        {
            AttackingMovement();
            return;
        }

        if (isThrowingShield)   // 방패 던지는 도중 가속 방지
        {
            ThrowingMovement();
            return;
        }

        if (isShielding)    // 방패 도중 움직임
        {
            ShieldMovement();
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

    void ThrowingMovement()
    {
        if (moveInput != attackStartDirection)   // 방향과 다른 키거나 키가 없으면 감속 (원래의 0.2 배로 감속함)
        {
            currentMoveSpeed = Mathf.Max(currentMoveSpeed - (deceleration * Time.deltaTime * 0.2f), 0);
        }

        if (isTouchingAnyWall)  // 벽과 충돌하면 속도 없어짐
        {
            currentMoveSpeed = 0;
        }
        rb.velocity = new Vector2(attackStartDirection * currentMoveSpeed, rb.velocity.y);

        normalizedSpeed = currentMoveSpeed / maxSpeed;
    }

    void ShieldMovement()
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
                lastMoveInput = -lastMoveInput;
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
            DashOperation();
        }
        else
        {
            currentDashDuration += Time.deltaTime;

            if (currentDashDuration >= dashDuration || isTouchingAnyWall || isThrowingShield)
            {
                isDashing = false;
                SetPlayerControlDisableDuration(0);
                NoGravityOff();
            }

            if (isTouchingAnyWall || isAttacking)
            {
                isDashing = false;
                NoGravityOff();
                return;
            }
        }
    }

    void DashOperation()
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
            dashFixedYPosition = transform.position.y;  // 대쉬가 시작된 Y좌표 저장

            dashVerticalVelocity = rb.velocity.y;
            if (Mathf.Abs(dashVerticalVelocity) < 4)    // 일정한 수직 가속이 없다면 수평 대쉬로 강제함
            {
                dashVerticalVelocity = 0;
            }

            NoGravityOn();
        }
    }

    void NoGravityOn()
    {
        rb.gravityScale = 0;
    }

    void NoGravityOff()
    {
        rb.gravityScale = startGravityScale;
    }

    void ShieldHandler()
    {
        if (!hasShield) return;

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
            SetShieldUIFadeOutDelay();

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
                SetShieldUIFadeOutDelay();
            }
        }

        ShieldGaugeHandler();
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
        ThrowCooldownHandler();

        ShieldLeapHandler();
        RangedAttackHandler();
    }


    void ThrowCooldownHandler()
    {
        if (!canThrow)
        {
            currentShieldThrowInterval += Time.deltaTime;

            if (currentShieldThrowInterval >= shieldThrowInterval)
            {
                canThrow = true;
            }
        }

        if (isThrowingShield)
        {
            currentShieldThrowDuration += Time.deltaTime;

            if (isDashing || currentShieldThrowDuration >= shieldThrowDuration)
            {
                isThrowingShield = false;
                rb.gravityScale = startGravityScale;

                if (isFlippedAfterThrowShield)
                {
                    transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                }
                return;
            }
        }
    }
    void RangedAttackHandler()
    {
        if (!canThrow || !hasShield || isThrowingShield || isWallSliding || isParrying || isShielding) return;
        // 방패 투척 불가능 조건 : 투척 쿨다운 중, 방패 없음, 방패 던지는 중, 벽에 붙었음, 패링 중, 방패로 막는 중
        if (crosshairInstance == null) return;
        // 또는 조준점 없음

        if (Input.GetMouseButton(0))
        {
            ThrowShield();
        }
    }

    void ThrowShield()
    {
        hasShield = false;
        ShieldSprite.SetActive(false);

        shieldInstance = Instantiate(shieldPrefabs, shieldSummonPos.position, quaternion.identity);
        shieldScript = shieldInstance.GetComponent<ShieldMovement>();

        Vector2 shootDirection = crosshairInstance.transform.position - transform.position;
        shieldScript.InitializeThrow(shootDirection, this);

        shieldPitchNormalized = GetNormalizedShieldPitch(shootDirection);
        anim.SetFloat("float_shieldPitchNormalized", shieldPitchNormalized);
        anim.SetTrigger("trigger_attack_ranged");

        isThrowingShield = true;
        currentShieldThrowDuration = 0;
        attackStartDirection = lastMoveInput;

        isFlippedAfterThrowShield = false;

        bool shouldFlip = (shootDirection.x >= 0 && !isFacingRight) ||
                  (shootDirection.x < 0 && isFacingRight);

        if (shouldFlip)
        {
            if (isRunning)
            {
                transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
                isFlippedAfterThrowShield = true;
            }
            else
            {
                lastMoveInput = -lastMoveInput;
                Flip();
            }
        }

        if (!isGrounded)
        {
            rb.gravityScale = startGravityScale * 0.5f;
        }
    }

    public void AimOn()
    {
        crosshairInstance = Instantiate(crosshairPrefabs, crosshairSummonPos.position, quaternion.identity);
    }

    public void AimOff()
    {
        Destroy(crosshairInstance);
        crosshairInstance = null;
    }

    public float GetNormalizedShieldPitch(Vector2 shootDirection)
    {
        float magnitude = shootDirection.magnitude;

        if (magnitude == 0)
        {
            return 0.5f;
        }

        float yRatio = shootDirection.y / magnitude;

        return (yRatio + 1f) / 2f;
    }

    public void CatchShield()   // 이 스크립트는 던져진 방패에서 호출됨
    {
        Destroy(shieldInstance);
        shieldInstance = null;
        

        hasShield = true;
        ShieldSprite.SetActive(true);

        canThrow = false;
        currentShieldThrowInterval = 0;
    }

    void ShieldLeapHandler()
    {
        if (shieldScript == null) return;
        if (hasShield || shieldScript.isReturning) return;
        if (shieldGauge < shieldLeapShieldGaugeDecrease) return;

        if (Input.GetMouseButtonDown(0))
        {
            isDashing = false;
            NoGravityOff();

            GameObject newEffect;
            if (isGrounded)
            {
                newEffect = Instantiate(shieldLeapGroundImpactPrefab);
            }
            else if (isWallSliding)
            {
                newEffect = Instantiate(shieldLeapWallSlideEffectPrefab);
            }
            else
            {
                newEffect = Instantiate(shieldLeapAirEffectPrefab);
            }

            newEffect.transform.position = transform.position;
            newEffect.transform.localScale = new Vector3(
                newEffect.transform.localScale.x * lastMoveInput,
                newEffect.transform.localScale.y,
                newEffect.transform.localScale.z);

            transform.position = shieldInstance.transform.position;
            CatchShield();
            DepleteShieldGauge(shieldLeapShieldGaugeDecrease);
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
    }

    void ShieldGaugeHandler()
    {
        if (!isShielding && hasShield && shieldGauge != 1)
        {
            float rechargeLevel = Time.deltaTime / shieldRechargeDuration;
            DepleteShieldGauge(-rechargeLevel);
            SetShieldUIFadeOutDelay();
        }

        if (shieldUIFadeDelay <= 0)
        {
            if (!isShieldGaugeHidden && !isShieldGaugeFadingOut)
            {
                isShieldGaugeFadingOut = true;
                shieldGaugeFadeoutCoroutine = StartCoroutine(FadeOutShieldUICoroutine(1f));
            }
        }
        else
        {
            shieldUIFadeDelay -= Time.deltaTime;
        }

        ShieldGaugeColorHandler();
    }

    void ShieldGaugeColorHandler()
    {
        Color targetColor = Color.Lerp(minShieldGaugeColor, maxShieldGaugeColor, shieldGauge);

        targetColor.a = shieldGaugeRenderer.color.a;
        shieldGaugeRenderer.color = targetColor;
    }

    IEnumerator FadeOutShieldUICoroutine(float duration)
    {
        float elapsedTime = 0f;
        float startAlpha = 1f;
        float targetAlpha = 0f;
        Color currentColor = shieldGaugeRenderer.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            currentColor = shieldGaugeRenderer.color;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);

            currentColor.a = currentAlpha;
            shieldGaugeRenderer.color = currentColor;

            yield return null;
        }

        ShieldGaugeHide();
    }

    void ShieldGaugeHide()
    {
        Color currentColor = shieldGaugeRenderer.color;
        currentColor.a = 0f;
        shieldGaugeRenderer.color = currentColor;

        shieldGaugeFadeoutCoroutine = null;

        isShieldGaugeFadingOut = false;
        isShieldGaugeHidden = true;
    }

    void SetShieldUIFadeOutDelay()
    {
        shieldUIFadeDelay = defaultShieldUIFadeDelay;

        isShieldGaugeHidden = false;

        Color currentColor = shieldGaugeRenderer.color;
        currentColor.a = 1f;
        shieldGaugeRenderer.color = currentColor;

        if (isShieldGaugeFadingOut)
        {
            StopCoroutine(shieldGaugeFadeoutCoroutine);
            shieldGaugeFadeoutCoroutine = null;

            isShieldGaugeFadingOut = false;
        }
    }

    void KnockbackHandler()
    {
        float sign = isFacingRight ? -1 : 1;
        float knockbackLevel = 3f;

        rb.velocity = new Vector2(sign * knockbackPower * knockbackLevel, rb.velocity.y);
        currentKnockbacktime -= Time.deltaTime;
        if (currentKnockbacktime <= 0)
        {
            StopKnockback();
        }
    }

    void AddKnockback(float power, float time)
    {
        if (time >= 0) hasKnockback = true;
        else StopKnockback();
        knockbackPower = power;
        currentKnockbacktime = time;
    }

    void StopKnockback()
    {
        hasKnockback = false;
        knockbackPower = 0;
        currentKnockbacktime = 0;
    }


    /// <summary>
    /// 인수 : 방패 대미지 - 넉백 정도 - 공격자 위치
    /// </summary>
    public void OnAttack(float damage, float knockbackPower, float knockbacktime, Transform attackerPos)
    {
        if (isDashing) return;

        if (isShielding)
        {
            float myX = transform.position.x;
            float attackerX = attackerPos.position.x;
            bool attackOnRight = attackerX > myX;

            if (attackOnRight == isFacingRight)
            {
                ApplyDamageToShield(damage);
                AddKnockback(knockbackPower, knockbacktime);

            }
            else Death();
        }
        else Death();
    }

    void ApplyDamageToShield(float damage)
    {
        shieldGauge -= damage;
        shieldGauge = Mathf.Clamp01(shieldGauge);
    }

    void DepleteShieldGauge(float damage)
    {
        shieldGauge -= damage;
        shieldGauge = Mathf.Clamp01(shieldGauge);
    }

    void Death()
    {
        testBlackScreenUI.SetActive(true);
    }
}
