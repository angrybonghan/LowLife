using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(CapsuleCollider2D))]
public class Maw_Deadparts : MonoBehaviour
{
    [Header("적 이름")]
    public string enemyType;

    [Header("중앙 X")]
    public float centerX;

    [Header("레이어/캐스트")]
    public LayerMask groundLayer;

    const float rayDistance = 0.05f;

    Animator anim;
    CapsuleCollider2D capsuleCol;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        capsuleCol = GetComponent<CapsuleCollider2D>();
    }

    void Start()
    {
        StartCoroutine(DeadpartSequence());
    }


    IEnumerator DeadpartSequence()
    {
        yield return new WaitUntil(() => IsGrounded());

        AchievementManager.Instance?.OnEnemyKilled(enemyType);
        anim.SetTrigger("land");
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
}
