using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(BoxCollider2D))]
public class MsMovement : MonoBehaviour, I_Attackable
{
    [Header("������")]
    public float maxSpeed = 8; // �ִ� ������ �ӵ�
    public float moveRadius; // ��� ���¿� �� ��ġ�κ��� �ִ� Ž�� ����. �� ������ ������ ���� ������ �� ����.
    public float trunDuration = 0.5f;   // ȸ�� ��� �ð�
    public float acceleration = 2f; // ���ӵ�

    [Header("��, �Ӹ�")]
    public Transform headObj;
    public Transform armObj;

    [Header("���� ����")]
    public Transform wallCheckPos;  // ��
    public Transform groundCheckPos;    // ��

    [Header("����")]
    public Transform firePoint;
    public GameObject projectile;
    public float readyToAttackTime = 0.5f;  // ������ �غ� �ð� (�� ���, ������)
    public float aimingTime = 0.2f;          // ���� �ð� (���� �߻�)
    public float reloadTime = 0.5f;  // ������ �غ� �ð� (������)

    [Header("�����")]
    public float damage = 0.4f;  // ���� �����

    [Header("�þ� ����")]
    public Vector2 viewOffset = new Vector2(0f, 0.5f); // �þ� �߽��� ������
    public Vector2 viewSize = new Vector2(5f, 3f);     // �þ� ������ ����/���� ũ��

    [Header("���� ����")]
    public float attackRange;

    [Header("���̾�, ĳ��Ʈ")]
    public LayerMask obstacleMask;  // ��ֹ��� ������ ���̾�
    public LayerMask playerLayer;   // ���� ���̾�

    [Header("�÷��̾� ����")]
    public Transform exclamationMarkPos;    // ����ǥ ��ġ
    public GameObject exclamationMarkObj;    // ����ǥ ������
    public float detectionTime = 1.0f;  // �߰��� ���� �ð�
    public float detectionCancelTime = 0.5f;  // �ǽ� ���� �ð�
    public float detectionDecayTime = 2.0f; // �߰��� ���� �ð�

    [Header("����")]
    public float deathDuration = 2; // �״� �ð�
    public float fallingOutPower = 15; // �׾��� �� ���ư� ��

    private int facingSign = 1; // �ٶ󺸴� ����

    private float currentNormalizedSpeed = 0;   // ����ȭ�� �ӵ�
    private float layerCheckRadius = 0.05f;  // ���� ��ġ �ݰ�
    private float detectionRate = 0;    // �߰��� ����

    private bool isFacingRight = true;  // �������� �ٶ󺸴��� ����
    private bool canGoStraight = true;  // ���� ���� ���� (���� ���� ���� �־�� ��)
    private bool isMoving = false;  // �����̰� �ִ��� ����
    private bool isDead = false;    // �׾����� ����
    private bool isAiming = false;

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    public enum state { idle, doubt, track, attack, endAttack }
    state currentState;

    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D boxCol;
    private ExclamationMarkHandler exclamationMark;

    GameObject playerObject;    // �÷��̾� ������Ʈ

    void Awake()
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

