using UnityEngine;
using UnityEngine.UIElements;

public class PlayerSound : MonoBehaviour
{
    public enum soundType { FootStep, Throw ,Jump,Dash,Parry};

    [Header("惯家府")]
    public AudioClip[] footStepSound;

    [Header("规菩 捧么")]
    public AudioClip[] throwSound;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void PlaySound(soundType type)
    {
        AudioClip clip = null;
        int randomIndex = 0;

        switch (type)
        {
            case soundType.FootStep:
                randomIndex = Random.Range(0, footStepSound.Length);
                clip = footStepSound[randomIndex];
                break;
            case soundType.Throw:
                randomIndex = Random.Range(0, throwSound.Length);
                clip = throwSound[randomIndex];
                break;
        }

        if (clip != null)
        {
            SoundManager.instance.PlaySoundAtPosition(transform.position, clip);
        }
    }
}
