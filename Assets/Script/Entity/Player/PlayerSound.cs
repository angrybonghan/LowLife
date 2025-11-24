using UnityEngine;

public enum playerSoundType { FootStep, Throw, Jump, Dash, Parry, QuickTrun, Stick, WallKick, BigCloth, Cloth };

public class PlayerSound : MonoBehaviour
{
    [Header("πﬂº“∏Æ")]
    public LayerMask groundMask;
    public AudioClip[] grassFootStepSound;
    public AudioClip[] stoneFootStepSound;
    public AudioClip[] woodFootStepSound;

    [Header("ƒ¸ ≈œ")]
    public AudioClip[] quickTrunSound;

    [Header("πÊ∆– ≈ı√¥")]
    public AudioClip[] throwSound;

    [Header("∫Æ ∞¸∑√")]
    public AudioClip stickSound;
    public AudioClip wallKickSound;

    [Header("¥ÎΩ¨")]
    public AudioClip dashSound;

    [Header("ø  ∆ﬁ∑∞¿”")]
    public AudioClip[] cloth;
    public AudioClip[] bigCloth;


    public void PlaySound(playerSoundType type)
    {
        AudioClip clip = null;
        int randomIndex = 0;

        switch (type)
        {
            case playerSoundType.FootStep:
                clip = GetMaterialFootstepSound();
                break;
            case playerSoundType.Throw:
                randomIndex = Random.Range(0, throwSound.Length);
                clip = throwSound[randomIndex];
                break;
            case playerSoundType.QuickTrun:
                randomIndex = Random.Range(0, quickTrunSound.Length);
                clip = quickTrunSound[randomIndex];
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
                randomIndex = Random.Range(0, cloth.Length);
                clip = cloth[randomIndex];
                break;
            case playerSoundType.BigCloth:
                randomIndex = Random.Range(0, bigCloth.Length);
                clip = bigCloth[randomIndex];
                break;
        }

        if (clip != null)
        {
            SoundManager.instance.PlaySoundAtPosition(transform.position, clip);
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
