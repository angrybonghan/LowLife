using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(BoxCollider2D))]
public class SlagMovement : MonoBehaviour
{
    [Header("������")]
    public float maxSpeed = 8; // �ִ� ������ �ӵ�
    public float moveRange; // ��� ���¿� �� ��ġ�κ��� �ִ� Ž�� ����. �� ������ ������ ���� ������ �� ����.

    [Header("���̾�, ĳ��Ʈ")]
    public LayerMask obstacleMask;  // ��ֹ��� ������ ���̾�

    [Header("����")]
    public float attackChargeTime;  // ������ �غ� �ð�
    public float attackDuration;    // ������ ���� �ð�
    public float attackCooldown;    // ���� ���ð� (���� ��Ÿ��)

    [Header("����")]
    public float deathDuration = 2; // �״� �ð�
    public float fallingOutPower = 15; // �׾��� �� ���ư� ��


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
