using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SmallSwampProjectile : MonoBehaviour
{
    [Header("加档")]
    public float riseSpeed = 5.0f;
    public float dropSpeed = 4.0f;

    [Header("盒魂狼 加档")]
    public float dispersionSpeed = 1f;

    [Header("此")]
    public GameObject groundSwamp;
    public GameObject effect;
    public bool isOnlyEffect;

    [Header("备开")]
    public float minX;
    public float maxX;
    public float targetY;
    public float floorY;

    bool isGoingUp = true;
    float currentDispersionSpeed = 0f;

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        currentDispersionSpeed = dispersionSpeed * Random.Range(-1f, 1f);
    }

    private void Update()
    {
        if (isGoingUp)
        {
            GoUp();
        }
        else
        {
            GoDown();
        }
    }

    void GoUp()
    {
        Vector2 newPos = transform.position;
        newPos.x += currentDispersionSpeed * Time.deltaTime;
        newPos.y += riseSpeed * Time.deltaTime;
        transform.position = newPos;

        if (targetY <= transform.position.y)
        {
            SetDropPos();
            isGoingUp = false;
            anim.SetTrigger("gotoDown");
        }
    }

    void GoDown()
    {
        transform.position += Vector3.down * dropSpeed * Time.deltaTime;

        if (floorY >= transform.position.y)
        {
            SetGroundSwamp();
        }
    }

    void SetDropPos()
    {
        Vector2 pos = transform.position;
        pos.x = isOnlyEffect ? Random.Range(minX, maxX) : MawManager.instance.GetSmallSwampXPos(); ;

        transform.position = pos;
    }

    void SetGroundSwamp()
    {
        Vector2 pos = transform.position;
        pos.y = floorY;

        if (isOnlyEffect) Instantiate(effect, pos, Quaternion.identity);
        else
        {
            GameObject newSwamp = Instantiate(groundSwamp, pos, Quaternion.identity);
            MawManager.instance.allSwamp.Add(newSwamp);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController.instance.OnAttack(0.5f, 1, 0.1f, transform);
        }
    }

}
