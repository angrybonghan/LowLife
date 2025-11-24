using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    private static float Name;


    [Header("엔티티 타격음")]
    public AudioClip[] entityHitSound;

    float volume = 1.0f;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            this.enabled = false;
            return;
        }

    }

    public void PlaySoundAtPosition(Vector3 position, AudioClip clip, float volumeMultiple = 1f)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, volume * volumeMultiple);
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

    public void PlayClipAtPointWithPitch(Vector3 position, AudioClip clip, float pitch = 1.0f)
    {
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;

        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.spatialBlend = 1.0f;

        audioSource.volume = volume;
        audioSource.pitch = pitch;

        audioSource.Play();
        Destroy(tempGO, clip.length / Mathf.Abs(pitch));
    }
}
