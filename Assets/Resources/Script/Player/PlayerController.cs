using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float acceleration = 5;  // �÷��̾� ���ӵ�
    public float decelerationMultiplier = 5;    // ���ӵ��� ����� �ۿ��ϴ� ���ӵ�
    public float maxSpeed = 15; // �ְ� �ӵ�
    public float jumpForce = 10;    // ���� ��

    [Header("���� ���� ��ġ")]
    public Transform groundCheckLeft;
    public Transform groundCheckCenter;
    public Transform groundCheckRight;
    public LayerMask groundLayer;

    private float moveInput = 0;    // ���� ���� �Է� (A : -1 , D : 1)
    private float lastMoveInput = 1;    // ���������� ���� ����Ű
    private float currentMoveSpeed = 1; // ���� ������ �ӵ� (���ӵ� �ݿ�)
    private float layerCheckRadius = 0.05f;  // ���� ��ġ �ݰ�

    private bool isFacingRight = true;  // �������� �ٶ󺸰� �ִ°�? (���� ��ȯ�� �ʿ���)
    private bool isRunning = false; // �����̴� ���ΰ�?
    private bool hasInput = false; // �Է��� �ִ°�? (A �Ǵ� D�� ������ �� true. ��, ���ÿ� ������ ���� ������.)
    private bool isGrounded = false;    // ���� ��Ҵ°�? (����, ���� ������ �̿�)

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
        HandleMovement();   // ������ Ű�� ������� ������
        JumpHandler();  // ����
        UpdateAnimation(); // �ִϸ��̼� ������Ʈ
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
        // �����̴��� Ȯ��
        if (currentMoveSpeed <= 0)
        {
            isRunning = false;
        }
        else
        {
            isRunning = true;
        }
        

        // �������� Ȯ��
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
