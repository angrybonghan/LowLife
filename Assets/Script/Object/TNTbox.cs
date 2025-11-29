using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TNTbox : MonoBehaviour, I_Attackable
{
    [Header("공격")]
    public float readyToExplosionTime = 0.5f;  // 폭발의 준비 시간 (이후 폭발함)
    public float damage = 1f;   // 대미지
    public float knockbackPower = 1f;
    public float knockbacktime = 0.1f;

    [Header("범위")]
    public float explosionRadius = 2.0f;    // 폭발의 범위

    [Header("레이어")]
    public LayerMask attackLayer;   // 공격 레이어

    [Header("데드파츠 부품")]
    public GameObject deadParts;

    [Header("소리")]
    public AudioClip beepSound;
    public AudioClip[] explosionSound;

    bool isExploding = false;   // 폭발 중인지 여부
    bool wasHitPlayer = false; // 플레이어가 한 번 피격되었는지 여부

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public bool CanAttack(Transform whatColorIsYourBugatti)
    {
        if (!isExploding) StartExplosion();

        return false;
    }

    public void OnAttack(Transform attackerTransform)
    {
        if (!isExploding) StartExplosion();
    }

    void StartExplosion()
    {
        isExploding = true;
        GameManager.SwitchLayerTo("Particle", gameObject);
        StartCoroutine(Explosion());
    }
    
    IEnumerator Explosion()
    {
        anim.SetTrigger("explosionPreparation");
        yield return new WaitForSeconds(readyToExplosionTime);
        anim.SetTrigger("explosion");
        Instantiate(deadParts, transform.position, transform.rotation);
        ExplosionDamage();
    }

    void ExplosionDamage()
    {
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(
            transform.position,                 // 중심 위치
            explosionRadius,                    // 반경
            attackLayer                // 감지할 레이어 마스크
        );

        foreach (Collider2D other in hitTargets)
        {
            if (!wasHitPlayer)
            {
                if (other.CompareTag("Player"))
                {
                    PlayerController pc = other.GetComponent<PlayerController>();

                    if (pc != null)
                    {
                        pc.OnAttack(damage, knockbackPower, knockbacktime, transform);
                        wasHitPlayer = true;
                    }
                    continue;
                }
            }
            
            I_Attackable attackableTarget = other.GetComponent<I_Attackable>();

            if (attackableTarget != null && !other.CompareTag("Player"))
            {
                attackableTarget.OnAttack(transform);
            }
        }
    }

    public void PlayExplosionSound()
    {
        SoundManager.instance.PlayRandomSoundAtPosition(transform.position, explosionSound);
    }

    public void PlayBeepSound()
    {
        SoundManager.instance.PlaySoundAtPosition(transform.position, beepSound);
    }

    public void ExplosionEnd()
    {
        Destroy(gameObject);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
