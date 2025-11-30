using System.Collections;
using UnityEngine;

public class MawManager : MonoBehaviour
{
    public static MawManager instance { get; private set; }

    [Header("스킬셋")]
    public float skillInterval = 0.7887f;
    public int skillCount;
    public Maw_S1 S1;
    public Maw_S2 S2;

    [Header("시작")]
    public Vector2 startPos = new Vector2(44f, 1.004995f);
    public bool isFacingRight = false;

    [Header("중앙 X")]
    public float centerX;

    [HideInInspector] public bool canUseSklill = true;

    Transform currentSkillPos;
    int lastSkillNumber = 0;

    
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
        UseSkill();

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
}
