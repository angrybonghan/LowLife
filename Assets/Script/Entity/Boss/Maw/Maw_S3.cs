using System.Collections;
using UnityEngine;

public class Maw_S3 : MonoBehaviour, I_MawSkill
{
    [Header("공격")]
    public int shootCount = 35;
    public float shootInterval = 0.025f;

    [Header("발사체 늪")]
    public Transform firePoint;
    public GameObject smallSwamp;
    public float smallSwampDispersion = 0.5f;


    public bool isFacingRight { get; set; }
    bool isAttacking = false;

    void Start()
    {
        if (!isFacingRight)
        {
            transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        }

        StartCoroutine(SkillSequence());
    }
    
    IEnumerator SkillSequence()
    {
        yield return new WaitUntil(() => isAttacking);

        for (int i = 0; i < shootCount; i++)
        {
            ShootSmallSwomp();
            yield return new WaitForSeconds(shootInterval);
        }
    }

    void ShootSmallSwomp()
    {
        Vector2 spawnPos = firePoint.position;
        spawnPos.x += Random.value * (smallSwampDispersion / 2);

        Instantiate(smallSwamp, spawnPos, Quaternion.identity);
    }

    public void StartAttack()
    {
        isAttacking = true;
    }

    public Transform GetTransform()
    {
        return transform;
    }
}
