using System.Collections;
using UnityEngine;

public class SpencerArm : MonoBehaviour
{
    [Header("팔 범위")]
    public float maxZAngle;
    public float minZAngle;

    [Header("팔 속도")]
    public float minArmMoveSpeed;
    public float maxArmMoveSpeed;

    [Header("방향")]
    public bool isFacingRight = true;

    [Header("공격")]
    public int weaponNumber; // 0,1,2 순으로 리볼버, 샷건, 로켓
    public float aimingTime;

    [Header("공격 위치")]
    public Transform firePoint;
    public Transform aimPoint;

    Transform targetPos;

    void Start()
    {
        targetPos = PlayerController.instance.transform;
    }

    void Update()
    {


    }

    //IEnumerator Sequence()
    //{


    //}

    void WeaponFiring()
    {

    }

}
