using UnityEngine;

public class GrubMovement : MonoBehaviour
{
    [Header("움직임")]
    public float maxSpeed = 8; // 최대 움직임 속도
    public float moveRadius; // 대기 상태에 들어간 위치로부터 최대 탐색 범위. 이 범위는 지형에 따라 조절될 수 있음.
    public float trunDuration = 0.5f;   // 회전 대기 시간

    [Header("지형 감지")]
    public Transform wallCheckPos;  // 벽
    public Transform groundCheckPos;    // 땅

    [Header("공격")]
    public float attackChargeTime = 0.5f;  // 공격의 준비 시간

    [Header("히트박스")]
    public Vector2 hitboxOffset = Vector2.zero;    // 히트박스 오프셋
    public Vector2 hitboxSize = new Vector2(1.0f, 1.0f); // 크기 (width, height)

    [Header("레이어, 캐스트")]
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어
    public LayerMask playerLayer;   // 플레이어 감지 레이어

    [Header("사망 후 제외 레이어")]
    public LayerMask afterDeathLayer;

    private int facingSign = 1; // 바라보는 방향

    private bool isFacingRight = true;  // 오른쪽을 바라보는지 여부
    private bool canGoStraight = true;  // 직진 가능 여부 (벽이 없고 땅이 있어야 함)
    private bool isDead = false;    // 죽었는지 여부

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Parried()
    {

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 hitboxLocalAdjustedOffset = new Vector2(hitboxOffset.x * facingSign, hitboxOffset.y);
        Vector2 hitboxGizmoCenter = (Vector2)transform.position + hitboxLocalAdjustedOffset;

        Gizmos.DrawWireCube(hitboxGizmoCenter, new Vector3(hitboxSize.x, hitboxSize.y, 0f));
    }
}
