using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(Animator))]
public class ShildMovement : MonoBehaviour
{
    [Header("���� �� ����")]
    public float throwSpeed = 25f;  // ���а� ���ư��� �ӵ�
    public float returnSpeed = 20f; // ���а� �÷��̾�� ���ƿ��� �ӵ�
    public float shieldRecallDistance = 2f; // ��ǥ ��ġ�κ��� ������ ���·� ��ȯ�� ���� ��ġ �Ÿ�.
    [Header("�ܺ� ���� ����")]
    public Transform playerPostion;    // ���и� ���� �÷��̾� ��ġ
    public Vector3 throwDirection; // ������ ����
    public bool isShieldDropped = false;
    public bool isReturning = false;

    // ���� Ȯ�ο� ������
    private enum ShieldState { THROWN, RETURN, DROPPED }
    private ShieldState currentState = ShieldState.THROWN;

    // ������ ����
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
    /// �μ� : ���� - �÷��̾� Ʈ������
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
