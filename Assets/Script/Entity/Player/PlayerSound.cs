using UnityEngine;

public enum playerSoundType { FootStep, Throw, Jump, Dash, Parry, QuickTrun, Stick, WallKick, BigCloth, Cloth, ShieldLerp, Death, Block, Fall };

public class PlayerSound : MonoBehaviour
{
    [Header("¹ß¼Ò¸®")]
    public LayerMask groundMask;
    public AudioClip[] grassFootStepSound;
    public AudioClip[] stoneFootStepSound;
    public AudioClip[] woodFootStepSound;

    [Header("Äü ÅÏ")]
    public AudioClip[] quickTrunSound;

    [Header("¹æÆÐ ÅõÃ´, µµ¾à")]
    public AudioClip[] throwSound;
    public AudioClip shieldLerp;

    [Header("º® °ü·Ã")]
    public AudioClip stickSound;
    public AudioClip wallKickSound;

    [Header("´ë½¬")]
    public AudioClip dashSound;

    [Header("¿Ê ÆÞ·°ÀÓ")]
    public AudioClip[] cloth;
    public AudioClip[] bigCloth;

    [Header("¸·±â, ÆÐ¸µ")]
    public AudioClip shieldBlockSound;
    public AudioClip parrySound;

    [Header("³Ñ¾îÁü")]
    public AudioClip fallSound;

    [Header("Á×À½")]
    public AudioClip deathSound;


    public void PlaySound(playerSoundType type)
    {
        AudioClip clip = null;

        switch (type)
        {
            case playerSoundType.FootStep:
                clip = GetMaterialFootstepSound();
                break;
            case playerSoundType.Throw:
                clip = AudioManager.GetRandomSound(throwSound);
                break;
            case playerSoundType.QuickTrun:
                clip = AudioManager.GetRandomSound(quickTrunSound);
                break;
            case playerSoundType.Stick:
                clip = stickSound;
                break;
            case playerSoundType.WallKick:
                clip = wallKickSound;
                break;
            case playerSoundType.Dash:
                clip = dashSound;
                break;
            case playerSoundType.Cloth:
                clip = AudioManager.GetRandomSound(cloth);
                break;
            case playerSoundType.BigCloth:
                clip = AudioManager.GetRandomSound(bigCloth);
                break;
            case playerSoundType.ShieldLerp:
                clip = shieldLerp;
                break;
            case playerSoundType.Parry:
                clip = parrySound;
                break;
            case playerSoundType.Block:
                clip = shieldBlockSound;
                float pitch = Random.Range(0.8f, 1.2f);
                AudioManager.Instance.Play3DSound(clip, transform.position, "shieldBlockSound", 1, pitch);
                return;
            case playerSoundType.Death:
                clip = deathSound;
                break;
            case playerSoundType.Fall:
                clip = fallSound;
                break;
        }

        if (clip != null)
        {
            AudioManager.Instance.Play3DSound(clip, transform.position);
        }
    }

    AudioClip GetMaterialFootstepSound()
    {
        const float MAX_RAY_DISTANCE = 1.0f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, MAX_RAY_DISTANCE, groundMask);

        if (hit.collider != null)
        {
            int randomIndex;

            if (hit.collider.gameObject.TryGetComponent<FloorMaterial>(out FloorMaterial floor))
            {
                switch (floor.material)
                {
                    case materialEnum.grass:
                        randomIndex = Random.Range(0, grassFootStepSound.Length);
                        return grassFootStepSound[randomIndex];
                    case materialEnum.stone:
                        randomIndex = Random.Range(0, stoneFootStepSound.Length);
                        return stoneFootStepSound[randomIndex];
                    case materialEnum.wood:
                        randomIndex = Random.Range(0, woodFootStepSound.Length);
                        return woodFootStepSound[randomIndex];
                    default:
                        randomIndex = Random.Range(0, grassFootStepSound.Length);
                        return grassFootStepSound[randomIndex];
                }
            }
            else
            {
                randomIndex = Random.Range(0, grassFootStepSound.Length);
                return grassFootStepSound[randomIndex];
            }
        }

        return null;
    }
}
