using System.Collections;
using UnityEngine;

public class Spencer_S1 : MonoBehaviour
{
    [Header("공격 시간")]
    public float aimingTime = 1.0f;

    [Header("팔들")]
    public SpencerArm[] arms;

    private void Awake()
    {
        int randomWeaponNumber = Random.Range(0, 4);
        foreach (SpencerArm arm in arms)
        {
            arm.weaponNumber = randomWeaponNumber;
        }
    }


    void Start()
    {
        
    }


    void Update()
    {
        
    }
    

    IEnumerator SkillSequence()
    {
        yield return null;
    }
}
