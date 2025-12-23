using UnityEngine;


public class Train_Inside_CutsceneSound : MonoBehaviour
{
    [Header("øÕ¿Â√¢")]
    public AudioClip glassBreakSound;

    public void PlayGlassBreakSound()
    {
        if (glassBreakSound != null) AudioManager.Instance.Play2DSound(glassBreakSound, "glassBreakSound", 0.5f);
    }
}
