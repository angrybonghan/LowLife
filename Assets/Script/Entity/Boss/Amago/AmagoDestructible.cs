using UnityEngine;

public class AmagoDestructible : MonoBehaviour
{
    [Header("颇鲍 家府")]
    public AudioClip[] destructionSound;

    [Header("单靛颇明")]
    public GameObject deadParticle;

    public void Destroy(Vector3 AttackerPos)
    {
        SoundManager.instance.PlayRandomSoundAtPosition(transform.position, destructionSound);
        if (deadParticle != null) Instantiate(deadParticle, transform.position, transform.localRotation);
        Destroy(gameObject);
    }
    
}
