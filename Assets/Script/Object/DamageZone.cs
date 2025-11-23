using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [Header("히트박스")]
    public Vector2 hitboxOffset = Vector2.zero;    // 히트박스 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)

    [Header("레이어, 캐스트")]
    public LayerMask playerLayer;   // 감지 레이어

    private void FixedUpdate()
    {
        Damage();
    }

    void Damage()
    {
        Vector2 localAdjustedOffset = new Vector2(hitboxOffset.x , hitboxOffset.y);
        Vector2 worldCenter = (Vector2)transform.position + localAdjustedOffset;

        Collider2D hitTargets = Physics2D.OverlapBox(
            worldCenter,            // 중심 위치
            hitboxSize,             // 크기
            0f,                     // 회전 각도
            playerLayer             // 감지할 레이어
        );

        if (hitTargets != null)
        {
            if (hitTargets.CompareTag("Player"))
            {
                PlayerController.instance.ImmediateDeath();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 hitboxGizmoCenter = (Vector2)transform.position + hitboxOffset;
        Gizmos.DrawWireCube(hitboxGizmoCenter, hitboxSize);
    }
}