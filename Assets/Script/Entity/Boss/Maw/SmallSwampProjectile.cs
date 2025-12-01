using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SmallSwampProjectile : MonoBehaviour
{
    [Header("加档")]
    public float riseSpeed = 5.0f;
    public float dropSpeed = 4.0f;

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

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
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
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
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
}
