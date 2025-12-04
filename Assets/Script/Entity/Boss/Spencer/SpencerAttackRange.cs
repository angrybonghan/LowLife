using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SpencerAttackRange : MonoBehaviour
{
    [Header("범위")]
    public GameObject redZone;
    public float xRange = 7.5f;
    public float startY = 14;

    [Header("시간")]
    public float gunDelay = 0.25f;

    [Header("발사체")]
    public EnemyLaser projectile;

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        Vector3 zoneScale = redZone.transform.localScale;
        zoneScale.x = xRange;
        redZone.transform.localScale = zoneScale;
    }

    public void StartGunShot()
    {
        StartCoroutine(WaitGunDelay());
    }

    public void ReloadPos()
    {
        Vector2 rangePos = PlayerController.instance.transform.position;
        rangePos.y = 0;
        transform.position = rangePos;

        anim.SetTrigger("start");
    }

    IEnumerator WaitGunDelay()
    {
        yield return new WaitForSeconds(gunDelay);
        Shot();
    }

    public void Shot()
    {
        for (int i = 0; i < 2; i++)
        {
            Vector2 pos = GetRandomPositionInRange();
            EnemyLaser proj = Instantiate(projectile, pos, Quaternion.identity);
            proj.LookPos(pos + Vector2.down);
        }
    }

    public Vector2 GetRandomPositionInRange()
    {
        float centerX = transform.position.x;

        float halfXRange = xRange / 2f;
        float randomX = Random.Range(centerX - halfXRange, centerX + halfXRange);
        Vector2 randomPosition = new Vector2(randomX, startY);

        return randomPosition;
    }

    public void EndAttack()
    {
        anim.SetTrigger("end");
    }

    public void Destruction()
    {
        Destroy(gameObject);
    }
}
