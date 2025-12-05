using System.Collections;
using UnityEditor.Search;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Collider2D))]
public class Spencer_S3 : MonoBehaviour, I_Attackable
{
    [Header("위치")]
    public float centerX;

    [Header("공격")]
    public float warningTime = 1f;
    public SpencerWebProjectile projectile;
    public float projectileRadius = 2f;
    public float attackInterval = 0.05f;
    public int projectileCount = 25;


    Animator anim;
    Collider2D col;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    public void GotoCenter()
    {
        Vector2 pos = new Vector2(centerX, transform.position.y);
        transform.position = pos;
        SpencerManager.Instance.positionNumber = 2;
        col.enabled = false;

        StartCoroutine(SkillSequence());
    }

    IEnumerator SkillSequence()
    {
        yield return new WaitForSeconds(warningTime);
        anim.SetTrigger("attack");
        for (int i = 0; i < projectileCount; i++)
        {
            FiringProjectile();
            yield return new WaitForSeconds(attackInterval);
        }
        anim.SetTrigger("end");
    }

    void FiringProjectile()
    {
        Vector2 projectilePos = Vector2.zero;
        projectilePos = Random.insideUnitCircle.normalized;
        if (projectilePos.y < -0.4f)
        {
            while (projectilePos.y < -0.4f)
            {
                projectilePos = Random.insideUnitCircle.normalized;
            }
        }
        projectilePos *= projectileRadius;
        projectilePos += (Vector2)transform.position;

        SpencerWebProjectile proj = Instantiate(projectile, projectilePos, Quaternion.identity);
        proj.DontLookPos(transform.position);
    }

    public void EndAttack()
    {
        SpencerManager.Instance.canUseSklill = true;
        col.enabled = true;
    }

    public bool CanAttack(Transform attacker)
    {
        return true;
    }

    public void OnAttack(Transform attacker)
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }
}