        isFacingRight = true;
        SetState(state.idle);
    }

    void Update()
    {
        if (isDead) return;
        UpdateStates();

        switch (currentState)
        {
            case state.idle:
                if (IsPlayerInView())
                {
                    SetState(state.doubt);
                }

                break;

            case state.track:
                TrackHandler();

                break;
        }

        if (isAiming) RotatePartsToLookAtPlayer();
    }

    void SetState(state targetState)
    {
        StopAllCoroutines();
        currentState = targetState;

        rb.velocity = Vector3.zero;
        currentNormalizedSpeed = 0;

        if (targetState == state.idle)
        {
            movePosRight = movePosLeft = transform.position;
            movePosRight.x += moveRadius;
            movePosLeft.x -= moveRadius;

            targetPos = isFacingRight ? movePosRight : movePosLeft;

            StartCoroutine(IdleMovement());
        }
        else if (targetState == state.doubt)
        {
            exclamationMark = Instantiate(exclamationMarkObj).GetComponent<ExclamationMarkHandler>();
            exclamationMark.SetTargetPos(exclamationMarkPos);

            StartCoroutine(DoubtHandler());
        }
        else if (targetState == state.attack)
        {
            StartCoroutine(AttackHandler());
        }
        else if (targetState == state.endAttack)
        {
            StartCoroutine(EndAttack());
        }
    }

    IEnumerator IdleMovement()
    {
        while (true)
        {
            float sign = isFacingRight ? 1f : -1f;

            while (!HasArrived(targetPos) && canGoStraight)
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

    IEnumerator DoubtHandler()
    {
        float TimeSincePlayerLost = 0;
        while (true)
        {
            if (IsPlayerInView()) detectionRate += Time.deltaTime / detectionTime;
            else detectionRate -= Time.deltaTime / detectionDecayTime;

            detectionRate = Mathf.Clamp01(detectionRate);
            exclamationMark.SetGaugeValue(detectionRate);

            if (detectionRate == 1)
            {
                anim.SetTrigger("readyToAttack");
                yield return new WaitForSeconds(readyToAttackTime);

                SetState(state.track);
            }

            if (detectionRate == 0)
            {
                TimeSincePlayerLost += Time.deltaTime;
                if (TimeSincePlayerLost >= detectionCancelTime)
                {
                    Destroy(exclamationMark.gameObject);
                    SetState(state.idle);
                }
            }
            else
            {
                TimeSincePlayerLost = 0;
                LookPos(playerObject.transform.position);
            }


            yield return null;
        }
    }

    void TrackHandler()
    {
        LookPos(playerObject.transform.position);

        Vector2 checkPos = playerObject.transform.position;
        checkPos.y = transform.position.y;

        if (canGoStraight && !HasArrived(checkPos))
        {
            currentNormalizedSpeed = Mathf.Clamp(currentNormalizedSpeed + acceleration * Time.deltaTime, 0.505f, 1f);
            rb.velocity = new Vector2(facingSign * currentNormalizedSpeed * maxSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector3.zero;
            currentNormalizedSpeed = 0;
        }

        if (IsPlayerInRange())
        {
            SetState(state.attack);
            detectionRate = 1;
            exclamationMark.SetGaugeValue(detectionRate);
            return;
        }

        if (IsPlayerInView())
        {
            detectionRate += Time.deltaTime / detectionTime;
        }
        else
        {
            detectionRate -= Time.deltaTime / detectionDecayTime;
        }

        detectionRate = Mathf.Clamp01(detectionRate);
        exclamationMark.SetGaugeValue(detectionRate);

        if (detectionRate == 0)
        {
            SetState(state.endAttack);
            Destroy(exclamationMark.gameObject);
        }
    }

    IEnumerator AttackHandler()
    {
        while (true)
        {
            anim.SetTrigger("aiming");
            isAiming = true;
            yield return new WaitForSeconds(aimingTime);
            isAiming = false;
            anim.SetTrigger("fire");
            Attack();
            yield return new WaitForSeconds(reloadTime);

            if (!IsPlayerInRange()) break;
        }

        SetState(state.track);
    }

    void Attack()
    {
        EnemyProjectile ep = Instantiate(projectile, firePoint.position, Quaternion.identity).GetComponent<EnemyProjectile>();
        ep.SetTarget(playerObject.transform);
    }

    IEnumerator EndAttack()
    {
        anim.SetTrigger("endAttack");
        yield return new WaitForSeconds(readyToAttackTime);
        SetState(state.idle);
    }


    void RotatePartsToLookAtPlayer()
    {
        LookPos(playerObject.transform.position);

        float verticalProximity = GetVerticalProximity();
        float targetArmAngle = verticalProximity * 90;
        float targetHeadAngle = targetArmAngle * 0.5f;

        Quaternion armRotation = Quaternion.Euler(0, 0, targetArmAngle);
        armObj.localRotation = armRotation;

        Quaternion headRotation = Quaternion.Euler(0, 0, targetHeadAngle);
        headObj.localRotation = headRotation;
    }

    float GetVerticalProximity()
    {
        Vector3 direction = playerObject.transform.position - transform.position;
        Vector2 normalizedDirection = direction.normalized;
        float verticalComponent = normalizedDirection.y;

        return verticalComponent;
    }

    bool IsPlayerInView()
    {
        Vector2 localAdjustedOffset = new Vector2(viewOffset.x * facingSign, viewOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            worldCenter,            // �߽� ��ġ
            viewSize,             // ũ��
            0f,                     // ȸ�� ����
            playerLayer             // ������ ���̾�
        );

        bool isPlayerInView = false;

        if (hitTargets.Length > 0)
        {
            foreach (Collider2D targetCollider in hitTargets)
            {
                if (targetCollider.CompareTag("Player"))
                {
                    isPlayerInView = true;
                    break;
                }
            }
        }

        if (!isPlayerInView) return false;

        return CanSeePlayer();
    }

    bool IsPlayerInRange()
    {
        bool inRange = Vector3.Distance(transform.position, playerObject.transform.position) < attackRange;

        return CanSeePlayer() && inRange;
    }

    bool CanSeePlayer()
    {
        Vector2 startPos = transform.position;
        Vector2 endPos = playerObject.transform.position;
        Vector2 direction = (endPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, endPos);

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, distance, obstacleMask);
        if (hit.collider != null) return false;
        return true;
    }

    void SwitchPos()
    {
        Flip();

        targetPos = isFacingRight ? movePosRight : movePosLeft;
    }

    void LookPos(Vector2 targetPos)
    {
        float directionX = targetPos.x - transform.position.x;

        if (directionX != 0 && (directionX > 0) != isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        facingSign = isFacingRight ? 1 : -1;

        UpdateStates();
    }

    bool HasArrived(Vector3 pos)
    {
        float distance = Vector3.Distance(transform.position, pos);
        return distance <= 0.1f;
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
        if (exclamationMark != null) Destroy(exclamationMark.gameObject);

        anim.SetTrigger("die");
        StopAllCoroutines();
        StartCoroutine(Dead());
    }

    float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

    IEnumerator Dead()
    {
        float timer = 0f;
        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = Vector3.zero;
        boxCol.isTrigger = false;

        while (timer < deathDuration)
        {
            timer += Time.deltaTime;

            float t = timer / deathDuration;

            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;

        Vector2 viewLocalAdjustedOffset = new Vector2(viewOffset.x * facingSign, viewOffset.y);
        Vector2 viewGizmoCenter = (Vector2)transform.position + viewLocalAdjustedOffset;

        Gizmos.DrawWireCube(viewGizmoCenter, new Vector3(viewSize.x, viewSize.y, 0f));

        Gizmos.color = Color.cyan;

        if (Application.isPlaying)
        {
            if (currentState == state.idle)
            {
                Gizmos.DrawWireSphere(movePosRight, 0.25f);
                Gizmos.DrawWireSphere(movePosLeft, 0.25f);
                Gizmos.DrawLine(movePosRight, movePosLeft);
            }
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
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(exclamationMarkPos.position, 0.05f);
    }
}
