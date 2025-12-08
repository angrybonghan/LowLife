using System.Collections;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CircleCollider2D))]
public class RashMovement : MonoBehaviour, I_Attackable
{
    [Header("적 이름")]
    public string enemyType;

    [Header("공격")]
    public float attackRange = 5; // 공격의 범위
    public float dashChrgeTime = 5; // 공격의 충전 시간
    public float dashSpeed = 5; // 공격의 속도

    [Header("레이캐스트")]
    public LayerMask targetCollisionMask; // 충돌을 검사할 레이어 마스크
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어 마스크

    [Header("대미지")]
    public float damage = 0.24f;
    public float knockbackPower = 1f;
    public float knockbacktime = 0.05f;

    [Header("시그널")]
    public float signalRange = 5;   // 신호의 범위
    public float maxSignalCount = 1;    // 신호를 보낼 수 있는 최대 개체 수
    public bool hasSignal = false;  // 이미 신호를 받았는지 여부 (시작 시 false로 고정됨)
    public float signalInputTime = 0.2f; // 신호를 받았을 때 대기 시간

    [Header("사망 후 제외 레이어")]
    public LayerMask afterDeathLayer;

    [Header("죽음")]
    public Transform selfAttackPos;
    public float deathDuration = 2; // 죽는 시간
    public float fallingOutPower = 15; // 죽었을 때 날아갈 힘

    private bool isFacingRight = true;  // 오른쪽을 바라보는지 여부
    private bool isDead = false;    // 죽었는지 여부

    public enum state { idle, readyToRush, rush }
    state currentState;

    Vector3 directionToPlayer;

    CircleCollider2D CirCol;
    Rigidbody2D rb;
    Animator anim;
    GameObject playerObject;

    private void Awake()
    {
        CirCol = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        playerObject = PlayerController.instance.gameObject;
        isFacingRight = true;
        hasSignal = false;
        currentState = state.idle;

        if (Random.value > 0.5f) Flip();

        StartCoroutine(Co_RashMovement());
    }

    private void Update()
    {
        if (isDead) return;

        switch (currentState)
        {
            case state.readyToRush:
                LookAtPlayer();
                break;

            case state.rush:
                RushMoveAndCollide();
                break;
        }
    }

    void RushMoveAndCollide()
    {
        Vector3 movement = directionToPlayer * dashSpeed * Time.deltaTime;

        float distance = movement.magnitude;
        float radius = CirCol.radius * Mathf.Abs(transform.localScale.x);

        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            radius,
            movement.normalized,
            distance,
            targetCollisionMask
        );

        if (hit.collider != null)
        {
            transform.position = (Vector3)hit.point - movement.normalized * radius;
            rb.velocity = Vector2.zero;
            ExecuteAttackLogic(hit.collider);
        }
        else
        {
            transform.position += movement;
        }
    }

    IEnumerator Co_RashMovement()
    {
        yield return new WaitUntil(() => (IsPlayerInRange() && CanSeePlayer()) || hasSignal);
        currentState = state.readyToRush;
        if (hasSignal) yield return new WaitForSeconds(signalInputTime);
        hasSignal = true;
        SendSignal();
        anim.SetTrigger("readyToRush");
        yield return new WaitForSeconds(dashChrgeTime);

        if (playerObject != null) directionToPlayer = playerObject.transform.position - transform.position;
        directionToPlayer = directionToPlayer.normalized;

        currentState = state.rush;
        anim.SetTrigger("rush");
    }

    void SendSignal()
    {
        var allRashes = FindObjectsOfType<RashMovement>();

        var potentialTargets = allRashes
            .Where(rash => rash != this && rash.hasSignal == false)
            .Select(rash => new
            {
                Target = rash,
                Distance = Vector3.Distance(transform.position, rash.transform.position)
            })
            .ToList();

        var sortedTargets = potentialTargets.OrderBy(t => t.Distance);
        var closestTargets = sortedTargets.Take((int)maxSignalCount);

        foreach (var target in closestTargets)
        {
            if (target.Distance < signalRange)
            {
                target.Target.SignalInput();
            }
        }
    }

    public void SignalInput()
    {
        hasSignal = true;
    }

    bool IsPlayerInRange()
    {
        if (playerObject == null) return false;
        return Vector3.Distance(transform.position, playerObject.transform.position) < attackRange;
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

    void LookAtPlayer()
    {
        if (playerObject == null) return;

        LookPos(playerObject.transform.position);

        float targetAngle = GetVerticalProximity() * 90;
        targetAngle *= isFacingRight ? 1 : -1;
        Quaternion Rotation = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Rotation;
    }

    float GetVerticalProximity()
    {
        if (playerObject == null) return 0f;

        Vector3 direction = playerObject.transform.position - transform.position;
        Vector2 normalizedDirection = direction.normalized;
        float verticalComponent = normalizedDirection.y;

        return verticalComponent;
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
    }

    void ExecuteAttackLogic(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController pc = collision.GetComponent<PlayerController>();
            if (pc.IsParried(transform))
            {
                OnAttack(playerObject.transform);
                return;
            }
            else
            {
                pc.OnAttack(damage, knockbackPower, knockbacktime, transform);
                OnAttack(selfAttackPos);
            }
        }
        else
        {
            OnAttack(selfAttackPos);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isDead || currentState == state.rush) return;

        ExecuteAttackLogic(collision);
    }

    public bool CanAttack(Transform attackerPos)
    {
        return true;
    }

    public void OnAttack(Transform attackerTransform)
    {
        if (isDead) return;
        isDead = true;

        SoundManager.instance.PlayEntityHitSound(transform.position);

        Vector2 direction = (transform.position - attackerTransform.position).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * fallingOutPower, ForceMode2D.Impulse);
        rb.freezeRotation = false;
        rb.gravityScale = 1f;
        rb.AddTorque(GetRandom(-20, 20));

        CirCol.isTrigger = false;
        CirCol.excludeLayers = afterDeathLayer;

        GameManager.SwitchLayerTo("Particle", gameObject);

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

        while (timer < deathDuration)
        {
            timer += Time.deltaTime;

            float t = timer / deathDuration;

            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            yield return null;
        }

        AchievementManager.Instance?.OnEnemyKilled(enemyType);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, signalRange);

    }
}