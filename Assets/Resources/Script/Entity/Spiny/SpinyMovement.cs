using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Animator), typeof(Rigidbody2D))]
public class SpinyMovement : MonoBehaviour, I_Attackable
{

    [Header("움직임")]
    public float moveSpeed = 5; // 움직임 속도
    public float moveRadius;    // 움직일 수 있는 반경
    public float rotationTime = 0.5f;   // 회전 시간

    [Header("공격")]
    public float attackChargeTime;  // 공격의 준비 시간
    public float attackDuration;    // 공격의 유지 시간
    public float attackOutTime;    // 공격 이후 다시 움직일 시간
    public float attackCooldown;    // 공격 대기시간 (공격 쿨타임)

    [Header("히트박스")]
    public Vector2 hitboxOffset = Vector2.zero;    // 히트박스 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask targetLayer;   // 감지 레이어

    [Header("죽음")]
    public float deathDuration; // 죽는 시간
    public float fallingOutPower; // 죽었을 때 날아갈 힘

    public enum state { move, turn , attack}
    private state currentState;

    bool isGoingUp; // 위쪽으로 움직이는지 여부
    bool isMoving;  // 움직이고 있는지 여부
    bool isAttacking; // 공격하고 있는지 여부
    bool isDead;    // 죽었는지 여부

    float currentAttackCooldown;    // 현재 공격 대기시간 (쿨타임 계산용)

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
                currentAttackCooldown = 0;
                Attack();
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

    void Turn()
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

    void Attack()
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

        Vector2 direction = (transform.position - attackerTransform.position).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * fallingOutPower, ForceMode2D.Impulse);
        rb.freezeRotation = false;
        rb.gravityScale = 1f;
        boxCol.isTrigger = false;
        rb.AddTorque(GetRandom(-20, 20));

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
