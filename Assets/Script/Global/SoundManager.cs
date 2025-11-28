using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    [Header("엔티티 타격음")]
    public AudioClip[] entityHitSound;

    float volume = 1.0f;
    List<GameObject> allBgmPlayer = new List<GameObject>();

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

    public void PlayRandomSoundAtPosition(Vector3 position, AudioClip[] clips, float volumeMultiple = 1f)
    {
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = null;
        int randomIndex = Random.Range(0, clips.Length);
        clip = clips[randomIndex];
        AudioSource.PlayClipAtPoint(clip, position, volume * volumeMultiple);
    }

    public AudioClip GetRandomSound(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;

        int randomIndex = Random.Range(0, clips.Length);
        return clips[randomIndex];
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

    public void PlayRandomClipAtPointWithPitch(Vector3 position, AudioClip[] clips, float pitch = 1.0f)
    {
        if (clips == null || clips.Length == 0) return;

        int randomIndex = Random.Range(0, clips.Length);
        AudioClip clip = clips[randomIndex];

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

    public void PlayLoopBgm(AudioClip clip, float pitch = 1.0f)
    {
        GameObject tempBgmGO = new GameObject("TempBgmAudio");
        allBgmPlayer.Add(tempBgmGO);

        AudioSource audioSource = tempBgmGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.loop = true;

        audioSource.Play();
    }

    public void StopAllBgm()
    {
        foreach (var player in allBgmPlayer)
        {
            Destroy(player);
        }

        allBgmPlayer.Clear();
    }
}
