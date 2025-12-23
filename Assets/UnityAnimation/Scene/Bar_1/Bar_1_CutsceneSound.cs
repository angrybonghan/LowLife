using UnityEngine;

public enum Bar_1_SoundType { firing, block }

public class Bar_1_CutsceneSound : MonoBehaviour
{
    [Header("√—")]
    public AudioClip gunShotSound;

    [Header("πÊ∆–")]
    public AudioClip shieldBlockSound;


    public void PlaySound(Bar_1_SoundType type)
    {
        AudioClip clip = null;

        switch (type)
        {
            case Bar_1_SoundType.firing:
                clip = gunShotSound;
                break;
            case Bar_1_SoundType.block:
                clip = shieldBlockSound;
                break;
        }

        if (clip != null) AudioManager.Instance.Play2DSound(clip, "", 0.5f);
    }
}
