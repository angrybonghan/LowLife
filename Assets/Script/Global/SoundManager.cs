using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    [Header("효과음 클립")]
    public AudioClip[] entityHitSound;

    private float volume = 1.0f; // 현재 볼륨 (0.0 ~ 1.0)
    private List<AudioSource> allBgmSources = new List<AudioSource>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        foreach (var src in allBgmSources)
        {
            if (src != null)
                src.volume = volume;
        }
        Debug.Log($"[SoundManager] Volume set to {volume}");
    }

    public void IncreaseVolume() => SetVolume(volume + 0.1f);
    public void DecreaseVolume() => SetVolume(volume - 0.1f);

    public void LowerVolumeForESC()
    {
        foreach (var src in allBgmSources)
        {
            if (src != null)
                src.volume = volume * 0.3f;
        }
    }

    public void RestoreVolumeAfterESC()
    {
        foreach (var src in allBgmSources)
        {
            if (src != null)
                src.volume = volume;
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
            AudioSource.PlayClipAtPoint(clipToPlay, position, volume);
        }
    }

    public void PlayClipAtPointWithPitch(Vector3 position, AudioClip clip, float pitch = 1.0f, float volumeMultiple = 1.0f)
    {
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;

        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.spatialBlend = 1.0f;

        audioSource.volume = volume * volumeMultiple;
        audioSource.pitch = pitch;

        audioSource.Play();
        Destroy(tempGO, clip.length / Mathf.Abs(pitch));
    }

    public void PlayRandomClipAtPointWithPitch(Vector3 position, AudioClip[] clips, float pitch = 1.0f, float volumeMultiple = 1.0f)
    {
        if (clips == null || clips.Length == 0) return;

        int randomIndex = Random.Range(0, clips.Length);
        AudioClip clip = clips[randomIndex];

        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;

        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.spatialBlend = 1.0f;

        audioSource.volume = volume * volumeMultiple;
        audioSource.pitch = pitch;

        audioSource.Play();
        Destroy(tempGO, clip.length / Mathf.Abs(pitch));
    }

    public void PlayLoopBgm(AudioClip clip, float pitch = 1.0f, float volumeMultiple = 1.0f)
    {
        GameObject tempBgmGO = new GameObject("TempBgmAudio");
        AudioSource audioSource = tempBgmGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume * volumeMultiple;
        audioSource.pitch = pitch;
        audioSource.loop = true;
        audioSource.Play();

        allBgmSources.Add(audioSource);
    }

    public void StopAllBgm()
    {
        foreach (var src in allBgmSources)
        {
            if (src != null)
                Destroy(src.gameObject);
        }
        allBgmSources.Clear();
    }
}
