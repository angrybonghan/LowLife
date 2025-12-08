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

    [Header("소리")]
    public AudioClip deadSound;

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
        if (centerX <= transform.position.x)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        StartCoroutine(DeadpartSequence());

        SoundManager.instance.PlaySoundAtPosition(transform.position, deadSound);
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

    public void NextScene()
    {
        ScreenTransition.ScreenTransitionGoto("Swamp_Boss_EndCut", "nope", Color.black, 0, 0, 0f, 0.2f, 0);
    }
}
