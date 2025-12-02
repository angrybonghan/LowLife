using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(CapsuleCollider2D))]
public class Maw_S2 : MonoBehaviour, I_MawSkill, I_Attackable
{
    [Header("점프")]
    public int jumpCount = 3;
    public float jumpInterval = 0.75f;

    [Header("이동")]
    public LayerMask groundLayer;
    public float minVerticalJumpPower = 5f;
    public float maxVerticalJumpPower = 16f;
    public float minHorizontalJumpPower = 5f;
    public float maxHorizontalJumpPower = 16f;

    [Header("기절")]
    public float stunDuration = 2.0f;

    [Header("중앙 X")]
    public float centerX;

    public bool isFacingRight { get; set; }

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

        StartCoroutine(SkillSequence());
    }

    IEnumerator SkillSequence()
    {
        for (int i = 0; i < jumpCount; i++)
        {
            yield return Co_Jump();
        }

        anim.SetTrigger("stun");
        yield return new WaitForSeconds(stunDuration);
        anim.SetTrigger("endStun");
        MawManager.instance.canUseSklill = true;
    }

    IEnumerator Co_Jump()
    {
        LookPos(centerX);
        anim.SetTrigger("jump");
        yield return new WaitUntil(() => !IsFalling());
        yield return new WaitUntil(() => IsFalling());
        anim.SetTrigger("turnabout");
        yield return new WaitUntil(() => IsGrounded());
        anim.SetTrigger("land");
        CameraMovement.PositionShaking(0.3f, 0.05f, 0.3f);
        yield return new WaitForSeconds(jumpInterval);
    }

    public void Jump()
    {
        int sigh = isFacingRight ? 1 : -1;

        rb.velocity = new Vector2(
            GetRandom(minHorizontalJumpPower, maxHorizontalJumpPower) * sigh,
            GetRandom(minVerticalJumpPower, maxVerticalJumpPower
            ));
    }

    void LookPos(float targetX)
    {
        float directionX = targetX - transform.position.x;

        if (directionX != 0 && (directionX > 0) != isFacingRight)
        {
            Flip();
        }
    }

    bool IsFalling()
    {
        return rb.velocity.y <= 0.0f;
    }

    public void Flip()
    {
        MawManager.instance.isFacingRight = isFacingRight = !isFacingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }

    bool IsGrounded()
    {
        Vector2 size = capsuleCol.size;
        Vector2 offset = capsuleCol.offset;
        float halfHeight = size.y / 2f;
        Vector2 rayOrigin = (Vector2)transform.position + offset - new Vector2(0f, halfHeight);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance, groundLayer);

        return hit.collider != null;
    }

    float GetRandom(float a, float b)
    {
        if (a == b) return a;
        else if (a < b) return Random.Range(a, b);
        else return Random.Range(b, a);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
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
