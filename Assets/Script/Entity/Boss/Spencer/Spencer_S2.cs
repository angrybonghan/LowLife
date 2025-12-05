using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Spencer_S2 : MonoBehaviour, I_Attackable
{
    [Header("공격 페이즈")]
    public int phaseCount;
    public float attackInterval;

    [Header("공격")]
    public float bulletInterval;
    public int bulletCount;

    [Header("범위")]
    public SpencerAttackRange range;
    public float xRange;

    Animator anim;
    SpencerAttackRange attackRange;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (SpencerManager.Instance.halfHP)
        {
            phaseCount = 2;
        }

        attackRange = Instantiate(range);
        attackRange.ReloadPos();
    }

    public void StartAttack()
    {
        StartCoroutine(SkillSequence());
    }

    IEnumerator SkillSequence()
    {
        
        for (int i = 0; i < phaseCount; i++)
        {
            float armNumber = Random.value;
            for (int j = 0; j < bulletCount; j++)
            {
                armNumber += Random.value * 0.8f;
                if (armNumber > 1) armNumber -= 1;

                anim.SetFloat("attackArmNumber", armNumber);
                anim.SetTrigger("attack");
                if (attackRange != null) attackRange.StartGunShot();
                yield return new WaitForSeconds(bulletInterval);
            }
            if (phaseCount - 1 != i)
            {
                float halfAttackInterval = attackInterval / 2;
                yield return new WaitForSeconds(halfAttackInterval);
                if (attackRange != null) attackRange.ReloadPos();
                yield return new WaitForSeconds(halfAttackInterval);
            }
        }

        SpencerManager.Instance.canUseSklill = true;
        anim.SetTrigger("endAttack");
        attackRange.EndAttack();
    }

    public bool CanAttack(Transform attacker)
    {
        return true;
    }

    public void OnAttack(Transform attacker)
    {
        SpencerManager.Instance.TakeDamage();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }

}
