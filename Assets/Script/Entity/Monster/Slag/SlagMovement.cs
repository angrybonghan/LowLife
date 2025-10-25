using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(BoxCollider2D))]
public class SlagMovement : MonoBehaviour
{
    [Header("움직임")]
    public float maxSpeed = 8; // 최대 움직임 속도
    public float moveRange; // 대기 상태에 들어간 위치로부터 최대 탐색 범위. 이 범위는 지형에 따라 조절될 수 있음.

    [Header("레이어, 캐스트")]
    public LayerMask obstacleMask;  // 장애물을 감지할 레이어

    [Header("공격")]
    public float attackChargeTime;  // 공격의 준비 시간
    public float attackDuration;    // 공격의 유지 시간
    public float attackCooldown;    // 공격 대기시간 (공격 쿨타임)

    [Header("죽음")]
    public float deathDuration = 2; // 죽는 시간
    public float fallingOutPower = 15; // 죽었을 때 날아갈 힘


    private float currentNormalizedSpeed = 0;

    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D boxCol;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCol = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
