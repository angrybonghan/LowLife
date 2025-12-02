using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D), typeof(Animator), typeof(AudioSource))]
public class ShieldMovement : MonoBehaviour
{
    [Header("참조 및 설정")]
    public float maxShieldFlightTime = 1;      // 방패가 날아갈 수 있는 최대 시간
    public float throwSpeed = 25f;     // 방패가 날아가는 속도
    public float returnSpeed = 20f;    // 방패가 플레이어에게 돌아오는 속도
    public float catchDistance = 0.75f; // 플레이어에게 잡혀질 거리

    [Header("크기")]
    public float shieldSize = 0.35f;

    [Header("레이어")]
    public LayerMask groundLayer;
    public LayerMask returnCollisionMask;
    public LayerMask throwCollisionMask;

    [Header("파티클")]
    public Transform particlePos;
    public GameObject groundHitParticle;
    public GameObject entityHitParticle_shape;
    public GameObject entityHitParticle_explod;

    [Header("외부 조작 설정")]
    public Transform playerPostion;    // 방패를 던진 플레이어 위치
    public Vector3 throwDirection;  // 던지는 방향
    public bool isReturning = false; // 현재 방패가 돌아오는 중인가?

    [Header("잔상 이펙트")]
    public GameObject afterEffect;
    public float afterEffectInterval;

    [Header("시간")]
    public bool controlTimeAtAttack = true;

    private float currentFlightTime = 0; // 현재 날아가는 시간 (시간 계산용)
    private float LastAfterEffect = 0;  // 마지막 잔상 시간  (시간 계산용)
    private float castRadius;

    Vector2 lastHitPos;

    // 참조용 변수
    private CircleCollider2D circleCol;
    private Animator anim;
    private PlayerController playerController;

    List<Collider2D> alreadyHitTargets = new List<Collider2D>();

    public static ShieldMovement shieldInstance { get; private set; }

    private void Awake()
    {
        if (shieldInstance == null)
        {
            shieldInstance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        circleCol = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        
        // 초기 설정
        circleCol.isTrigger = true;
        isReturning = false;
        LastAfterEffect = Time.time;
        castRadius = circleCol.radius;

        playerController = PlayerController.instance;
        throwDirection = (CrosshairController.instance.transform.position - transform.position).normalized;
        playerPostion = playerController.transform;
    }

    private void Start()
    {
        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        if (playerPostion == null)
        {
            shieldInstance = null;
            Destroy(gameObject);
            return;
        }

        MoveAndCollide();

        if (!isReturning)
        {
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
                SetReturnState();
            }
        }
        else
        {
            if (Vector2.Distance(playerPostion.position, transform.position) <= catchDistance)
            {
                if (playerController != null)
                {
                    playerController.CatchShield();
                }
                Destroy(gameObject);
            }
        }
    }

    private void MoveAndCollide()
    {
        Vector3 targetDirection;
        float currentSpeed;

        if (isReturning)
        {
            targetDirection = (playerPostion.position - transform.position).normalized;
            currentSpeed = returnSpeed;
        }
        else
        {
            targetDirection = throwDirection;
            currentSpeed = throwSpeed;
        }

        Vector3 movement = targetDirection * currentSpeed * Time.deltaTime;
        float distance = movement.magnitude;

        if (distance <= 0) return;

        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            castRadius,
            movement.normalized,
            distance,
            isReturning ? returnCollisionMask : throwCollisionMask
        );

        lastHitPos = hit.point;

        if (hit.collider != null)
        {
            if (alreadyHitTargets.Contains(hit.collider))
            {
                transform.position += movement;
                return;
            }

            if (hit.collider.TryGetComponent<I_Attackable>(out I_Attackable targetAttackable))
            {
                if (targetAttackable.CanAttack(transform))
                {
                    transform.position = (Vector3)hit.point - (Vector3)movement.normalized * castRadius;
                    ExecuteCollisionLogic(hit.collider);
                }
                else
                {
                    transform.position += movement;
                }
            }
            else
            {
                transform.position = (Vector3)hit.point - (Vector3)movement.normalized * castRadius;
                ExecuteCollisionLogic(hit.collider);
            }

            SetReturnState();
        }
        else
        {
            transform.position += movement;
        }
    }

    private void ExecuteCollisionLogic(Collider2D other)
    {
        if (other.TryGetComponent<I_Attackable>(out I_Attackable targetAttackable))
        {
            targetAttackable.OnAttack(transform);

            Instantiate(entityHitParticle_shape, lastHitPos, Quaternion.identity);
            Instantiate(entityHitParticle_explod, lastHitPos, Quaternion.identity);
            if (controlTimeAtAttack) TimeManager.StartTimedSlowMotion(0.2f, 0.2f);
            CameraMovement.PositionShaking(1f, 0.05f, 0.2f);

            if (!alreadyHitTargets.Contains(other))
            {
                alreadyHitTargets.Add(other);
            }
        }
        else if (other.TryGetComponent<I_Destructible>(out I_Destructible targetDestructible))
        {
            if (targetDestructible.CanDestructible())
            {
                targetDestructible.OnAttack();

                Instantiate(entityHitParticle_explod, lastHitPos, Quaternion.identity);
                CameraMovement.PositionShaking(0.5f, 0.05f, 0.15f);
            }
        }
        else if (other.TryGetComponent<I_Projectile>(out I_Projectile targetProjectile))
        {
            targetProjectile.Collision();
        }
        else if (groundLayer == (groundLayer | (1 << other.gameObject.layer)))
        {
            Instantiate(groundHitParticle, lastHitPos, Quaternion.identity);
        }
    }

    private void SetReturnState()
    {
        if (playerPostion == null)
        {
            Destroy(gameObject);
        }
        else
        {
            isReturning = true;
        }
    }

    private void OnDestroy()
    {
        shieldInstance = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shieldSize);
    }
}