using UnityEngine;


public class Train_Inside : MonoBehaviour
{
    [Header("øÕ¿Â√¢")]
    public AudioClip glassBreakSound;

    public void PlayGlassBreakSound()
    {
        Vector3 camPos = Camera.main.transform.position;
        if (glassBreakSound != null) AudioManager.instance.PlaySoundAtPosition(camPos, glassBreakSound, 0.5f);
    }
}
