using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(Animator))]
public class ShieldMovement : MonoBehaviour
{
    [Header("���� �� ����")]
    public float maxShieldFlightTime = 1;      // ���а� ���ư� �� �ִ� �ִ� �ð�
    public float throwSpeed = 25f;     // ���а� ���ư��� �ӵ�
    public float returnSpeed = 20f;    // ���а� �÷��̾�� ���ƿ��� �ӵ�
    public float catchDistance = 0.75f; // �÷��̾�� ������ �Ÿ�

    [Header("�ܺ� ���� ����")]
    public Transform playerPostion;    // ���и� ���� �÷��̾� ��ġ
    public Vector3 throwDirection;  // ������ ����
    public bool isReturning = false; // ���� ���а� ���ƿ��� ���ΰ�?
    [Header("����Ʈ")]
    public GameObject afterEffect;
    public float afterEffectInterval;

    private float currentFlightTime = 0; // ���� ���ư��� �ð� (�ð� ����)
    private float LastAfterEffect = 0;  // ������ �ܻ� �ð�  (�ð� ����)

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

        // �ʱ� ����
        boxCol.isTrigger = true;
        rb.gravityScale = 0f;
        isReturning = false;
        LastAfterEffect = Time.time;
    }

    void Update()
    {
        if (!isReturning)
        {
            rb.velocity = throwDirection * throwSpeed;
            currentFlightTime += Time.deltaTime;

            if (LastAfterEffect + afterEffectInterval <= Time.time)
            {
                LastAfterEffect = Time.time;
                GradientAfterimagePlayer afterImageScript = Instantiate(afterEffect, transform.position, Quaternion.identity).GetComponent<GradientAfterimagePlayer>();
                if (afterImageScript != null)
                {
                    afterImageScript.SetColorLevel(currentFlightTime / maxShieldFlightTime);
                }
            }

            if (currentFlightTime >= maxShieldFlightTime)
            {
                SetReturnState(); // ���� ���·� ��ȯ
            }
        }
        else // �ǵ��ƿ��� ����
        {
            if (playerPostion != null)
            {
                Vector3 directionToPlayer = (playerPostion.position - transform.position).normalized;
                rb.velocity = directionToPlayer * returnSpeed;

                // �÷��̾�� ����
                if (Vector2.Distance(playerPostion.position, transform.position) <= catchDistance)
                {
                    playerController.CatchShield();
                    Destroy(gameObject);
                }
            }
            else
            {
                // �÷��̾� ������ ������ٸ�, ���� �ı�
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

    private void SetReturnState()
    {
        isReturning = true;
        rb.gravityScale = 0f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<I_Attackable>(out I_Attackable targetAttackable))
        {
            targetAttackable.OnAttack(transform);
        }

        if (!isReturning)
        {
            SetReturnState(); // ������ �浹 �� ���� ���·� ��ȯ
        }
    }
}