using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }
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

    public void PlaySoundAtPosition(Vector3 position, AudioClip clip, float volume = 1.0f)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }
    }
}
