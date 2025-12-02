using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Maw_S3 : MonoBehaviour, I_MawSkill, I_Attackable
{
    [Header("공격")]
    public int shootCount = 35;
    public float shootInterval = 0.025f;

    [Header("발사체 늪")]
    public Transform firePoint;
    public SmallSwampProjectile smallSwamp;
    public float swampDispersion = 0.5f;
    public float dispersionSpeed = 2.0f;
    public int poolCount = 4;

    bool isAttackEnd = false;

    public bool isFacingRight { get; set; }
    bool isAttacking = false;
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

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

        yield return ShootSwamp();

        anim.SetTrigger("attackEnd");

        yield return new WaitUntil(() => isAttackEnd);

        MawManager.instance.canUseSklill = true;
    }

    IEnumerator ShootSwamp()
    {
        MawManager.instance.ClearAllSwamp();

        bool[] isPool = new bool[shootCount];
        for (int i = 0; i < poolCount; i++)
        {
            isPool[i] = true;
        }
        ShuffleBooleanArray(isPool);

        for (int i = 0; i < shootCount; i++)
        {
            Attack(isPool[i]);
            yield return new WaitForSeconds(shootInterval);
        }
    }

    public void ShuffleBooleanArray(bool[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            bool temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

    void Attack(bool isPool)
    {
        Vector2 spawnPos = firePoint.position;
        //spawnPos.x += Random.value * (swampDispersion / 2);
        // 아 병ㅅ
        spawnPos.x += ((Random.value - 0.5f) * 2) * swampDispersion;


        SmallSwampProjectile swamp = Instantiate(smallSwamp, spawnPos, Quaternion.identity);
        swamp.isOnlyEffect = !isPool;
        swamp.dispersionSpeed = dispersionSpeed;
    }

    public void EndAttack()
    {
        isAttackEnd = true;
    }

    public void StartAttack()
    {
        isAttacking = true;
    }

    public Transform GetTransform()
    {
        return transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }

    public bool CanAttack(Transform attacker)
    {
        return true;
    }

    public void OnAttack(Transform attacker)
    {
        MawManager.instance.TakeDamage();
    }
}
