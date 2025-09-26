using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float acceleration = 5;  // �÷��̾� ���ӵ�
    public float decelerationMultiplier = 5;    // ���ӵ��� ����� �ۿ��ϴ� ���ӵ�
    public float maxSpeed = 15; // �ְ� �ӵ�
    public float jumpForce = 10;    // ���� ��

    [Header("���� ���� ��ġ")]    // Transform 3�� �� �ϳ��� groundLayer�� ������� ���� ������ ��
    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public LayerMask groundLayer;

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

    private bool isFacingRight = true;  // �������� �ٶ󺸰� �ִ°�? (���� ��ȯ�� �ʿ���)
    private bool isRunning = false; // �����̴� ���ΰ�?
    private bool hasInput = false; // �Է��� �ִ°�? (A �Ǵ� D�� ������ �� true. ��, ���ÿ� ������ ���� ������.)
    private bool isGrounded = false;    // ���� ��Ҵ°�? (����, ���� ������ �̿�)
    private bool isFalling = false; // �������� �ִ°�? (�ö󰡴� ���̶�� false)
    private bool isQuickTurning = false;    // �� �� �����ΰ�?


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
        MoveInputHandler(); // ���� Ű ����

        CheckFlip();    // ĳ���� �¿� ȸ��
        QuickTurn(); // ���� (�⺻ �̵� ���� �۵����� ����)
        HandleMovement();   // ������ Ű�� ������� ������
        JumpHandler();  // ����, ���� �ִϸ��̼� Ʈ����
        UpdateAnimation(); // �ִϸ��̼� ������Ʈ (�޸���, ����, ���� ����, ������ �ӵ�, �߶� ����)
    }

    void CheckFlip()
    {
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
        if (isQuickTurning) return;

        if (hasInput)
        {
            currentMoveSpeed = Mathf.Min(currentMoveSpeed + acceleration * Time.deltaTime, maxSpeed);
        }
        else
        {
            currentMoveSpeed = Mathf.Max(currentMoveSpeed - (acceleration * Time.deltaTime) * decelerationMultiplier, 0);
        }

        rb.velocity = new Vector2(lastMoveInput * currentMoveSpeed, rb.velocity.y);

        normalizedSpeed = currentMoveSpeed / maxSpeed;
    }

    void JumpHandler()
    {
        if (!isGrounded) return;

        if (Input.GetKey(KeyCode.Space))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            anim.SetTrigger("trigger_jump");
            coyoteTime = coyoteTimeDuration;
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
        bool leftFoot = Physics2D.OverlapCircle(groundCheckLeft.position, layerCheckRadius, groundLayer);
        bool centerFoot = Physics2D.OverlapCircle(groundCheckCenter.position, layerCheckRadius, groundLayer);
        bool rightFoot = Physics2D.OverlapCircle(groundCheckRight.position, layerCheckRadius, groundLayer);

        if (leftFoot || centerFoot || rightFoot)
        {
            coyoteTime = 0;
            isGrounded = true;
        }
        else
        {
            coyoteTime += Time.deltaTime;
            if (coyoteTime >= coyoteTimeDuration)
            {
                isGrounded = false;
            }
        }
    }

    void UpdateAnimation()
    {
        anim.SetBool("bool_isRunning", isRunning);
        anim.SetBool("bool_isFalling", isFalling);
        anim.SetBool("bool_isGrounded", isGrounded);
        anim.SetBool("bool_isQuickTurning", isQuickTurning);

        anim.SetFloat("float_moveSpeed", normalizedSpeed);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckLeft.position, layerCheckRadius);
        Gizmos.DrawWireSphere(groundCheckCenter.position, layerCheckRadius);
        Gizmos.DrawWireSphere(groundCheckRight.position, layerCheckRadius);
    }
}
