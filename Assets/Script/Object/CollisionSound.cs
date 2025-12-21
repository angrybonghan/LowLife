using UnityEngine;

public class CollisionSound : MonoBehaviour
{
    [Header("¼Ò¸®")]
    public AudioClip[] sounds;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        AudioManager.instance.PlayRandomSoundAtPosition(transform.position, sounds);
    }
}
