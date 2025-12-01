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

    [Header("시작")]
    public Vector2 startPos = new Vector2(44f, 1.004995f);
    public bool isFacingRight = false;

    [Header("중앙 X")]
    public float centerX;

    [Header("늪 함정 범위")]
    public float swampPositionInterval = 0.45f;
    public float maxSwampX;
    public float minSwampX;



    [HideInInspector] public bool canUseSklill = true;
    [HideInInspector] public List<GameObject> allSwamp = new List<GameObject>();

    Transform currentSkillPos;
    int lastSkillNumber = 0;
    int currentSmallSwampCount = 0;
    float swampPositionCenter;



    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            this.enabled = false;
            Destroy(gameObject);
            return;
        }

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
        }
        else if (skillNumber == 2)
        {
            Maw_S2 sk2 = Instantiate(S2, skillPos, Quaternion.identity);
            sk = sk2;

            sk2.centerX = centerX;
        }
        else if (skillNumber == 3)
        {
            Maw_S3 sk3 = Instantiate(S3, skillPos, Quaternion.identity);
            sk = sk3;
        }


        sk.isFacingRight = isFacingRight;
        currentSkillPos = sk.GetTransform();
    }

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


}
