using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    private static float Name;


    [Header("엔티티 타격음")]
    public AudioClip[] entityHitSound;

    float volume = 1.0f;

    AudioSource AS;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            this.enabled = false;
            return;
        }

        AS = GetComponent<AudioSource>();
    }

    public void PlaySoundAtPosition(Vector3 position, AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }

    public void PlayEntityHitSound(Vector3 position)
    {
        if (entityHitSound == null || entityHitSound.Length == 0) return;

        int randomIndex = Random.Range(0, entityHitSound.Length);

        AudioClip clipToPlay = entityHitSound[randomIndex];

        if (clipToPlay != null)
        {
            PlaySoundAtPosition(position, clipToPlay);
        }
    }
}
