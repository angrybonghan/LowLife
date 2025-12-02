using System.Collections;
using UnityEngine;

public class SpencerArm : MonoBehaviour
{
    [Header("허용 범위")]
    public float maxZAngle;
    public float minZAngle;
    public float normalAngle;

    [Header("방향")]
    public bool isFacingRight = true;

    [Header("공격")]
    public int weaponNumber; // 0,1,2 순으로 권총, 샷건, 로켓
    public float aimingTime;

    Transform targetPos;

    void Start()
    {
        targetPos = PlayerController.instance.transform;
    }

    void Update()
    {
        AimAtTarget();
    }

    IEnumerator Sequence()
    {
        float time = 0;
        while (time < aimingTime)
        {
            AimAtTarget();
            yield return null;
            time += Time.deltaTime;
        }

        WeaponFiring();
    }

    void WeaponFiring()
    {

    }

    void AimAtTarget()
    {
        float finalAngle;

        if (targetPos == null)
        {
            finalAngle = isFacingRight ? normalAngle : (normalAngle + 180f);
        }
        else
        {
            Vector3 direction = targetPos.position - transform.position;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            float angleToCompare = isFacingRight
                ? targetAngle
                : (targetAngle > 0 ? targetAngle - 180f : targetAngle + 180f);

            finalAngle = (angleToCompare >= minZAngle && angleToCompare <= maxZAngle)
                         ? targetAngle
                         : (isFacingRight ? normalAngle : (normalAngle + 180f));
        }

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, finalAngle + (isFacingRight ? 0 : 180));
        transform.rotation = targetRotation;
    }
}
