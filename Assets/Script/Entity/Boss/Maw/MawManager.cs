using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MawManager : MonoBehaviour
{
    public static MawManager instance { get; private set; }

    [Header("시작")]
    public bool spawnMawAtStart = false;

    [Header("스킬셋")]
    public float skillInterval = 0.7887f;
    public int skillCount;
    public Maw_S1 S1;
    public Maw_S2 S2;
    public Maw_S3 S3;
    public Maw_S4 S4;

    [Header("사망")]
    public GameObject deathMaw;

    [Header("시작")]
    public Vector2 startPos = new Vector2(44f, 1.004995f);
    public bool isFacingRight = false;

    [Header("중앙 X")]
    public float centerX;

    [Header("늪 함정 범위")]
    public float swampPositionInterval = 0.45f;
    public float maxSwampX;
    public float minSwampX;

    [Header("HP")]
    public int maxHP = 30;


    [HideInInspector] public bool canUseSklill = true;
    [HideInInspector] public List<GameObject> allSwamp = new List<GameObject>();

    Transform currentSkillPos;
    int currentHP = 0;
    int lastSkillNumber = 0;
    int currentSmallSwampCount = 0;
    float normalizedHP = 0;
    float swampPositionCenter;
    bool halfHP = false;


    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            this.enabled = false;
            Destroy(gameObject);
            return;
        }

        currentHP = maxHP;
        normalizedHP = 1f;
    }

    private void Start()
    {
        canUseSklill = false;
        if (spawnMawAtStart) UseSkill();
        
        StartCoroutine(SkillManagerSequence());
    }

    IEnumerator SkillManagerSequence()
    {
        while (true)
        {
            yield return new WaitUntil(() => canUseSklill);
            yield return new WaitForSeconds(skillInterval);

            UseSkill();
            canUseSklill = false;
        }
    }

    void UseSkill()
    {
        Vector2 skillPos;
        if (currentSkillPos == null) skillPos = startPos;
        else 
        {
            skillPos = currentSkillPos.position; 
            Destroy(currentSkillPos.gameObject);
        }

        int skillNumber = GetNextSkillNumber();
        lastSkillNumber = skillNumber;
        I_MawSkill sk = null;
        if (skillNumber == 1)
        {
            Maw_S1 sk1 = Instantiate(S1, skillPos, Quaternion.identity);
            sk = sk1;

            sk1.centerX = centerX;
            if (halfHP)
            {
                sk1.phaseCount = 2;
            }
        }
        else if (skillNumber == 2)
        {
            Maw_S2 sk2 = Instantiate(S2, skillPos, Quaternion.identity);
            sk = sk2;

            sk2.centerX = centerX;
            if (halfHP)
            {
                sk2.jumpCount = 5;
                sk2.jumpInterval = 0.3f;
            }
            else
            {
                sk2.jumpCount = 3;
                sk2.jumpInterval = 0.75f;
            }
        }
        else if (skillNumber == 3)
        {
            Maw_S3 sk3 = Instantiate(S3, skillPos, Quaternion.identity);
            sk = sk3;

            if (halfHP)
            {
                sk3.shootCount = 25;
                sk3.poolCount = 7;
            }
            else
            {
                sk3.shootCount = 15;
                sk3.poolCount = 3;
            }
        }
        else
        {
            Maw_S4 sk4 = Instantiate(S4, skillPos, Quaternion.identity);
            sk = sk4;

            sk4.centerX = centerX;

            if (halfHP)
            {
                sk4.aimingTime = 0.7887f;
                sk4.phaseCount = 3;
            }
            else
            {
                sk4.aimingTime = 3f;
                sk4.phaseCount = 1;
            }
        }


        sk.isFacingRight = isFacingRight;
        currentSkillPos = sk.GetTransform();
    }

    /* 신내림을 받았을 경우 게임이 터지는 문제를 완벽하게 해결
    int GetNextSkillNumber()
    {
        int skillNumber = Random.Range(1, skillCount + 1);

        if (lastSkillNumber == skillNumber)
        {
            while (lastSkillNumber == skillNumber)
            {
                skillNumber = Random.Range(1, skillCount + 1);
            }
        }
        
        return skillNumber;
    }   OH MY KAMI-SAMA
    */ 

    int GetNextSkillNumber()
    {
        int nextNumber = Random.Range(1, skillCount);

        if (nextNumber >= lastSkillNumber)
        {
            nextNumber++;
        }

        return nextNumber;
    }

    public float GetSmallSwampXPos()
    {
        if (currentSmallSwampCount == 0)
        {
            SetSmallSwampXPos();
        }
        currentSmallSwampCount++;
        float newX = swampPositionCenter;
        newX += SmallSwampMultypleValue(currentSmallSwampCount - 1) * swampPositionInterval;

        return newX;
    }

    void SetSmallSwampXPos()
    {
        swampPositionCenter = Random.Range(minSwampX, maxSwampX);
    }
    
    public void ClearAllSwamp()
    {
        foreach (GameObject swamp in allSwamp)
        {
            Destroy(swamp);
        }

        allSwamp.Clear();
        currentSmallSwampCount = 0;
    }

    int SmallSwampMultypleValue(int x)
    {
        if (x % 2 == 0)
        {
            return -x / 2;
        }
        else
        {
            return (x + 1) / 2;
        }
    }

    public void TakeDamage()
    {
        currentHP--;
        Debug.Log(currentHP);
        normalizedHP = currentHP / maxHP;

        if (normalizedHP < 0.5f)
        {
            halfHP = true;
        }

        if (currentHP <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        Vector2 skillPos = currentSkillPos.position;
        Destroy(currentSkillPos.gameObject);

        Instantiate(deathMaw, skillPos, Quaternion.identity);
        StopAllCoroutines();
    }
}
