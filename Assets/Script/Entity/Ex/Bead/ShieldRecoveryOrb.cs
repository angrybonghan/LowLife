using System.Collections;
using UnityEngine;

public class ShieldRecoveryOrb : MonoBehaviour
{
    [Header("속도")]
    public float moveSpeed = 50f;
    public float trackingPower = 3f;

    [Header("초당 속도 증가량")]
    public float speedIncrease = 50f;
    public float trackingIncrease = 10f;

    [Header("회복값")]
    public float shieldRecoveryAmount = 0.1f;


    [Header("레이어, 캐스트")]
    public LayerMask playerLayer;   // 감지 레이어

    [Header("색상")]
    public bool canChangeColor = true;
    public Color minColor = Color.black;
    public Color maxColor = Color.black;

    [Header("크기")]
    public float orbSize = 1.0f;
    public float maxSizeMultiples = 1.1f;
    public float minSizeMultiples = 0.9f;

    [Header("잔상")]
    public AfterimagePlayer afterimage;

    [Header("소리")]
    public AudioClip gainSound;

    private SpriteRenderer sr;
    private Transform playerPostion;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        playerPostion = PlayerController.instance.transform;

        if (canChangeColor) SetRandomColor();
        SetRandomSize();
        SetRandomRotate();
    }

    void Update()
    {
        if (playerPostion != null)
        {
            UpdatePower();
            RotateTowardsTarget();
            MoveAndCollide();
            AfterimageHandler();
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    void UpdatePower()
    {
        moveSpeed += speedIncrease * Time.deltaTime;
        trackingPower += trackingIncrease * Time.deltaTime;
    }


    void SetRandomColor()
    {
        float lerpValue = Random.value;
        Color randomColor = Color.Lerp(minColor, maxColor, lerpValue);
        sr.color = randomColor;
    }

    void SetRandomSize()
    {
        float multiple = GetRandom(minSizeMultiples, maxSizeMultiples);
        transform.localScale = transform.localScale * multiple;
        orbSize *= multiple;
    }

    void SetRandomRotate()
    {
        float randomAngle = GetRandom(0f, 360f);
        Quaternion randomRotation = Quaternion.Euler(0f, 0f, randomAngle);
        transform.rotation = randomRotation;
    }

    float GetRandom(float min, float max)
    {
        if (max == min) return max;
        else if (min < max) return Random.Range(min, max);
        else return Random.Range(max, min);
    }

    void RotateTowardsTarget()
    {
        Vector2 directionToTarget = (Vector2)(playerPostion.position - transform.position);
        float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        float t = Mathf.Clamp01(trackingPower * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
    }

    void MoveAndCollide()
    {
        Vector3 targetDirection = (transform.right * 999 - transform.position).normalized;
        Vector3 movement = targetDirection * moveSpeed * Time.deltaTime;
        float distance = movement.magnitude;

        if (distance <= 0) return;

        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            orbSize,
            movement.normalized,
            distance,
            playerLayer
        );

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                PlayerController.instance.AddDamageToShield(-shieldRecoveryAmount);
                float randomPitch = Random.Range(0.5f, 1.2f);
                SoundManager.instance.PlaySoundAtPositionWithPitch(transform.position, gainSound, randomPitch);
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"플레이어 태그를 가졌지만 스크립트가 없는 대상: {hit.collider.gameObject.name}");
            }
        }
        else
        {
            transform.position += movement;
        }
    }

    void AfterimageHandler()
    {
        AfterimagePlayer newAfterimage = Instantiate(afterimage, transform.position, Quaternion.identity);
        newAfterimage.SetColor(sr.color);
        newAfterimage.SetSize(transform.localScale);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, orbSize);
    }
}
