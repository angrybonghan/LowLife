using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(BoxCollider2D))]
public class SlagMovement : MonoBehaviour
{
    [Header("������")]
    public float maxSpeed = 8; // �ִ� ������ �ӵ�
    public float moveRadius; // ��� ���¿� �� ��ġ�κ��� �ִ� Ž�� ����. �� ������ ������ ���� ������ �� ����.
    public float trunDuration = 0.5f;   // ȸ�� ��� �ð�
    public float acceleration = 2f;

    [Header("���̾�, ĳ��Ʈ")]
    public LayerMask obstacleMask;  // ��ֹ��� ������ ���̾�

    [Header("���� ����")]
    public Transform wallCheckPos;  // ��
    public Transform groundCheckPos;    // ��

    [Header("����")]
    public float attackChargeTime;  // ������ �غ� �ð�
    public float attackDuration;    // ������ ���� �ð�
    public float attackCooldown;    // ���� ���ð� (���� ��Ÿ��)

    [Header("��Ʈ�ڽ�")]
    public Vector2 hitboxOffset = Vector2.zero;    // ��Ʈ�ڽ� ������
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // ũ�� (width, height)
    public LayerMask targetLayer;   // ���� ���̾�

    [Header("����")]
    public float deathDuration = 2; // �״� �ð�
    public float fallingOutPower = 15; // �׾��� �� ���ư� ��

    private float currentNormalizedSpeed = 0;
    private float layerCheckRadius = 0.05f;  // ���� ��ġ �ݰ�

    private bool isFacingRight = true;
    private bool canGoStraight = true;
    private bool isMoving = false;

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    public enum state { idle, track, attack }
    state currentState;

    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D boxCol;

    GameObject playerObject;    // �÷��̾� ������Ʈ
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCol = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        playerObject = GameObject.FindWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("�÷��̾� ����");
        }

        SetState(state.idle);

        isFacingRight = true;

        movePosRight = movePosLeft = transform.position;
        movePosRight.x += moveRadius;
        movePosLeft.x -= moveRadius;

        targetPos = movePosRight;
    }

    void Update()
    {
        UpdateStates();

        //switch (currentState)
        //{

        //}
    }

    void SetState(state targetState)
    {
        StopAllCoroutines();
        currentState = targetState;

        if (targetState == state.idle)
        {
            StartCoroutine(IdleMovement());
        }
        else if (targetState == state.track)
        {
            
        }
        else if (targetState == state.attack)
        {
            
        }
    }

    void SwitchPos()
    {
        isFacingRight = !isFacingRight;

        if (isFacingRight)
        {
            targetPos = movePosRight;
        }
        else
        {
            targetPos = movePosLeft;
        }

        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1f;
        transform.localScale = currentScale;
    }

    void SetIdlePos()
    {

    }

    IEnumerator IdleMovement()
    {
        while (true)
        {
            float sign = isFacingRight ? 1f : -1f;

            while (!HasArrived() && canGoStraight)
            {
                currentNormalizedSpeed = Mathf.Min(currentNormalizedSpeed + acceleration * Time.deltaTime, 0.5f);
                rb.velocity = new Vector2(sign * currentNormalizedSpeed * maxSpeed, rb.velocity.y);
                yield return null;
            }
            rb.velocity = Vector3.zero;
            currentNormalizedSpeed = 0;

            yield return new WaitForSeconds(trunDuration);
            SwitchPos();
            yield return null;
        }
    }

    bool HasArrived()
    {
        float distance = Vector3.Distance(transform.position, targetPos);
        return distance <= 0.1f;
    }

    bool IsPlayerInRange()
    {
        float offsetX = hitboxOffset.x;
        if (!isFacingRight) offsetX *= -1;
        Vector2 localAdjustedOffset = new Vector2(offsetX, hitboxOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

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
                    return true;
                }
            }
        }

        return false;
    }

    void UpdateStates()
    {
        movePosRight.y = movePosLeft.y = targetPos.y = transform.position.y;

        bool isGrounded = Physics2D.OverlapCircle(groundCheckPos.position, layerCheckRadius, obstacleMask);
        bool isTouchingAnyWall = Physics2D.OverlapCircle(wallCheckPos.position, layerCheckRadius, obstacleMask);

        canGoStraight = isGrounded && !isTouchingAnyWall;

        isMoving = currentNormalizedSpeed > 0f;

        anim.SetBool("isMoving", isMoving);
        anim.SetFloat("moveSpeed", currentNormalizedSpeed);

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float offsetX = hitboxOffset.x;
        if (!isFacingRight) offsetX *= -1;
        Vector2 localAdjustedOffset = new Vector2(offsetX, hitboxOffset.y);
        Vector2 gizmoCenter = (Vector2)transform.position + localAdjustedOffset;

        Gizmos.DrawWireCube(gizmoCenter, new Vector3(hitboxSize.x, hitboxSize.y, 0f));

        Gizmos.color = Color.cyan;

        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(movePosRight, 0.25f);
            Gizmos.DrawWireSphere(movePosLeft, 0.25f);
            Gizmos.DrawLine(movePosRight, movePosLeft);
        }
        else
        {
            Vector3 gizmosMovePosRight = transform.position;
            Vector3 gizmosMovePosLeft = transform.position;
            gizmosMovePosRight.x += moveRadius;
            gizmosMovePosLeft.x -= moveRadius;

            Gizmos.DrawWireSphere(gizmosMovePosRight, 0.25f);
            Gizmos.DrawWireSphere(gizmosMovePosLeft, 0.25f);
            Gizmos.DrawLine(gizmosMovePosRight, gizmosMovePosLeft);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckPos.position, 0.05f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckPos.position, 0.05f);

    }

}
