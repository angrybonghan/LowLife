using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Spencer_S1 : MonoBehaviour
{
    [Header("공격 시간")]
    public float aimingTime = 1.0f;

    [Header("팔들")]
    public SpencerArm[] arms;

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }


    void Start()
    {
        int randomWeaponNumber = Random.Range(0, 4);
        foreach (SpencerArm arm in arms)
        {
            arm.weaponNumber = randomWeaponNumber;
        }
    }


    void Update()
    {
        
    }
    

    IEnumerator SkillSequence()
    {
        yield return null;
    }
}
