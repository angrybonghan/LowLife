using UnityEngine;

public class StingSoldierMovement : MonoBehaviour
{
    [Header("������")]
    public float moveSpeed = 5; // ������ �ӵ�
    public float moveRange; // ��� ���¿� �� ��ġ�κ��� �ִ� �̵��� �� �ִ� ����

    [Header("����")]
    public float attackRange;   // ������ ����
    public float detectionRange;    // ������ ����
    public float attackCooldown;    // ���� ���ð� (���� ��Ÿ��)
    public float attackDuration;    // ���� ����� ���� �ð�

    [Header("��Ʈ�ڽ�")]
    public Vector2 hitboxOffset;    // ��Ʈ�ڽ� ������
    public float hitboxWidth = 1.0f;    // ��Ʈ�ڽ� ���� ���� (��)
    public float hitboxHeight = 1.0f;   // ��Ʈ�ڽ� ���� ���� (����)


    Vector3 idleStartPos;   // ��Ⱑ ���۵� ��ġ
    public enum state { idle, track, attack }
    state currentState;


    void Start()
    {
        
    }

    void Update()
    {
        
    }


}
