using UnityEngine;

public enum AudioLibrarySoundType { EntityHit }

public class AudioLibrary : MonoBehaviour
{
    public static AudioLibrary Instance { get; private set; }

    public AudioClip[] EntityHitSound;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            this.enabled = false;
            return;
        }
    }

    public void PlaySound(AudioLibrarySoundType type, Vector3 pos)
    {
        AudioClip clip = GetAudioClipByAudioLibrarySoundType(type);
        if (clip == null) return;

        AudioManager.Instance.Play3DSound(clip, pos);
    }

    AudioClip GetAudioClipByAudioLibrarySoundType(AudioLibrarySoundType type)
    {
        switch (type)
        {
            case AudioLibrarySoundType.EntityHit:
                return AudioManager.GetRandomSound(EntityHitSound);
        }

        return null;
    }
}
