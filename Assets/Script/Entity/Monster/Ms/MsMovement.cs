using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(BoxCollider2D))]
public class MsMovement : MonoBehaviour
{
    [Header("������")]
    public float maxSpeed = 8; // �ִ� ������ �ӵ�
    public float moveRadius; // ��� ���¿� �� ��ġ�κ��� �ִ� Ž�� ����. �� ������ ������ ���� ������ �� ����.
    public float trunDuration = 0.5f;   // ȸ�� ��� �ð�
    public float acceleration = 2f; // ���ӵ�

    [Header("���� ����")]
    public Transform wallCheckPos;  // ��
    public Transform groundCheckPos;    // ��

    [Header("����")]
    public float readyToAttackTime = 0.5f;  // ������ �غ� �ð� (�� ���, ������)
    public float aimingTime = 0.2f;          // ���� �ð� (���� �߻�)
    public float reloadTime = 0.5f;  // ������ �غ� �ð� (������)

    [Header("�����")]
    public float damage = 0.4f;  // ���� �����

    [Header("�þ� ����")]
    public Vector2 viewOffset = new Vector2(0f, 0.5f); // �þ� �߽��� ������
    public Vector2 viewSize = new Vector2(5f, 3f);     // �þ� ������ ����/���� ũ��

    [Header("���� ����")]
    public float attackRange;

    [Header("���̾�, ĳ��Ʈ")]
    public LayerMask obstacleMask;  // ��ֹ��� ������ ���̾�
    public LayerMask playerLayer;   // ���� ���̾�

    [Header("�÷��̾� ����")]
    public Transform exclamationMarkPos;    // ����ǥ ��ġ
    public GameObject exclamationMarkObj;    // ����ǥ ������
    public float detectionTime = 1.0f;  // �߰��� ���� �ð�
    public float detectionCancelTime = 0.5f;  // �ǽ� ���� �ð�
    public float detectionDecayTime = 2.0f; // �߰��� ���� �ð�

    [Header("����")]
    public float deathDuration = 2; // �״� �ð�
    public float fallingOutPower = 15; // �׾��� �� ���ư� ��

    private int facingSign = 1; // �ٶ󺸴� ����

    private float currentNormalizedSpeed = 0;   // ����ȭ�� �ӵ�
    private float layerCheckRadius = 0.05f;  // ���� ��ġ �ݰ�
    private float detectionRate = 0;    // �߰��� ����

    private bool isFacingRight = true;  // �������� �ٶ󺸴��� ����
    private bool canGoStraight = true;  // ���� ���� ���� (���� ���� ���� �־�� ��)
    private bool isMoving = false;  // �����̰� �ִ��� ����
    private bool isDead = false;    // �׾����� ����

    Vector3 movePosRight;
    Vector3 movePosLeft;
    Vector3 targetPos;

    public enum state { idle, doubt, track, attack, endAttack }
    state currentState;

    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D boxCol;
    private ExclamationMarkHandler exclamationMark;

    GameObject playerObject;    // �÷��̾� ������Ʈ

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCol = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        playerObject = GameObject.FindWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("�÷��̾� ����");
        }

        isFacingRight = true;
        SetState(state.idle);
    }

    void Update()
    {
        
    }

    void SetState(state targetState)
    {
        StopAllCoroutines();
        currentState = targetState;

        rb.velocity = Vector3.zero;
        currentNormalizedSpeed = 0;

        if (targetState == state.idle)
        {
            movePosRight = movePosLeft = transform.position;
            movePosRight.x += moveRadius;
            movePosLeft.x -= moveRadius;

            targetPos = isFacingRight ? movePosRight : movePosLeft;

        }
        else if (targetState == state.doubt)
        {
            exclamationMark = Instantiate(exclamationMarkObj).GetComponent<ExclamationMarkHandler>();
            exclamationMark.SetTargetPos(exclamationMarkPos);

        }
        else if (targetState == state.attack)
        {

        }
        else if (targetState == state.endAttack)
        {

        }
    }





    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;

        Vector2 viewLocalAdjustedOffset = new Vector2(viewOffset.x * facingSign, viewOffset.y);
        Vector2 viewGizmoCenter = (Vector2)transform.position + viewLocalAdjustedOffset;

        Gizmos.DrawWireCube(viewGizmoCenter, new Vector3(viewSize.x, viewSize.y, 0f));

        Gizmos.color = Color.cyan;

        if (Application.isPlaying)
        {
            if (currentState == state.idle)
            {
                Gizmos.DrawWireSphere(movePosRight, 0.25f);
                Gizmos.DrawWireSphere(movePosLeft, 0.25f);
                Gizmos.DrawLine(movePosRight, movePosLeft);
            }
        }
        else
        {
            Vector3 gizmosMovePosRight = transform.position;
            Vector3 gizmosMovePosLeft = transform.position;
            gizmosMovePosRight.x += moveRadius;
            gizmosMovePosLeft.x -= moveRadius;

            Gizmos.DrawWireSphere(gizmosMovePosRight, 0.25f);
            Gizmos.DrawWireSphere(gizmosMovePosLeft, 0.25f);
            Gizmos.DrawLine(gizmosMovePosRight, gizmosMovePosLeft);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheckPos.position, 0.05f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(wallCheckPos.position, 0.05f);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(exclamationMarkPos.position, 0.05f);
    }
}
