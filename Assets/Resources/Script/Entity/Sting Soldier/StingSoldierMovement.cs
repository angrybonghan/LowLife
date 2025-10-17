using UnityEngine;

public class StingSoldierMovement : MonoBehaviour
{
    [Header("움직임")]
    public float moveSpeed = 5; // 움직임 속도
    public float moveRange; // 대기 상태에 들어간 위치로부터 최대 이동할 수 있는 범위

    [Header("공격")]
    public float attackRange;   // 공격의 범위
    public float detectionRange;    // 감지의 범위
    public float attackCooldown;    // 공격 대기시간 (공격 쿨타임)
    public float attackDuration;    // 공격 모션의 유지 시간

    [Header("히트박스")]
    public Vector2 hitboxOffset;    // 히트박스 오프셋
    public float hitboxWidth = 1.0f;    // 히트박스 가로 길이 (폭)
    public float hitboxHeight = 1.0f;   // 히트박스 세로 길이 (높이)


    Vector3 idleStartPos;   // 대기가 시작된 위치
    public enum state { idle, track, attack }
    state currentState;


    void Start()
    {
        
    }

    void Update()
    {
        
    }


}
