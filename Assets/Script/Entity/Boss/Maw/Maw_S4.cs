using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Maw_S4 : MonoBehaviour, I_MawSkill
{
    [Header("점프 목표")]
    public float jumpPower = 25.0f;
    public float targetY;

    [Header("중앙 X")]
    public float centerX;

    [Header("혀")]
    public MawTongue tongue;
    public float aimingTime;

    public bool isFacingRight { get; set; }
    Vector2 fallDownPos;

    Animator anim;
    Rigidbody2D rb;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
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
        yield return new WaitUntil(() => transform.position.y >= targetY);

        transform.position = new Vector2(7777, 1234);

        

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

    public void FallDown()
    {
        transform.position = fallDownPos;
        LookPos(centerX);
        int sigh = isFacingRight ? 1 : -1;

        rb.gravityScale = 0f;
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
}
