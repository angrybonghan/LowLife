using UnityEngine;

public enum LumenSoundType { Fire, Reload1, Reload2, Eject, Aiming }
public class LumenSound : MonoBehaviour
{
    [Header("발사")]
    public AudioClip fireSound;

    [Header("재장전")]
    public AudioClip reloadSound1;
    public AudioClip reloadSound2;
    public AudioClip ejectSound;

    [Header("조준")]
    public AudioClip aimingSound;

    public void PlaySound(LumenSoundType type)
    {
        AudioClip clip = GetSound(type);
        SoundManager.instance.PlaySoundAtPosition(transform.position, clip);
    }

    AudioClip GetSound(LumenSoundType type)
    {
        switch (type)
        {
            case LumenSoundType.Fire:
                return fireSound;
            case LumenSoundType.Reload1:
                return reloadSound1;
            case LumenSoundType.Reload2:
                return reloadSound2;
            case LumenSoundType.Eject:
                return ejectSound;
            case LumenSoundType.Aiming:
                return aimingSound;
            default:
                return null;
        }
    }

}
