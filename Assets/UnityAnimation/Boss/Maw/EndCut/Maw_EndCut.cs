using UnityEngine;

public enum MawEndCutSoundType { stuck, smash, headRoll }
public class Maw_EndCut : MonoBehaviour
{
    [Header("¶¥¿¡ ¹ÚÈû")]
    public AudioClip stuckSound;

    [Header("°¡°Ý")]
    public AudioClip smashSound;

    [Header("¸Ó¸® µ¥±¼µ¥±¼")]
    public AudioClip headRollSound;

    public void PlaySound(MawEndCutSoundType type)
    {
        AudioClip clip = null;
        Vector3 camPos = Camera.main.transform.position;

        switch (type)
        {
            case MawEndCutSoundType.stuck:
                clip = stuckSound;
                break;
            case MawEndCutSoundType.smash:
                clip = smashSound;
                break;
            case MawEndCutSoundType.headRoll:
                clip = headRollSound;
                break;
        }

        if (clip != null) AudioManager.instance.PlaySoundAtPosition(camPos, clip, 0.5f);
    }
}
