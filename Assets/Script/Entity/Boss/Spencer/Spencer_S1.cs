using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Spencer_S1 : MonoBehaviour, I_Attackable
{
    [Header("공격 시간")]
    public float aimingTime = 1.0f;

    [Header("팔들")]
    public SpencerArm[] arms;

    Animator anim;
    bool endAttack = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        arms[0].parents = this;
    }


    void Start()
    {
        StartCoroutine(SkillSequence());
    }


    IEnumerator SkillSequence()
    {
        yield return new WaitForSeconds(aimingTime);
        foreach (SpencerArm arm in arms)
        {
            if (arm != null) arm.FireWeapon();
        }
        anim.enabled = true;
        anim.SetTrigger("fire");

        yield return new WaitUntil(() => endAttack);

        SpencerManager.Instance.canUseSklill = true;
    }

    public void StartMoveArms()
    {
        anim.enabled = false;
    }

    public void EndMoveArms()
    {
        anim.SetTrigger("endAttack");
        endAttack = true;
    }

    public void DeleteArms()
    {
        foreach (SpencerArm arm in arms)
        {
            if (arm == null) continue;

            Destroy(arm.gameObject);
        }
    }

    public bool CanAttack(Transform attacker)
    {
        return true;
    }

    public void OnAttack(Transform attacker)
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }
}
