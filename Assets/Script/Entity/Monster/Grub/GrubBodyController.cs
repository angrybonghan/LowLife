using UnityEngine;

public class GrubBodyController : MonoBehaviour
{
    [Header("몸 스크립트")]
    public GrubBody[] bodies;

    [HideInInspector]
    public int bodyCount = 0;

    float deathDuration = 2; // 죽는 시간
    float fallingOutPower = 15; // 죽었을 때 날아갈 힘

    GrubMovement grubMovement;

    private void Awake()
    {
        grubMovement = GetComponent<GrubMovement>();
        bodyCount = bodies.Length;
    }

    void Start()
    {
        foreach (GrubBody body in bodies)
        {
            body.AppointTheirGod(this); // 내가... 이 세계의 『신』이 되겠다!!
        }

        deathDuration = grubMovement.deathDuration;
        fallingOutPower = grubMovement.fallingOutPower;
    }

    void Update()
    {

        if (bodyCount <= 0)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void EnableAttack(bool isEnable)
    {
        foreach (GrubBody body in bodies)
        {
            body.AttackEnable(isEnable);
        }
    }

    public void Dead(Transform attackerTransform)
    {
        Vector2 direction = (transform.position - attackerTransform.position).normalized;

        foreach (GrubBody body in bodies)
        {
            body.transform.SetParent(null);
            body.Dead(direction * fallingOutPower, deathDuration);
        }

        Destroy(gameObject);
    }
}
