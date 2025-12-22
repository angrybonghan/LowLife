using UnityEngine;

public enum SpencerEndCutSoundType { firing, teleport, spiderWeb , pickUp, parry }

public class Spencer_EndCutsceneSound : MonoBehaviour
{
    [Header("총 관련")]
    public AudioClip firingSound;
    public AudioClip pickUp;

    [Header("텔레포트")]
    public AudioClip teleport;

    [Header("거미줄")]
    public AudioClip spiderWebSound;

    [Header("패링")]
    public AudioClip parrySound;


    public void PlaySound(SpencerEndCutSoundType type)
    {
        AudioClip clip = null;

        switch (type)
        {
            case SpencerEndCutSoundType.firing:
                clip = firingSound;
                break;
            case SpencerEndCutSoundType.teleport:
                clip = teleport;
                break;
            case SpencerEndCutSoundType.spiderWeb:
                clip = spiderWebSound;
                break;
            case SpencerEndCutSoundType.pickUp:
                clip = pickUp;
                break;
            case SpencerEndCutSoundType.parry:
                clip = parrySound;
                break;
        }

        if (clip != null) AudioManager.Instance.Play2DSound(clip, "Spencer_EndCutsceneSound", 0.5f);
    }
}
