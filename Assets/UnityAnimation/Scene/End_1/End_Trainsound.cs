using UnityEngine;

public class End_Trainsound : MonoBehaviour
{
    [Header("기차 소리")]
    public AudioClip trainSound;

    const string soundName = "TrainSoundLoop";

    void Start()
    {
        SoundManager.instance.PlayLoopBgm(trainSound, soundName);
    }

    public void StopTrainSound()
    {
        SoundManager.instance.StopSound(soundName);
    }
}
