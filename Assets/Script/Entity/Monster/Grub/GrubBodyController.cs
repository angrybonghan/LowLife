using UnityEngine;

public class GrubBodyController : MonoBehaviour
{
    [Header("몸 스크립트")]
    public GrubBody[] bodies;

    [HideInInspector]
    public int bodyCount = 0;

    void Start()
    {
        bodyCount = bodies.Length;

        foreach (GrubBody body in bodies)
        {
            body.AppointTheirGod(this); // 내가... 이 세계의 『신』이 되겠다!!
        }
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

    public void Dead(Transform attackerTransform, float fallingOutPower, float deathDuration)
    {
        Vector2 direction = (transform.position - attackerTransform.position).normalized;

        foreach (GrubBody body in bodies)
        {
            body.Dead(direction * fallingOutPower, deathDuration);
        }
    }
}
