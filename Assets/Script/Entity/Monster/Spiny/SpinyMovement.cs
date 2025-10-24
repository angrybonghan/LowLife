using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Animator), typeof(Rigidbody2D))]
public class SpinyMovement : MonoBehaviour, I_Attackable
{

    [Header("������")]
    public float moveSpeed = 5; // ������ �ӵ�
    public float moveRadius = 2;    // ������ �� �ִ� �ݰ�
    public float rotationTime = 0.5f;   // ȸ�� �ð�

    [Header("����")]
    public float attackChargeTime = 1;  // ������ �غ� �ð�
    public float attackDuration = 2;    // ������ ���� �ð�
    public float attackOutTime = 0.5f;    // ���� ���� �ٽ� ������ �ð�
    public float attackCooldown = 3;    // ���� ���ð� (���� ��Ÿ��)

    [Header("��Ʈ�ڽ�")]
    public Vector2 hitboxOffset = new Vector2(0.5f, 0f);    // ��Ʈ�ڽ� ������
    public Vector2 hitboxSize = new Vector2(0.5f, 1); // ũ�� (width, height)
    public LayerMask targetLayer;   // ���� ���̾�

    [Header("����")]
    public float deathDuration = 2; // �״� �ð�
    public float fallingOutPower = 15; // �׾��� �� ���ư� ��

    public enum state { move, turn , attack}
    private state currentState;

    bool isGoingUp; // �������� �����̴��� ����
    bool isMoving;  // �����̰� �ִ��� ����
    bool isAttacking; // �����ϰ� �ִ��� ����
    bool isDead;    // �׾����� ����

    float currentAttackCooldown;    // ���� ���� ���ð� (��Ÿ�� ����)

    Vector3 movePosUp;
    Vector3 movePosDown;
    Vector3 targetPos;

    Coroutine turnCoroutine;
    Coroutine attackCoroutine;

    Animator anim;
    Rigidbody2D rb;
    BoxCollider2D boxCol;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        boxCol = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        SetState(state.move);
        
        if (moveRadius < 0) moveRadius *= -1;

        movePosUp = movePosDown = transform.position;
        movePosUp.y += moveRadius;
        movePosDown.y -= moveRadius;

        targetPos = movePosUp;
        isGoingUp = true;
        isAttacking = false;
        isDead = false;
    }


    void Update()
    {
        if (isDead) return;

        switch (currentState)
        {
            case state.move:
                MoveHandler();
                break;
            
            case state.turn:

                break;
        }

        if (isAttacking) Attack();

        UpdateAnimation();
    }

    void SetState(state targetState)
    {
        switch (targetState)
        {
            case state.move:
                currentState = state.move;
                isMoving = true;
                break;

            case state.turn:
                currentState = state.turn;
                isMoving = false;
                StartTurn();
                break;

            case state.attack:
                currentState = state.attack;
                isMoving = false;
                currentAttackCooldown = 0;
                StartAttack();
                break;
        }
    }

    void MoveHandler()
    {
        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        if (transform.position == targetPos)
        {
            SetState(state.turn);
        }
        else
        {
            currentAttackCooldown += Time.deltaTime;

            if (currentAttackCooldown >= attackCooldown)
            {
                SetState(state.attack);
            }
        }
    }

    void StartTurn()
    {
        isGoingUp = !isGoingUp;


        if (isGoingUp) targetPos = movePosUp;
        else targetPos = movePosDown;

        if (turnCoroutine != null) StopCoroutine(turnCoroutine);
        turnCoroutine = StartCoroutine(WaitToTurn());
    }

    IEnumerator WaitToTurn()
    {
        anim.SetTrigger("trun");
        yield return new WaitForSeconds(rotationTime);

        SetState(state.move);
        UpdateAnimation();
        Flip();
        turnCoroutine = null;
    }

    void Flip()
    {
        Vector3 currentScale = transform.localScale;
        currentScale.y *= -1f;
        transform.localScale = currentScale;
    }

    void StartAttack()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackStep());
    }

    IEnumerator AttackStep()
    {
        anim.SetTrigger("startAttack");
        yield return new WaitForSeconds(attackChargeTime);
        isAttacking = true;
        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
        yield return new WaitForSeconds(attackOutTime);

        SetState(state.move);
        attackCoroutine = null;
    }


    public bool GetRandomBool()
    {
        return Random.Range(0, 2) == 0;
    }

    float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

    private void UpdateAnimation()
    {
        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isAttacking", isAttacking);
    }

    void Attack()
    {
        Vector2 worldCenter = (Vector2)transform.position + hitboxOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // �߽� ��ġ
            hitboxSize,             // ũ��
            0f,                     // ȸ�� ����
            targetLayer             // ������ ���̾�
        );

        if (hitTargets.Length > 0)
        {
            foreach (Collider2D targetCollider in hitTargets)
            {
                if (targetCollider.CompareTag("Player"))
                {
                    if (targetCollider.TryGetComponent<PlayerController>(out PlayerController playerScript))
                    {
                        playerScript.OnAttack(1, 0, 0, transform);
                    }
                    else
                    {
                        Debug.LogWarning($"�÷��̾� �±׸� �������� ��ũ��Ʈ�� ���� ���: {targetCollider.gameObject.name}");
                    }

                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(!isDead) OnTrigger(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isDead) OnTrigger(other);
    }

    void OnTrigger(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerController>(out PlayerController playerScript))
            {
                playerScript.OnAttack(1, 0, 0, transform);
            }
            else
            {
                Debug.LogWarning($"�÷��̾� �±׸� �������� ��ũ��Ʈ�� ���� ���: {other.gameObject.name}");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 gizmoCenter = (Vector2)transform.position + hitboxOffset;
        Gizmos.DrawWireCube(gizmoCenter, new Vector3(hitboxSize.x, hitboxSize.y, 0f));

        Gizmos.color = Color.cyan;

        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(movePosUp, 0.25f);
            Gizmos.DrawWireSphere(movePosDown, 0.25f);
            Gizmos.DrawLine(movePosUp, movePosDown);
        }
        else
        {
            Vector3 gizmosMovePosUp = transform.position;
            Vector3 gizmosMovePosDown = transform.position;
            gizmosMovePosUp.y += moveRadius;
            gizmosMovePosDown.y -= moveRadius;

            Gizmos.DrawWireSphere(gizmosMovePosUp, 0.25f);
            Gizmos.DrawWireSphere(gizmosMovePosDown, 0.25f);
            Gizmos.DrawLine(gizmosMovePosUp, gizmosMovePosDown);
        }

    }

    public void OnAttack(Transform attackerTransform)
    {
        if (isAttacking || isDead) return;
        isDead = true;

        Vector2 direction = (transform.position - attackerTransform.position).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * fallingOutPower, ForceMode2D.Impulse);
        rb.freezeRotation = false;
        rb.gravityScale = 1f;
        rb.AddTorque(GetRandom(-20, 20));
        boxCol.isTrigger = false;
        anim.SetTrigger("die");
        StopAllCoroutines();
        StartCoroutine(Dead());
    }

    IEnumerator Dead()
    {
        float timer = 0f;
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;

        while (timer < deathDuration)
        {
            timer += Time.deltaTime;

            float t = timer / deathDuration;

            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            yield return null;
        }

        Destroy(gameObject);
    }
}
