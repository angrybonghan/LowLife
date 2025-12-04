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


    [HideInInspector] public int randomWeaponNumber;
    [HideInInspector] public bool canUseSklill = true;
    
    int lastSkillNumber = 0;
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
        if (currentSkillInstance != null) Destroy(currentSkillInstance);
        int skillNumber = GetNextSkillNumber();
        lastSkillNumber = skillNumber;

        if (skillNumber == 1)
        {
            randomWeaponNumber = Random.Range(0, 3);
            currentSkillInstance = Instantiate(S1, currentSkillPos, Quaternion.identity).gameObject;
        }
        else if (skillNumber == 2)
        {
            currentSkillInstance = Instantiate(S2, currentSkillPos, Quaternion.identity).gameObject;
        }
    }

    int GetNextSkillNumber()
    {
        int nextNumber = Random.Range(1, skillCount);

        if (nextNumber >= lastSkillNumber)
        {
            nextNumber++;
        }

        return nextNumber;
    }
}
