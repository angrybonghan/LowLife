using UnityEngine;

public class End_Trainsound : MonoBehaviour
{
    [Header("기차 소리")]
    public AudioClip trainSound;

    const string soundName = "TrainSoundLoop";

    void Start()
    {
        AudioManager.Instance.Play2DSound(trainSound, soundName);
    }

    public void StopTrainSound()
    {
        AudioManager.Instance.StopSound(soundName, 2);
    }
}
