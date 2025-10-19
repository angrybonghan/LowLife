using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Animator))]
public class SpinyMovement : MonoBehaviour
{

    [Header("������")]
    public float moveSpeed = 5; // ������ �ӵ�
    public float moveRadius;    // ������ �� �ִ� �ݰ�
    public float rotationTime = 0.5f;   // ȸ�� �ð�

    [Header("����")]
    public float attackChargeTime;  // ������ �غ� �ð�
    public float attackDuration;    // ������ ���� �ð�
    public float attackCooldown;    // ���� ���ð� (���� ��Ÿ��)

    [Header("��Ʈ�ڽ�")]
    public Vector2 hitboxOffset = Vector2.zero;    // ��Ʈ�ڽ� ������
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // ũ�� (width, height)
    public LayerMask targetLayer;   // ���� ���̾�

    public enum state { move, turn , attack}
    private state currentState;

    bool isGoingUp; // �������� �����̴��� ����
    bool isMoving;  // �����̰� �ִ��� ����
    bool isAttacking; // �����ϰ� �ִ��� ����

    Vector3 movePosUp;
    Vector3 movePosDown;
    Vector3 targetPos;

    Coroutine TurnCoroutine;

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
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
    }


    void Update()
    {
        switch (currentState)
        {
            case state.move:
                MoveHandler();
                break;
            
            case state.turn:

                break;
        }

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
                Turn();
                break;
            case state.attack:
                currentState = state.attack;
                isMoving = false;
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

    void Turn()
    {
        isGoingUp = !isGoingUp;


        if (isGoingUp) targetPos = movePosUp;
        else targetPos = movePosDown;

        if (TurnCoroutine != null) StopCoroutine(TurnCoroutine);
        TurnCoroutine = StartCoroutine(WaitToTurn());
    }

    IEnumerator WaitToTurn()
    {
        anim.SetTrigger("trun");
        yield return new WaitForSeconds(rotationTime);

        SetState(state.move);
        UpdateAnimation();
        Flip();
        TurnCoroutine = null;
    }

    void Flip()
    {
        Vector3 currentScale = transform.localScale;
        currentScale.y *= -1f;
        transform.localScale = currentScale;
    }


    public bool GetRandomBool()
    {
        return Random.Range(0, 2) == 0;
    }

    private void UpdateAnimation()
    {
        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isAttacking", isAttacking);
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
}
