using UnityEngine;

public class PlaySoundAtStart : MonoBehaviour
{
    [Header("소리")]
    public AudioClip soundClip;

    [Header("설정")]
    public bool is3D = true;
    public Vector3 soundPosition;

    void Start()
    {
        if (soundClip == null) return;

        if (is3D)
        {
            SoundManager.instance.PlaySoundAtPosition(soundPosition, soundClip);
        }
        else
        {
            SoundManager.instance.Play2DSound(soundClip);
        }
    }
}
