using UnityEngine;

public class AmagoRoadSelector : MonoBehaviour
{
    [Header("다음 길")]
    public AmagoRoadSelector nextRoad;
    
    [Header("조건부 길")]
    public bool useExRoad = false;
    public AmagoRoadSelector exRoad;

    [Header("조건부 길 감지 위치")]
    public Vector2 hitboxOffset = Vector2.zero;    // 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)
    public LayerMask playerLayer;

    AmagoRoadSelector ConfirmedRoad;
    Vector2 boxCenter;

    void Start()
    {
        if (useExRoad) boxCenter = (Vector2)transform.position + hitboxOffset;
    }

    public void SetConfirmedRoad()
    {
        if (useExRoad && IsPlayerInRange())
        {
            ConfirmedRoad = exRoad;
            nextRoad?.RoadDestruction();
        }
        else
        {
            ConfirmedRoad = nextRoad;
            exRoad?.RoadDestruction();
        }
    }

    public AmagoRoadSelector GetNextRoad()
    {
        return ConfirmedRoad;

        // **삼함연산은 이제 없다.**
        // 꿈과희망이사라지다!!!!!!!!!!!!!!!!!!!!!!!!!!
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

    public void RoadDestruction()
    {
        nextRoad?.RoadDestruction();
        exRoad?.RoadDestruction();

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (!useExRoad) return;

        Gizmos.color = Color.red;

        Vector2 hitboxGizmoCenter = (Vector2)transform.position + hitboxOffset;
        Gizmos.DrawWireCube(hitboxGizmoCenter, hitboxSize);
    }
}
