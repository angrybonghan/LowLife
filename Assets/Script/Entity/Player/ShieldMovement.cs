using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class ShieldMovement : MonoBehaviour
{
    [Header("참조 및 설정")]
    public float maxShieldFlightTime = 1;      // 방패가 날아갈 수 있는 최대 시간
    public float throwSpeed = 25f;     // 방패가 날아가는 속도
    public float returnSpeed = 20f;    // 방패가 플레이어에게 돌아오는 속도
    public float catchDistance = 0.75f; // 플레이어에게 잡혀질 거리

    [Header("레이어")]
    public LayerMask groundLayer;
    public LayerMask entityLayer;

    [Header("파티클")]
    public Transform particlePos;
    public GameObject groundHitParticle;
    public GameObject entityHitParticle_shape;
    public GameObject entityHitParticle_explod;

    [Header("외부 조작 설정")]
    public Transform playerPostion;    // 방패를 던진 플레이어 위치
    public Vector3 throwDirection;  // 던지는 방향
    public bool isReturning = false; // 현재 방패가 돌아오는 중인가?
    [Header("이펙트")]
    public GameObject afterEffect;
    public float afterEffectInterval;

    [Header("사운드")]
    public AudioClip[] hitSounds;

    private float currentFlightTime = 0; // 현재 날아가는 시간 (시간 계산용)
    private float LastAfterEffect = 0;  // 마지막 잔상 시간  (시간 계산용)

    // 참조용 변수
    private Rigidbody2D rb;
    private CircleCollider2D circleCol;
    private Animator anim;
    private PlayerController playerController;
    private AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCol = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // 초기 설정
        circleCol.isTrigger = true;
        rb.gravityScale = 0f;
        isReturning = false;
        LastAfterEffect = Time.time;
    }

    private void Start()
    {
        rb.velocity = throwDirection * throwSpeed;
        float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
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
                SetReturnState(); // 복귀 상태로 전환
            }
        }
        else // 되돌아오는 상태
        {
            if (Vector2.Distance(playerPostion.position, transform.position) <= catchDistance)
            {
                playerController.CatchShield();
                Destroy(gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        if (isReturning)
        {
            Vector3 directionToPlayer = (playerPostion.position - transform.position).normalized;
            rb.velocity = directionToPlayer * returnSpeed;

            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    /// <summary>
    /// 인수 : 방향 - 플레이어 스크립트
    /// </summary>
    public void InitializeThrow(Vector3 direction, PlayerController script)
    {
        throwDirection = direction.normalized;
        playerPostion = script.transform;
        playerController = script;
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
            rb.gravityScale = 0f;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<I_Attackable>(out I_Attackable targetAttackable))
        {
            if (targetAttackable.CanAttack())
            {
                targetAttackable.OnAttack(transform);

                Instantiate(entityHitParticle_shape, transform.position, Quaternion.identity);
                Instantiate(entityHitParticle_explod, transform.position, Quaternion.identity);
                PlayRandomHitSound();
                TimeManager.StartTimedSlowMotion(0.2f, 0.2f);
                CameraMovement.PositionShaking(1f, 0.05f, 0.2f);
            }
            else
            {
                //엔티티이긴 하나 공격하지 못하는 적
            }
        }

        if (other.TryGetComponent<I_Destructible>(out I_Destructible targetDestructible))
        {
            if (targetDestructible.CanDestructible())
            {
                targetDestructible.OnAttack();

                Instantiate(entityHitParticle_explod, transform.position, Quaternion.identity);
                CameraMovement.PositionShaking(0.5f, 0.05f, 0.15f);
            }
        }



        if (!isReturning)
        {
            // 충돌한 개체의 레이어가 일치하는지 확인
            if (groundLayer == (groundLayer | (1 << other.gameObject.layer)))
            {
                Instantiate(groundHitParticle, particlePos.position, Quaternion.identity);
            }

            SetReturnState(); // 뭔가에 충돌 시 복귀 상태로 전환
        }
    }

    private void PlayRandomHitSound()
    {
        if (hitSounds == null || hitSounds.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, hitSounds.Length);
        AudioSource.PlayClipAtPoint(hitSounds[randomIndex], transform.position, 2f);
    }
}