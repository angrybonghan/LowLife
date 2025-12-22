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
            AudioManager.Instance.Play3DSound(soundClip, soundPosition);
        }
        else
        {
            AudioManager.Instance.Play2DSound(soundClip);
        }
    }
}
