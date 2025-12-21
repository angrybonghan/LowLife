using UnityEngine;

public enum Bar_1_SoundType { firing, block }

public class Bar_1_Cut : MonoBehaviour
{
    [Header("√—")]
    public AudioClip gunShotSound;

    [Header("πÊ∆–")]
    public AudioClip shieldBlockSound;


    public void PlaySound(Bar_1_SoundType type)
    {
        AudioClip clip = null;
        Vector3 camPos = Camera.main.transform.position;

        switch (type)
        {
            case Bar_1_SoundType.firing:
                clip = gunShotSound;
                break;
            case Bar_1_SoundType.block:
                clip = shieldBlockSound;
                break;
        }

        if (clip != null) AudioManager.instance.PlaySoundAtPosition(camPos, clip, 0.5f);
    }
}
