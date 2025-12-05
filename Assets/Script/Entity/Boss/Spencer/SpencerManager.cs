using System.Collections;
using UnityEngine;

public class SpencerManager : MonoBehaviour
{
    public static SpencerManager Instance;

    [Header("시작")]
    public bool spawnSpencerAtStart = false;
    public Vector2 startPos = new Vector2(44f, 1.004995f);

    [Header("스킬셋")]
    public float skillInterval = 0.6887f;
    public int skillCount;
    public Spencer_S1 S1;
    public Spencer_S2 S2;
    public Spencer_S4 S4;

    [Header("서브 스킬셋")]
    public Spencer_SS0 SS0;
    public Spencer_SS1 SS1;

    [HideInInspector] public int randomWeaponNumber;
    [HideInInspector] public int positionNumber = 2;
    [HideInInspector] public bool canUseSklill = true;
    [HideInInspector] public bool[] possessWeapon = { true, true, true, true, true, true };

    int totalSkillsUsed = 0;
    int lastSkillNumber = -1;
    Vector2 currentSkillPos;
    GameObject currentSkillInstance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            this.enabled = false;
            return;
        }

        randomWeaponNumber = Random.Range(0, 3);
    }

    void Start()
    {
        currentSkillPos = startPos;
        canUseSklill = false;
        if (spawnSpencerAtStart) UseSkill();

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
        if (currentSkillInstance != null)
        {
            currentSkillPos = currentSkillInstance.transform.position;
            Destroy(currentSkillInstance);
        }

        if (totalSkillsUsed % 2 == 0)
        {
            currentSkillInstance = Instantiate(SS0, currentSkillPos, Quaternion.identity).gameObject;
        }
        else
        {
            int skillNumber = GetNextSkillNumber();
            lastSkillNumber = skillNumber;

            if (skillNumber == 1)
            {
                if (GetWeaponCount() <= 4)
                {
                    currentSkillInstance = Instantiate(SS1, currentSkillPos, Quaternion.identity).gameObject;
                    lastSkillNumber = -1;

                    for (int i = 0; i < possessWeapon.Length; i++)
                    {
                        possessWeapon[i] = true;
                    }
                }
                else
                {
                    randomWeaponNumber = Random.Range(0, 3);
                    currentSkillInstance = Instantiate(S1, currentSkillPos, Quaternion.identity).gameObject;
                }
            }
            else if (skillNumber == 2)
            {
                currentSkillInstance = Instantiate(S2, currentSkillPos, Quaternion.identity).gameObject;
            }
            else if (skillNumber == 3)
            {
                currentSkillInstance = Instantiate(S4, currentSkillPos, Quaternion.identity).gameObject;
            }
        }

        totalSkillsUsed++;
    }

    int GetNextSkillNumber()
    {
        if (lastSkillNumber == -1) return Random.Range(1, skillCount + 1);

        int nextNumber = Random.Range(1, skillCount);

        if (nextNumber >= lastSkillNumber)
        {
            nextNumber++;
        }

        return nextNumber;
    }

    public int GetWeaponCount()
    {
        int count = 0;

        foreach (bool hasWeapon in possessWeapon)
        {
            if (hasWeapon)
            {
                count++;
            }
        }

        return count;
    }
}
