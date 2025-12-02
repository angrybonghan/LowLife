using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MawTongue : MonoBehaviour
{
    [Header("목표")]
    public float backoffY;
    public float targetY;
    public float floorY;

    [Header("속도")]
    public float attackTime = 0.2f;
    public float backoffTime = 0.5f;

    [Header("공격 시간")]
    public int phaseCount = 1;
    public float aimingTime = 3f;

    [Header("침")]
    public GameObject spit;
    public GameObject spitShatter;

    bool isAttacking = false;
    bool isAiming = false;
    Transform target;
    Coroutine moveCoroutine;
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }


    void Start()
    {
        target = transform;
        if (PlayerController.instance != null) target = PlayerController.instance.transform;
        Vector3 startPos = Vector3.zero;
        if (target != null) startPos.x = target.position.x;
        startPos.y = backoffY;
        transform.position = startPos;

        StartCoroutine(SkillSequence());
    }

    IEnumerator SkillSequence()
    {
        for (int i = 0; i < phaseCount; i++)
        {
            Vector2 targetPos = transform.position;
            targetPos.y = targetY;

            MoveTo(targetPos, 0.3f);
            yield return new WaitForSeconds(0.3f);
            Coroutine droolingCoroutine = StartCoroutine(Drooling());
            float time = 0;
            while (time < aimingTime)
            {
                time += Time.deltaTime;
                if (target != null) targetPos.x = target.position.x;
                transform.position = Vector2.Lerp(transform.position, targetPos, 8f * Time.deltaTime);
                yield return null;
            }
            StopCoroutine(droolingCoroutine);

            targetPos.y = floorY;
            isAttacking = true;
            MoveTo(targetPos, attackTime);
            yield return new WaitForSeconds(attackTime);
            isAttacking = false;
            CameraMovement.PositionShaking(0.2f, 0.05f, 0.2f);
            Instantiate(spitShatter, transform.position, Quaternion.identity);
            targetPos.y = backoffY;
            anim.SetTrigger("attack");
            MoveTo(targetPos, backoffTime);
            yield return new WaitForSeconds(backoffTime);
            anim.SetTrigger("default");
        }

        Destroy(gameObject);
    }

    IEnumerator Drooling()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.value * 0.3f);
            Instantiate(spit, transform.position, Quaternion.identity);
        }
    }

    void MoveTo(Vector2 targetPos, float duration)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(Co_MoveTo(targetPos, duration));
    }

    IEnumerator Co_MoveTo(Vector2 targetPos, float duration)
    {
        if (duration > 0)
        {
            float time = 0;
            Vector2 startPos = transform.position;

            while (time < duration)
            {
                yield return null;
                transform.position = Vector2.Lerp(startPos, targetPos, time / duration);
                time += Time.deltaTime;
            }
        }

        transform.position = targetPos;
        moveCoroutine = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAttacking) return;

        if (collision.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }
}
