using System.Collections;
using TreeEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CapsuleCollider2D))]
public class Maw_S1 : MonoBehaviour, I_MawSkill, I_Attackable
{
    [Header("ÀÌµ¿")]
    public Transform wallCheckPos;
    public LayerMask wallLayer;
    public float layerCheckRadius= 0.05f;
    public float verticalJumpPower = 16.0f;
    public float horizontalJumpPower = 16.0f;

    [Header("°ø°Ý")]
    public int bulletCount = 20;
    public int phaseCount = 1;
    public float fireRate = 0.05f;
    public float phaseInterval = 0.5f;

    [Header("ÃÑ¾Ë")]
    public float damage = 0.035f;
    public float knockBackPower = 0.3f;
    public float knockBackTime = 0.05f;
    public EnemyLaser bullet;

    [Header("°ø°Ý À§Ä¡")]
    public Transform firePoint;
    public bool isFacingRight { get; set; }

    [Header("ÅºÇÇ")]
    public Transform cartridgePos;
    public MawCartridge cartridge;

    [Header("°³¹ÌÁö¿Á ÆÄÃ÷")]
    public GameObject antlion;

    [Header("Áß¾Ó X")]
    public float centerX;

    [Header("¼Ò¸®")]
    public AudioClip[] jumpSound;
    public AudioClip[] landSound;
    public AudioClip minigunSpin;
    public AudioClip[] fire;

    bool canFire = false;
    bool aiming = false;
    const float rayDistance = 0.05f;

    Rigidbody2D rb;
    Animator anim;
    CapsuleCollider2D capsuleCol;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        capsuleCol = GetComponent<CapsuleCollider2D>();
    }

    void Start()
    {
        if (!isFacingRight)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        LookPos(centerX);

        StartCoroutine(SkillSequence());
    }

    private void LateUpdate()
    {
        if (aiming)
        {
            if (PlayerController.instance != null) RotateObjTo(antlion, PlayerController.instance.transform.position);
        }
    }

    IEnumerator SkillSequence()
    {
        yield return new WaitUntil(() => IsTouchingWall());
        SoundManager.instance.PlayRandomSoundAtPosition(transform.position, landSound);

        anim.SetTrigger("readyToAttack");
        anim.SetTrigger("fireReady");
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;

        yield return new WaitUntil(() => canFire);
        SoundManager.instance.PlayLoopSoundAtPosition(transform.position, minigunSpin, "Maw_minigunSpin");
        aiming = true;
        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < phaseCount; i++)
        {
            anim.SetTrigger("fire");
            for (int j = 0; j < bulletCount; j++)
            {
                Attack();
                CameraMovement.PositionShaking(0.2f, 0.05f, fireRate);
                yield return new WaitForSeconds(fireRate);
            }

            if (phaseCount - 1 != i)
            {
                anim.SetTrigger("fireReady");
                yield return new WaitForSeconds(1.5f);
                anim.SetTrigger("fire");
            }
        }
        aiming = false;
        
        anim.SetTrigger("attackEnd");
        SoundManager.instance.StopSound("Maw_minigunSpin");
        yield return new WaitUntil(() => IsGrounded());

        anim.SetTrigger("land");
        SoundManager.instance.PlayRandomSoundAtPosition(transform.position, landSound);
        MawManager.instance.canUseSklill = true;
    }

    bool IsTouchingWall()
    {
        return Physics2D.OverlapCircle(wallCheckPos.position, layerCheckRadius, wallLayer);
    }

    public void Jump()
    {
        int sigh = isFacingRight? 1 : -1;
        rb.velocity = new Vector2(horizontalJumpPower * sigh, verticalJumpPower);
        SoundManager.instance.PlayRandomSoundAtPosition(transform.position, jumpSound);
    }

    public void Drop()
    {
        Flip();

        rb.gravityScale = 3f;
        int sigh = isFacingRight ? 1 : -1;
        rb.velocity = new Vector2(3.5f * sigh, 0);
    }

    void Attack()
    {
        EnemyLaser bul = Instantiate(bullet, firePoint.position, Quaternion.identity);

        if (PlayerController.instance != null) bul.SetTarget(PlayerController.instance.transform);
        bul.SetDamage(damage, knockBackPower, knockBackTime);
        bul.laserDispersion = 2f;

        MawCartridge cart = Instantiate(cartridge, cartridgePos.position, Quaternion.identity);
        cart.flyToRight = isFacingRight;

        SoundManager.instance.PlayRandomSoundAtPosition(transform.position, fire);
    }

    public void Flip()
    {
        MawManager.instance.isFacingRight = isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    void RotateObjTo(GameObject obj, Vector2 pos)
    {
        Vector2 direction = pos - (Vector2)obj.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle + (isFacingRight ? 180 : 0));
        obj.transform.rotation = rotation;
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckPos.position, layerCheckRadius);
    }

    public void FireReady()
    {
        canFire = true;
    }

    bool IsGrounded()
    {
        Vector2 size = capsuleCol.size;
        Vector2 offset = capsuleCol.offset;
        float halfHeight = size.y / 2f;
        Vector2 rayOrigin = (Vector2)transform.position + offset - new Vector2(0f, halfHeight);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, wallLayer);

        return hit.collider != null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }

    void LookPos(float targetX)
    {
        float directionX = targetX - transform.position.x;

        if (directionX != 0 && (directionX > 0) != isFacingRight)
        {
            Flip();
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public bool CanAttack(Transform attacker)
    {
        return true;
    }

    public void OnAttack(Transform attacker)
    {
        MawManager.instance.TakeDamage();
    }
}
