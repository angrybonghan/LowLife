using UnityEngine;

public enum ShamblerSoundType { Beep, Explosion }
public class ShamblerSound : MonoBehaviour
{
    [Header("¼Ò¸®")]
    public AudioClip beepSound;
    public AudioClip[] explosionSound;

    public void PlaySound(ShamblerSoundType type)
    {
        AudioClip clip = GetSound(type);
        AudioManager.Instance.Play3DSound(clip, transform.position);
    }

    AudioClip GetSound(ShamblerSoundType type)
    {
        switch (type)
        {
            case ShamblerSoundType.Beep:
                return beepSound;
            case ShamblerSoundType.Explosion:
                int index = Random.Range(0, explosionSound.Length);
                return explosionSound[index];
            default:
                return null;
        }
    }
}
