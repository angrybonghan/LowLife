using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CapsuleCollider2D))]
public class Maw_S4 : MonoBehaviour, I_MawSkill, I_Attackable
{
    [Header("Á¡ÇÁ ¸ñÇ¥")]
    public float jumpPower = 25.0f;
    public float targetY;

    [Header("Áß¾Ó X")]
    public float centerX;

    [Header("Çô")]
    public MawTongue tongue;
    public float aimingTime;
    public int phaseCount;

    [Header("¶¥ ·¹ÀÌ¾î")]
    public float underCeiling = 2f;
    public LayerMask groundLayer;

    public bool isFacingRight { get; set; }
    const float rayDistance = 0.05f;
    Vector2 fallDownPos;

    Animator anim;
    Rigidbody2D rb;
    CapsuleCollider2D capsuleCol;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        capsuleCol = GetComponent<CapsuleCollider2D>();
    }

    void Start()
    {
        isFacingRight = true;
        if (!isFacingRight)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        StartCoroutine(SkillSequence());
    }

    IEnumerator SkillSequence()
    {
        yield return new WaitUntil(() => transform.position.y >= targetY);

        transform.position = new Vector2(7777, 1234);

        MawTongue newTongue = Instantiate(tongue, transform.position, Quaternion.identity);
        newTongue.aimingTime = aimingTime;
        newTongue.phaseCount = phaseCount;

        yield return new WaitUntil(() => newTongue == null);
        FallDown();
        anim.SetTrigger("drop");

        yield return new WaitUntil(() => IsGrounded());
        anim.SetTrigger("land");
    }

    public void Jump()
    {
        LookPos(centerX);
        int sigh = isFacingRight ? 1 : -1;

        rb.gravityScale = 0f;
        rb.velocity = new Vector2(jumpPower * sigh, jumpPower * 2);

        fallDownPos.x = centerX;
        fallDownPos.x += transform.position.x >= centerX ? -5f : 5f;
        fallDownPos.y = targetY;
    }

    public void EndAttack()
    {
        MawManager.instance.canUseSklill = true;
    }

    void FallDown()
    {
        transform.position = fallDownPos;
        LookPos(centerX);
        int sigh = isFacingRight ? 1 : -1;

        rb.gravityScale = 1f;
        rb.velocity = new Vector2(jumpPower * sigh, jumpPower * -2);
    }

    void LookPos(float targetX)
    {
        float directionX = targetX - transform.position.x;

        if (directionX != 0 && (directionX > 0) != isFacingRight)
        {
            Flip();
        }
    }

    public void Flip()
    {
        MawManager.instance.isFacingRight = isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    public Transform GetTransform()
    {
        return transform;
    }

    bool IsGrounded()
    {
        if (transform.position.y > underCeiling) return false;

        Vector2 size = capsuleCol.size;
        Vector2 offset = capsuleCol.offset;
        float halfHeight = size.y / 2f;
        Vector2 rayOrigin = (Vector2)transform.position + offset - new Vector2(0f, halfHeight);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundLayer);

        return hit.collider != null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
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
