using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SpencerArm : MonoBehaviour, I_Attackable
{
    [Header("팔 번호")]
    public int armNumber = 1;

    [Header("팔 범위")]
    public float maxZAngle;
    public float minZAngle;

    [Header("팔 속도")]
    public float minArmMoveSpeed;
    public float maxArmMoveSpeed;

    [Header("방향")]
    public bool isFacingRight = true;

    [Header("공격 위치")]
    public Transform firePoint;
    public Transform aimPoint;

    [Header("투사체")] // 웨폰 넘버 0-1-2 순
    public EnemyLaser revolver;
    public ShotGunBullet shotgun;
    public Ms_PatriotProjectile roket;

    [Header("놓칠 총")]
    public GameObject revolverDrop;
    public GameObject shotgunDrop;
    public GameObject roketDrop;

    [Header("소리")]
    public AudioClip[] gunOutSounds;
    public AudioClip[] revolverFireSounds;
    public AudioClip[] shotgunFireSounds;
    public AudioClip[] roketFireSounds;


    [HideInInspector] public Spencer_S1 parents;
    int weaponNumber;
    bool gunDrop = false;
    bool canGunDrop = false;
    Animator anim;

    private void Awake()
    {
        armNumber--;
        if (!SpencerManager.Instance.possessWeapon[armNumber])
        {
            Destroy(gameObject);
            this.enabled = false;
            return;
        }

        anim = GetComponent<Animator>();
        weaponNumber = SpencerManager.Instance.randomWeaponNumber;
        anim.SetInteger("weaponNumber", weaponNumber);
    }

    private void Start()
    {
        if (SpencerManager.Instance.halfHP)
        {
            minArmMoveSpeed = 0.1f;
            maxArmMoveSpeed = 0.3f;
        }

        AudioManager.instance.PlayRandomClipAtPointWithPitch(transform.position, gunOutSounds, Random.Range(0.5f, 1.5f));
    }


    IEnumerator ArmMove()
    {
        float angle1 = maxZAngle;
        float angle2 = minZAngle;

        if (Random.value > 0.5f)
        {
            angle1 = minZAngle;
            angle2 = maxZAngle;
        }

        while (true)
        {
            yield return StartCoroutine(RotateCoroutine(angle1, GetArmSpeed()));
            yield return StartCoroutine(RotateCoroutine(angle2, GetArmSpeed()));
        }

    }

    public void FireWeapon()
    {
        StopAllCoroutines();
        anim.SetTrigger("fire");
        canGunDrop = false;
    }

    public void EndMoveArms()
    {
        parents.EndMoveArms();
    }

    public void SummonProjectile()
    {
        if (gunDrop) return;

        if (weaponNumber == 0)
        {
            EnemyLaser proj = Instantiate(revolver, firePoint.position, Quaternion.identity);
            proj.SetTarget(aimPoint);
            proj.shrinkTime = 0.1f;
            proj.laserThickness = 0.1f;

            AudioManager.instance.PlayRandomClipAtPointWithPitch(transform.position, revolverFireSounds, Random.Range(0.5f, 1.5f));
            CameraMovement.PositionShaking(0.5f, 0.05f, 0.2f);
        }
        else if (weaponNumber == 1)
        {
            for (int i = 0; i < 16; i++)
            {
                Instantiate(shotgun, firePoint.position, Quaternion.identity).LookPos(aimPoint.position);
            }

            AudioManager.instance.PlayRandomClipAtPointWithPitch(transform.position, shotgunFireSounds, Random.Range(0.5f, 1.5f));
            CameraMovement.PositionShaking(0.75f, 0.05f, 0.2f);
        }
        else
        {
            Ms_PatriotProjectile proj = Instantiate(roket, firePoint.position, Quaternion.identity);
            proj.SetFacing(aimPoint.position);

            AudioManager.instance.PlayRandomClipAtPointWithPitch(transform.position, roketFireSounds, Random.Range(0.5f, 1.5f));
            CameraMovement.PositionShaking(1f, 0.05f, 0.2f);
        }
    }

    float GetArmSpeed()
    {
        return Random.Range(minArmMoveSpeed, maxArmMoveSpeed);
    }

    public void StartArmMove()
    {
        StartCoroutine(ArmMove());
        parents.StartMoveArms();
        canGunDrop = true;
    }

    public void Destruction()
    {
        if (CheckAllWeaponsFalse())
        {
            parents.EndMoveArms();
        }

        Destroy(gameObject);
    }

    IEnumerator RotateCoroutine(float targetZ, float duration)
    {
        float timeElapsed = 0f;
        float startZ = transform.eulerAngles.z;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZ);

        while (timeElapsed < duration)
        {
            float t = timeElapsed / duration;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
            yield return null;
            timeElapsed += Time.deltaTime;
        }

        transform.rotation = targetRotation;
    }

    public bool CanAttack(Transform attacker)
    {
        return canGunDrop;
    }

    public void OnAttack(Transform attacker)
    {
        GetComponent<Collider2D>().enabled = false;
        anim.SetTrigger("hit");
        gunDrop = true;
        SpencerManager.Instance.possessWeapon[armNumber] = false;
    }

    public void GunDrop()
    {
        GameObject gunDrop = null;
        if (weaponNumber == 0) gunDrop = revolverDrop;
        else if (weaponNumber == 1) gunDrop = shotgunDrop;
        else gunDrop = roketDrop;

        GameObject gun = Instantiate(gunDrop, firePoint.position, Quaternion.identity);
        if (!isFacingRight)
        {
            Vector2 scale = gun.transform.localScale;
            scale.x *= -1f;
            gun.transform.localScale = scale;
        }
    }

    public bool CheckAllWeaponsFalse()
    {
        foreach (bool hasWeapon in SpencerManager.Instance.possessWeapon)
        {
            if (hasWeapon)
            {
                return false;
            }
        }

        return true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (gunDrop) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }
}
