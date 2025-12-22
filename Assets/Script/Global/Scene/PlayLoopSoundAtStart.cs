using UnityEngine;

public class PlayLoopSoundAtStart : MonoBehaviour
{
    [Header("소리")]
    public AudioClip soundClip;

    [Header("설정")]
    public string loopSoundName= "LoopBGM";
    public bool is3D = true;
    public Vector3 soundPosition;

    void Start()
    {
        if (soundClip == null) return;

        if (is3D)
        {
            AudioManager.Instance.Play3DSound(soundClip, soundPosition, loopSoundName, 1, 1, true);
        }
        else
        {
            AudioManager.Instance.Play2DSound(soundClip, loopSoundName, 1, 1, true);
        }
    }
}
