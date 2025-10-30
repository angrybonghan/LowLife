using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class ShieldMovement : MonoBehaviour
{
    [Header("���� �� ����")]
    public float maxShieldFlightTime = 1;      // ���а� ���ư� �� �ִ� �ִ� �ð�
    public float throwSpeed = 25f;     // ���а� ���ư��� �ӵ�
    public float returnSpeed = 20f;    // ���а� �÷��̾�� ���ƿ��� �ӵ�
    public float catchDistance = 0.75f; // �÷��̾�� ������ �Ÿ�

    [Header("���̾�")]
    public LayerMask groundLayer;
    public LayerMask entityLayer;

    [Header("��ƼŬ")]
    public Transform particlePos;
    public GameObject groundHitParticle;
    public GameObject entityHitParticle_shape;
    public GameObject entityHitParticle_explod;

    [Header("�ܺ� ���� ����")]
    public Transform playerPostion;    // ���и� ���� �÷��̾� ��ġ
    public Vector3 throwDirection;  // ������ ����
    public bool isReturning = false; // ���� ���а� ���ƿ��� ���ΰ�?
    [Header("����Ʈ")]
    public GameObject afterEffect;
    public float afterEffectInterval;

    [Header("����")]
    public AudioClip[] hitSounds;

    private float currentFlightTime = 0; // ���� ���ư��� �ð� (�ð� ����)
    private float LastAfterEffect = 0;  // ������ �ܻ� �ð�  (�ð� ����)

    // ������ ����
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

        // �ʱ� ����
        circleCol.isTrigger = true;
        rb.gravityScale = 0f;
        isReturning = false;
        LastAfterEffect = Time.time;
    }

    void Update()
    {
        if (!isReturning)
        {
            rb.velocity = throwDirection * throwSpeed;
            float angle = Mathf.Atan2(throwDirection.y, throwDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

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
                SetReturnState(); // ���� ���·� ��ȯ
            }
        }
        else // �ǵ��ƿ��� ����
        {
            if (playerPostion != null)
            {
                Vector3 directionToPlayer = (playerPostion.position - transform.position).normalized;
                rb.velocity = directionToPlayer * returnSpeed;

                float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);

                // �÷��̾�� ����
                if (Vector2.Distance(playerPostion.position, transform.position) <= catchDistance)
                {
                    playerController.CatchShield();
                    Destroy(gameObject);
                }
            }
            else
            {
                // �÷��̾� ������ ������ٸ�, ���� �ı�
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// �μ� : ���� - �÷��̾� ��ũ��Ʈ
    /// </summary>
    public void InitializeThrow(Vector3 direction, PlayerController script)
    {
        throwDirection = direction.normalized;
        playerPostion = script.transform;
        playerController = script;
    }

    private void SetReturnState()
    {
        isReturning = true;
        rb.gravityScale = 0f;
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
                //��ƼƼ�̱� �ϳ� �������� ���ϴ� ��
            }
        }

        

        if (!isReturning)
        {
            // �浹�� ��ü�� ���̾ ��ġ�ϴ��� Ȯ��
            if (groundLayer == (groundLayer | (1 << other.gameObject.layer)))
            {
                Instantiate(groundHitParticle, particlePos.position, Quaternion.identity);
            }

            SetReturnState(); // ������ �浹 �� ���� ���·� ��ȯ
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