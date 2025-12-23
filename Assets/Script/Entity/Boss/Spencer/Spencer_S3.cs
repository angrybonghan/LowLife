using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Collider2D))]
public class Spencer_S3 : MonoBehaviour, I_Attackable
{
    [Header("위치")]
    public float centerX;

    [Header("공격")]
    public float warningTime;
    public SpencerWebProjectile projectile;
    public float projectileRadius;
    public float attackInterval;
    public int projectileCount;

    [Header("소리")]
    public AudioClip attackSound;


    Animator anim;
    Collider2D col;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (SpencerManager.Instance.halfHP)
        {
            attackInterval = 0.01f;
            projectileCount = 60;
            warningTime = 0.4f;
        }
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
        col.enabled = true;
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

        float pitch = Random.Range(0.8f, 1.2f);
        AudioManager.Instance.Play3DSound(attackSound, transform.position, "attackSound", 1, pitch);
        CameraMovement.PositionShaking(0.1f, 0.05f, attackInterval);
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
        SpencerManager.Instance.TakeDamage();
    }

    public void PlayTeleportStartSound()
    {
        SpencerManager.Instance.PlayTeleportSound(true, transform.position);
    }

    public void PlayTeleportEndSound()
    {
        SpencerManager.Instance.PlayTeleportSound(false, transform.position);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }
}
