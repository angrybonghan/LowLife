using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CapsuleCollider2D))]
public class GrubMovement : MonoBehaviour
{
    [Header("파티클")]
    public GrubParticle particle;
    public GameObject stickOutParticle;
    public GameObject landParticle;

    [Header("움직임")]
    public float maxSpeed = 8; // 최대 움직임 속도
    public float moveRadius; // 대기 상태에 들어간 위치로부터 최대 탐색 범위. 이 범위는 지형에 따라 조절될 수 있음.
    public float trunDuration = 0.5f;   // 회전 대기 시간

    [Header("지형 감지")]
    public Transform wallCheckPos;  // 벽
    public Transform groundDetectPos;    // 땅
    public float layerCheckRadius = 0.05f;  // 감지 위치 반경

    [Header("공격")]
    public float attackChargeTime = 0.5f;  // 공격의 준비 시간
    public float attackSpeed = 12f;  // 공격할 때 수평으로 달려드는 힘
    public float jumpPower = 5f;  // 공격할 때 점프하는 힘

    [Header("히트박스")]
    public Vector2 hitboxOffset = Vector2.zero;    // 히트박스 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)

    [Header("레이어, 캐스트")]
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어
    public LayerMask playerLayer;   // 플레이어 감지 레이어

    [Header("죽음")]
    public float deathDuration = 2; // 죽는 시간
    public float fallingOutPower = 15; // 죽었을 때 날아갈 힘

    [Header("사망 후 제외 레이어")]
    public LayerMask afterDeathLayer;

    public enum state { idle, attack }
    state currentState;

    private int facingSign = 1; // 바라보는 방향

    private float rayDistance = 0;  // 레이캐스트 길이 (초기값으로 캐싱됨)
    private float fallingTime = 0;  // 추락의 지속 시간 (땅에서 0으로 고정)

    private bool isFacingRight = true;  // 오른쪽을 바라보는지 여부
    private bool canGoStraight = true;  // 직진 가능 여부 (벽이 없고 땅이 있어야 함)
    private bool isFalling = false; // 하강 중인지 여부

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    private Rigidbody2D rb;
    private Animator anim;
    private CapsuleCollider2D capsuleCol;
    private GrubBodyController bodyControll;

