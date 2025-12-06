using UnityEngine;

public class PlayerPosToDestroyGameobject : MonoBehaviour
{
    [Header("플레이어 범위")]
    public Vector2 hitboxOffset = Vector2.zero;    // 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask playerLayer;

    Vector2 boxCenter;
    private void Start()
    {
        boxCenter = (Vector2)transform.position + hitboxOffset;
    }

    private void Update()
    {
        if (IsPlayerInRange())
        {
            Destroy(gameObject);
        }
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
