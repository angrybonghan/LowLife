using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D),typeof(Animator))]
public class Spencer_S4 : MonoBehaviour, I_Attackable
{
    [Header("범위")]
    public float centerX;
    public float laserStartingRange;
    public float startY;

    [Header("레이저")]
    public DiffuseLaser laser;

    [Header("레이저 설정")]
    public float laserDispersion;
    public float timeToFire;
    public int laserCount;
    public float firingInterval;

    Collider2D col;
    Animator anim;
    Vector2 absurdSpace = new(0b11111101001, 0b10010110101);    // 그냥 아주 먼 아무 공간, 레이저 생성 위치에 사용됨
    Vector2 attackOrigin;
    Transform player;
    bool callTunnel = false;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (SpencerManager.Instance.halfHP)
        {
            timeToFire = 1.2f;
            laserCount = 80;
            firingInterval = 0.015f;
            laserDispersion = 2.0f;
            callTunnel = true;
            TrainTunnelMovement.instance.StartTunnel();
        }

        if (PlayerController.instance != null) player = PlayerController.instance.transform;

        float offset = Random.value * laserStartingRange;
        offset *= Random.value > 0.5f ? 1 : -1;
        attackOrigin.x = transform.position.x + offset;
        attackOrigin.y = startY;
    }

    public void StartFiringLaser()
    {
        col.enabled = false;
        StartCoroutine(SkillSequence());
    }

    IEnumerator SkillSequence()
    {
        for (int i = 0; i < laserCount; i++)
        {
            FiringLaser();
            yield return new WaitForSeconds(firingInterval);
        }

        if (callTunnel)
        {
            yield return new WaitForSeconds(timeToFire);
            TrainTunnelMovement.instance.EndTunnel();
        }

        anim.SetTrigger("end");
    }

    void FiringLaser()
    {
        if (player == null) return;

        DiffuseLaser proj = Instantiate(laser, absurdSpace, Quaternion.identity);
        proj.laserDispersion = laserDispersion;
        proj.timeToFire = timeToFire;
        proj.LookPos(attackOrigin, player.position);
    }

    public void EndAttack()
    {
        col.enabled = true;
        SpencerManager.Instance.canUseSklill = true;
    }

    public void PlayTeleportStartSound()
    {
        SpencerManager.Instance.PlayTeleportSound(true, transform.position);
    }

    public void PlayTeleportEndSound()
    {
        SpencerManager.Instance.PlayTeleportSound(false, transform.position);
    }

    public bool CanAttack(Transform attacker)
    {
        return true;
    }

    public void OnAttack(Transform attacker)
    {
        SpencerManager.Instance.TakeDamage();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }
}
