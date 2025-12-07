using System.Collections;
using UnityEngine;

public class SpencerManager : MonoBehaviour
{
    public static SpencerManager Instance;

    [Header("시작")]
    public bool spawnSpencerAtStart = false;
    public Vector2 startPos = new Vector2(44f, 1.004995f);

    [Header("스킬셋")]
    public float skillInterval = 0.5887f;
    public int skillCount;
    public Spencer_S1 S1;
    public Spencer_S2 S2;
    public Spencer_S3 S3;
    public Spencer_S4 S4;

    [Header("서브 스킬셋")]
    public Spencer_SS0 SS0;
    public Spencer_SS1 SS1;

    [Header("거미줄")]
    public float slownessDuration = 8f;
    public float minSlownessSpeed = 6f;

    [Header("HP")]
    public int maxHP = 46;

    [HideInInspector] public int randomWeaponNumber;
    [HideInInspector] public int positionNumber = 2;
    [HideInInspector] public bool canUseSklill = true;
    [HideInInspector] public bool[] possessWeapon = { true, true, true, true, true, true };
    [HideInInspector] public bool halfHP = false;

    int currentHP = 0;
    int totalSkillsUsed = 0;
    int lastSkillNumber = -1;
    float originalPlayerSpeed;
    float currentSlownessSpeed;
    Vector2 currentSkillPos;
    GameObject currentSkillInstance;
    Coroutine slownessCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            this.enabled = false;
            return;
        }

        currentHP = maxHP;
        halfHP = false;
        randomWeaponNumber = Random.Range(0, 3);
    }

    void Start()
    {
        originalPlayerSpeed = PlayerController.instance.maxSpeed;
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
            Spencer_SS0 sk = Instantiate(SS0, currentSkillPos, Quaternion.identity);
            currentSkillInstance = sk.gameObject;

            if (totalSkillsUsed == 0)
            {
                sk.useUnconditionallyLocation = true;
                sk.unconditionallyLocationNumber = 3;
            }
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
                currentSkillInstance = Instantiate(S3, currentSkillPos, Quaternion.identity).gameObject;
            }
            else if (skillNumber == 4)
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

    public void StartSlownessPlayer()
    {
        if (slownessCoroutine != null)
        {
            StopCoroutine(slownessCoroutine);
            currentSlownessSpeed *= 0.5f;
        }
        else
        {
            currentSlownessSpeed = minSlownessSpeed;
        }

        PlayerController.instance.maxSpeed = currentSlownessSpeed;
        slownessCoroutine = StartCoroutine(Slowness());
    }

    IEnumerator Slowness()
    {
        yield return new WaitForSeconds(slownessDuration);
        PlayerController.instance.maxSpeed = originalPlayerSpeed;
        slownessCoroutine = null;
    }

    public void TakeDamage()
    {
        currentHP--;
        Debug.Log(currentHP);

        if (currentHP <= 0)
        {
            Death();
            return;
        }

        if (!halfHP)
        {
            if (currentHP * 2 <= maxHP)
            {
                halfHP = true;
                skillInterval = 0.3f;
            }
        }
    }

    void Death()
    {

    }
}
