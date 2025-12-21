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
            AudioManager.instance.PlayLoopSoundAtPosition(soundPosition, soundClip, loopSoundName);
        }
        else
        {
            AudioManager.instance.PlayLoopBgm(soundClip, loopSoundName);
        }
    }
}
