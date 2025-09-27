using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float acceleration = 5;  // �÷��̾� ���ӵ�
    public float deceleration = 25;    // �÷��̾� ���ӵ�
    public float maxSpeed = 15; // �ְ� �ӵ�
    public float jumpForce = 10;    // ���� ��

    [Header("���� ���� ��ġ")]    // Transform 3�� �� �ϳ��� groundLayer�� ������� ���� ������ ��
    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public LayerMask groundLayer;

    [Header("�� ���� ��ġ")]
    public Transform wallCheckTop;
    public Transform wallCheckBottom;
    public LayerMask wallLayer;

    [Header("����Ʈ ��ġ / ������")]
    public GameObject wallKickEffect;
    public Transform wallKickEffectPos;

    private float moveInput = 0;    // ���� ���� �Է� (A : -1 , D : 1)
    private float lastMoveInput = 1;    // ���������� ���� ����Ű
    private float currentMoveSpeed = 0; // ���� ������ �ӵ� (���ӵ� �ݿ�)
    private float normalizedSpeed = 0;  // ����ȭ�� �ӵ�
    private float layerCheckRadius = 0.05f;  // ���� ��ġ �ݰ�
    private float quickTrunDuration = 0.3f;    // ���� �ִ� ����
    private float quickTrunTime = 0; // ���� ���� ����
    private float quickTurnDirection = 1;   // �� ���� ����
    private float coyoteTimeDuration = 0.1f; // �ڿ��� Ÿ�� ����
    private float coyoteTime = 0; // ���� �ڿ���Ÿ��
    private float timeSinceLastJump = 0; // ������ �����κ��� ���� �ð�
    private float controlDisableDuration = 0;   // ���� ��Ȱ��ȭ ���� �ð� (0���� �۰ų� ������ ��Ȱ��ȭ)
    private float currentWallSlidingSpeed = 0;  // ���� �� �����̵� �ӵ�
    private float WallSlidingSpeed = -0.1f;  // ��ǥ �� �����̵� �ӵ�


    private bool isControlDisabled = false; // ������ ��Ȱ��ȭ���� ���� (��ű�� ���)
    private bool isFacingRight = true;  // �������� �ٶ󺸰� �ִ°�? (���� ��ȯ�� �ʿ���)
    private bool isRunning = false; // �����̴� ���ΰ�?
    private bool hasInput = false; // �Է��� �ִ°�? (A �Ǵ� D�� ������ �� true. ��, ���ÿ� ������ ���� ������.)
    private bool isGrounded = false;    // ���� ��Ҵ°�? (����, ���� ������ �̿�)
    private bool isFalling = false; // �������� �ִ°�? (�ö󰡴� ���̶�� false)
    private bool isQuickTurning = false;    // �� �� �����ΰ�?
    private bool isTouchingClimbableWall = false;   // ���� �� �ִ� ���� ��� �ִ°�?
    private bool isTouchingAnyWall = false; // ���� ��� �ִ°�? (�ƹ� ���̳� �پ������� true. ���� �� �ִ� �� ����)
    private bool isWallSliding = false; // �� �����̵� �����ΰ�?


    // �Ӽ� ����
    private Rigidbody2D rb;
    private Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateStates(); // ���� ������Ʈ
        PlayerControlDisableHandler();  // ���� �ߴ� �ð� ���
        CoyoteTimeHandler(); // �ڿ��� Ÿ�� �ð� ���
        MoveInputHandler(); // ���� Ű ����

        CheckFlip();    // ĳ���� �¿� ȸ��
        QuickTurn(); // ���� (�⺻ �̵� ���� �۵����� ����)
        WallSlidingHandler(); // �� �����̵�, �� ű �ִϸ��̼� Ʈ����
        HandleMovement();   // ������ Ű�� ������� ������
        JumpHandler();  // ����, ���� �ִϸ��̼� Ʈ����
        UpdateAnimation(); // �ִϸ��̼� ������Ʈ (�޸���, ����, ���� ����, ������ �ӵ�, �߶� ����)
    }

    void CheckFlip()
    {
        if (isControlDisabled) return;

        if ((moveInput > 0 && !isFacingRight) || (moveInput < 0 && isFacingRight)) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        if (normalizedSpeed >= 0.7f && isGrounded && !isQuickTurning)    // �ְ� �ӵ� ���� 70% �̻��� �ӵ����� ������ �۵��� �� ����
        {
            StartQuickTurn();
        }
    }

    void MoveInputHandler()
    {
        if (IsSingleInput())
        {
            if (Input.GetKey(KeyCode.A)) moveInput = -1;
            else moveInput = 1;

            hasInput = true;
            if (!isControlDisabled) lastMoveInput = moveInput;
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
        if (isControlDisabled || isQuickTurning) return;

        if (!isWallSliding)
        {
            if (hasInput)
            {
                currentMoveSpeed = Mathf.Min(currentMoveSpeed + acceleration * Time.deltaTime, maxSpeed);
            }
            else
            {
                currentMoveSpeed = Mathf.Max(currentMoveSpeed - deceleration * Time.deltaTime, 0);
            }

            if (isTouchingAnyWall)
            {
                currentMoveSpeed = 0;
            }
            rb.velocity = new Vector2(lastMoveInput * currentMoveSpeed, rb.velocity.y);
        }
        else    // �� �����̵� ���� ���
        {
            if (currentWallSlidingSpeed > WallSlidingSpeed)
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

        normalizedSpeed = currentMoveSpeed / maxSpeed;
    }

    void JumpHandler()
    {
        timeSinceLastJump += Time.deltaTime;

        if (!isGrounded) return;

        if (Input.GetKey(KeyCode.Space))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            anim.SetTrigger("trigger_jump");
            coyoteTime = coyoteTimeDuration;
            timeSinceLastJump = 0;
        }
    }

    void StartQuickTurn()
    {
        isQuickTurning = true;
        quickTrunTime = 0;
        quickTurnDirection = -moveInput;
    }

    void QuickTurn()
    {
        if (!isQuickTurning) return;

        quickTrunTime += Time.deltaTime;
        rb.velocity = new Vector2(quickTurnDirection * currentMoveSpeed * (1 - (quickTrunTime / quickTrunDuration)), rb.velocity.y);
        
        // �� �� ���� ����
        // ���� �ð� �ʰ�, ���� ���� ������ �ٽ� ��ȯ, �߶� (������ ������)
        if ((quickTrunTime >= quickTrunDuration) || !isGrounded)
        {
            isQuickTurning = false;
        }
        // ������ �ٽ� ��ȯ���� ������, ���� �ӵ��� ���� ����
        if (quickTurnDirection == moveInput)
        {
            isQuickTurning = false;
            currentMoveSpeed *= 0.5f;
        }
    }

    void WallSlidingHandler()
    {
        if (!isWallSliding)
        {
            if (!isGrounded && isTouchingClimbableWall && timeSinceLastJump > 0.3f) // �� �����̵� Ʈ����
            {
                SetPlayerControlDisableDuration(0.1f);
                isWallSliding = true;
                currentMoveSpeed = 0;
                currentWallSlidingSpeed = rb.velocity.y;
                anim.SetTrigger("trigger_wallSliding");
            }
        }
        else
        {

            if (isGrounded)
            {
                isWallSliding = false;
                Flip();

                return;
            }

            if (!isTouchingClimbableWall)
            {
                isWallSliding = false;

                return;
            }

            if (Input.GetKey(KeyCode.Space) && !isControlDisabled)    // �� ű �۵�
            {
                GameObject newWallKickEffect = Instantiate(wallKickEffect, wallKickEffectPos.position, Quaternion.identity);
                newWallKickEffect.transform.localScale = new Vector3(
                    newWallKickEffect.transform.localScale.x * lastMoveInput,
                    newWallKickEffect.transform.localScale.y,
                    newWallKickEffect.transform.localScale.z);
                
                currentMoveSpeed = maxSpeed;
                rb.velocity = new Vector2(jumpForce * -lastMoveInput, jumpForce);
                Flip();
                lastMoveInput = - lastMoveInput;
                SetPlayerControlDisableDuration(0.15f);
                anim.SetTrigger("trigger_wallKick");
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

    void UpdateStates()
    {
        // �����̴��� Ȯ��
        if (currentMoveSpeed <= 0)
        {
            isRunning = false;
        }
        else
        {
            isRunning = true;
        }

        // �ϰ� ������ (���� y�� 0 ���϶�� �ϰ�)
        isFalling = (rb.velocity.y < 0);


        // �������� Ȯ��
        bool isGroundedLeftFoot = Physics2D.OverlapCircle(groundCheckLeft.position, layerCheckRadius, groundLayer);
        bool isGroundedCenterFoot = Physics2D.OverlapCircle(groundCheckCenter.position, layerCheckRadius, groundLayer);
        bool isGroundedRightFoot = Physics2D.OverlapCircle(groundCheckRight.position, layerCheckRadius, groundLayer);

        if (isGroundedLeftFoot || isGroundedCenterFoot || isGroundedRightFoot)
        {
            coyoteTime = 0;
            isGrounded = true;
        }

        // Ż �� �ִ� ���� �پ��ִ��� Ȯ��
        bool isClimbableWallDetectedTop = Physics2D.OverlapCircle(wallCheckTop.position, layerCheckRadius, wallLayer);
        bool isClimbableWallDetectedBottom = Physics2D.OverlapCircle(wallCheckBottom.position, layerCheckRadius, wallLayer);
        isTouchingClimbableWall = isClimbableWallDetectedTop || isClimbableWallDetectedBottom;

        // �ƹ� ���� �پ��ִ��� Ȯ��
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

    void UpdateAnimation()
    {
        anim.SetBool("bool_isRunning", isRunning);
        anim.SetBool("bool_isFalling", isFalling);
        anim.SetBool("bool_isGrounded", isGrounded);
        anim.SetBool("bool_isQuickTurning", isQuickTurning);
        anim.SetBool("bool_isWallSliding", isWallSliding);

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
}
