using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Animator), typeof(Rigidbody2D))]
public class WiggleMovement : MonoBehaviour, I_Attackable
{

    [Header("������")]
    public float moveSpeed = 5; // ������ �ӵ�
    public float moveRadius = 2;    // ������ �� �ִ� �ݰ�
    public float rotationTime = 0.5f;   // ȸ�� �ð�

    [Header("����")]
    public float deathDuration = 2; // �״� �ð�
    public float fallingOutPower = 15; // �׾��� �� ���ư� ��

    public enum state { move, turn }
    private state currentState;

    bool isGoingUp; // �������� �����̴��� ����
    bool isDead;    // �׾����� ����

    Vector3 movePosUp;
    Vector3 movePosDown;
    Vector3 targetPos;

    Coroutine turnCoroutine;

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
        isDead = false;
    }


    void Update()
    {
        if (isDead) return;

        if (currentState == state.move) MoveHandler();
    }

    void SetState(state targetState)
    {
        switch (targetState)
        {
            case state.move:
                currentState = state.move;
                anim.SetBool("isMoving", true);
                break;

            case state.turn:
                currentState = state.turn;
                anim.SetBool("isMoving", false);
                StartTurn();
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
        Flip();
        turnCoroutine = null;
    }

    void Flip()
    {
        Vector3 currentScale = transform.localScale;
        currentScale.y *= -1f;
        transform.localScale = currentScale;
    }

    float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDead) OnTrigger(other);
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
        if (isDead) return;
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

    public bool CanAttack()
    {
        return true;
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
