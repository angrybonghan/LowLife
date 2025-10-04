using TMPro.EditorUtilities;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(Animator))]
public class ShildMovement : MonoBehaviour
{
    [Header("���� �� ����")]
    public float maxShieldFlightTime = 1;    // ���а� ���ư� �� �ִ� �ִ� �ð�
    public float throwSpeed = 25f;  // ���а� ���ư��� �ӵ�
    public float returnSpeed = 20f; // ���а� �÷��̾�� ���ƿ��� �ӵ�
    public float catchDistance = 0.75f; // �÷��̾�� ������ �Ÿ�
    [Header("�ܺ� ���� ����")]
    public Transform playerPostion;    // ���и� ���� �÷��̾� ��ġ
    public Vector3 throwDirection; // ������ ����
    public bool isShieldDropped = false;
    public bool isReturning = false;

    private float currentFlightTime = 0;    // ���� ���ư��� �ð� (�ð� ����)

    // ���� Ȯ�ο� ������
    private enum ShieldState { THROWN, RETURN, DROPPED }
    private ShieldState currentState = ShieldState.THROWN;

    // ������ ����
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
    /// �μ� : ���� - �÷��̾� ��ũ��Ʈ
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