    GameObject playerObject;    // 플레이어 오브젝트

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        capsuleCol = GetComponent<CapsuleCollider2D>();
        bodyControll = GetComponent<GrubBodyController>();
    }

    void Start()
    {
        playerObject = PlayerController.instance.gameObject;

        rayDistance = Mathf.Max(hitboxOffset.x, hitboxOffset.y) + Mathf.Max(hitboxSize.x, hitboxSize.y);

        movePosRight = movePosLeft = transform.position;
        movePosRight.x += moveRadius;
        movePosLeft.x -= moveRadius;

        SetState(state.idle);
    }

    void Update()
    {
        UpdateStates();
        if (playerObject != null)
        {
            switch (currentState)
            {
                case state.idle:
                    if (IsPlayerInView() && IsPlayerInRange())
                    {
                        SetState(state.attack);
                    }

                    break;
            }
        }
    }

    void SetState(state targetState)
    {
        StopAllCoroutines();
        currentState = targetState;

        Vector2 originVelocity = rb.velocity;
        originVelocity.x = 0;
        rb.velocity = originVelocity;

        if (targetState == state.idle)
        {
            particle.ParticleToggle(true);

            targetPos = isRightCloser() ? movePosRight : movePosLeft;

            StartCoroutine(IdleMovement());
        }
        else if (targetState == state.attack)
        {
            particle.ParticleToggle(false);
            StartCoroutine(AttackMovement());
        }
    }

    IEnumerator IdleMovement()
    {
        while (true)
        {
            LookPos(targetPos);
            float sign = isFacingRight ? 1f : -1f;

            while (!HasArrived(targetPos) && canGoStraight)
            {
                rb.velocity = new Vector2(sign * maxSpeed, rb.velocity.y);
                yield return null;
            }
            Vector2 originVelocity = rb.velocity;
            originVelocity.x = 0;
            rb.velocity = originVelocity;

            if (isRightCloser() == isFacingRight)
            {
                yield return new WaitForSeconds(trunDuration);
            }
            SwitchPos();
            yield return null;
        }
    }

    IEnumerator AttackMovement()
    {
        anim.SetTrigger("readyToAttack");
        yield return new WaitForSeconds(attackChargeTime);
        bodyControll.EnableAttack(true);
        anim.SetTrigger("attack");

        int sign = isFacingRight ? 1 : -1;
        rb.velocity = new Vector2(attackSpeed * sign, jumpPower);
        UpdateStates();
        Instantiate(stickOutParticle, transform.position, Quaternion.identity);

        yield return new WaitUntil(() => isFalling);
        yield return new WaitUntil(() => IsBodyGrounded());

        Instantiate(landParticle, transform.position, Quaternion.identity);
        bodyControll.EnableAttack(false);
        anim.SetTrigger("landed");
        SetState(state.idle);
    }

    private bool IsBodyGrounded()
    {
        Vector2 point = new Vector2(capsuleCol.bounds.center.x, capsuleCol.bounds.min.y);
        RaycastHit2D hit = Physics2D.Raycast(point, Vector2.down, 0.05f, obstacleMask);
        return hit.collider != null;
    }

    void LookPos(Vector2 targetPos)
    {
        float directionX = targetPos.x - transform.position.x;

        if (directionX != 0 && (directionX > 0) != isFacingRight)
        {
            Flip();
        }
    }

    public bool isRightCloser()
    {
        float sqrDistanceToRight = (movePosRight - transform.position).sqrMagnitude;
        float sqrDistanceToLeft = (movePosLeft - transform.position).sqrMagnitude;

        return sqrDistanceToRight < sqrDistanceToLeft;
    }

    bool HasArrived(Vector3 pos)
    {
        float distance = Vector3.Distance(transform.position, pos);
        return distance <= 0.1f;
    }

    void SwitchPos()
    {
        targetPos = isRightCloser() ? movePosLeft : movePosRight;
        LookPos(targetPos);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);

        facingSign = isFacingRight ? 1 : -1;

        particle.Flip();

        UpdateStates();
    }

    bool IsPlayerInView()
    {
        if (Vector2.Distance(playerObject.transform.position, transform.position) > rayDistance)
        {
            return false;
        }

        Vector2 startPos = transform.position;
        Vector2 endPos = playerObject.transform.position;
        Vector2 direction = (endPos - startPos).normalized;

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, rayDistance, obstacleMask);
        return hit.collider == null;
    }

    bool IsPlayerInRange()
    {
        Vector2 localAdjustedOffset = new Vector2(hitboxOffset.x * facingSign, hitboxOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D hitTargets = Physics2D.OverlapBox(
            worldCenter,            // 중심 위치
            hitboxSize,             // 크기
            0f,                     // 회전 각도
            playerLayer             // 감지할 레이어
        );

        if (hitTargets != null)
        {
            if (hitTargets.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    void UpdateStates()
    {
        movePosRight.y = movePosLeft.y = targetPos.y = transform.position.y;

        bool groundDetect = Physics2D.OverlapCircle(groundDetectPos.position, layerCheckRadius, obstacleMask);
        bool isTouchingAnyWall = Physics2D.OverlapCircle(wallCheckPos.position, layerCheckRadius, obstacleMask);

        canGoStraight = groundDetect && !isTouchingAnyWall;

        isFalling = rb.velocity.y < 0;
        anim.SetBool("isFalling", isFalling);

        if (isFalling)
        {
            fallingTime += Time.deltaTime;

            if (fallingTime > 5)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            fallingTime = 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 hitboxLocalAdjustedOffset = new Vector2(hitboxOffset.x * facingSign, hitboxOffset.y);
        Vector2 hitboxGizmoCenter = (Vector2)transform.position + hitboxLocalAdjustedOffset;

        Gizmos.DrawWireCube(hitboxGizmoCenter, new Vector3(hitboxSize.x, hitboxSize.y, 0f));

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
        Gizmos.DrawWireSphere(groundDetectPos.position, 0.05f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckPos.position, 0.05f);
    }
}
