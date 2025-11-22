using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CircleCollider2D))]
public class BeadMovement : MonoBehaviour, I_Attackable
{
    [Header("회복할 방패 게이지")]
    public float shieldRecoveryAmount = 0.5f;
    public float orbMaxRecoveryAmount = 0.05f;

    [Header("죽음")]
    public float deathDuration = 2; // 죽는 시간
    public float fallingOutPower = 15; // 죽었을 때 날아갈 힘

    [Header("사망 후 제외 레이어")]
    public LayerMask afterDeathLayer;

    [Header("방패 회복 오브")]
    public ShieldRecoveryOrb shieldRecoveryOrb;


    bool isDead = false;  // 죽었는지 여부

    CircleCollider2D circleCol;
    Rigidbody2D rb;
    Animator anim;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        circleCol = GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();
    }

    public bool CanAttack()
    {
        return true;
    }

    public void OnAttack(Transform attackerTransform)
    {
        if (isDead) return;
        isDead = true;

        SummonOrb();

        Vector2 direction = (transform.position - attackerTransform.position).normalized;
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * fallingOutPower, ForceMode2D.Impulse);
        rb.freezeRotation = false;
        rb.gravityScale = 1f;
        rb.AddTorque(Random.Range(-20.0f,20.0f));

        circleCol.excludeLayers = afterDeathLayer;

        GameManager.SwitchLayerTo("Particle", gameObject);

        anim.SetTrigger("die");
        StopAllCoroutines();
        StartCoroutine(Dead());
    }

    void SummonOrb()
    {
        float currentShieldRecoveryAmount = shieldRecoveryAmount;

        while (currentShieldRecoveryAmount > 0)
        {
            if (currentShieldRecoveryAmount >= orbMaxRecoveryAmount)
            {
                ShieldRecoveryOrb newOrb = Instantiate(shieldRecoveryOrb, transform.position, Quaternion.identity);
                newOrb.shieldRecoveryAmount = 0.05f;
                currentShieldRecoveryAmount -= orbMaxRecoveryAmount;

            }
            else
            {
                ShieldRecoveryOrb newOrb = Instantiate(shieldRecoveryOrb, transform.position, Quaternion.identity);
                newOrb.shieldRecoveryAmount = currentShieldRecoveryAmount;
                currentShieldRecoveryAmount -= Mathf.Infinity;
            }
        }
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
