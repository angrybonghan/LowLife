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
            AudioManager.instance.PlaySoundAtPosition(soundPosition, soundClip);
        }
        else
        {
            AudioManager.instance.Play2DSound(soundClip);
        }
    }
}
