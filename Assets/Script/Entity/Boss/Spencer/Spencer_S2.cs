using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Spencer_S2 : MonoBehaviour
{
    [Header("공격 페이즈")]
    public int phaseCount = 1;
    public float attackInterval = 0.5f;

    [Header("공격")]
    public float bulletInterval = 0.05f;
    public int bulletCount = 25;

    [Header("범위")]
    public SpencerAttackRange range;
    public float xRange = 7.5f;

    Animator anim;
    SpencerAttackRange attackRange;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
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
                attackRange.StartGunShot();
                yield return new WaitForSeconds(bulletInterval);
            }
            if (phaseCount - 1 != i)
            {
                float halfAttackInterval = attackInterval / 2;
                yield return new WaitForSeconds(halfAttackInterval);
                attackRange.ReloadPos();
                yield return new WaitForSeconds(halfAttackInterval);
            }

            SpencerManager.Instance.canUseSklill = true;
        }

        anim.SetTrigger("endAttack");
        attackRange.EndAttack();
    }

}
