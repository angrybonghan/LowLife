using System.Collections;
using UnityEngine;

public class ImaginationVisualization : MonoBehaviour
{
    [Header("스프라이트 애니메이터 파츠")]
    public Sprite_Animator[] sprites;

    [Header("감지 범위")]
    public Vector2 hitboxOffset = Vector2.zero;    // 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask playerLayer;
    Vector2 boxCenter;

    [Header("시간")]
    public float imaginationTime = 1.5f;

    [Header("형태")]
    public float fadeTime = 0.5f;
    public float startAlpha = 0f;
    public float targetAlpha = 0.3f;

    bool hasFoundPlayer = false;

    private void Start()
    {
        boxCenter = (Vector2)transform.position + hitboxOffset;

        foreach (Sprite_Animator sprite in sprites)
        {
            sprite.SetAlpha(startAlpha, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (!hasFoundPlayer && IsPlayerInRange())
        {
            hasFoundPlayer = true;
            StartCoroutine(StartVisualization());
        }
    }

    IEnumerator StartVisualization()
    {
        yield return new WaitForSeconds(imaginationTime);

        foreach (Sprite_Animator sprite in sprites)
        {
            sprite.SetAlpha(targetAlpha, fadeTime);
        }

        this.enabled = false;
    }

    public bool IsPlayerInRange()
    {
        Collider2D hitCollider = Physics2D.OverlapBox(
            boxCenter,
            hitboxSize,
            0f,
            playerLayer
        );

        if (hitCollider != null && hitCollider.CompareTag("Player"))
        {
            return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 hitboxGizmoCenter = (Vector2)transform.position + hitboxOffset;
        Gizmos.DrawWireCube(hitboxGizmoCenter, hitboxSize);
    }
}
