using UnityEngine;

public enum MawEndCutSoundType { stuck }
public class Maw_EndCut : MonoBehaviour
{
    [Header("¶¥¿¡ ¹ÚÈû")]
    public AudioClip stuckSound;

    public void PlaySound(MawEndCutSoundType type)
    {
        AudioClip clip = null;
        Vector3 camPos = Camera.main.transform.position;

        switch (type)
        {
            case MawEndCutSoundType.stuck:
                clip = stuckSound;
                break;
        }

        if (clip != null) SoundManager.instance.PlaySoundAtPosition(camPos, clip, 0.5f);
    }
}
