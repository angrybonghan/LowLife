using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DestructibleObjects : MonoBehaviour, I_Destructible
{
    [Header("파괴 설정")]
    public AudioClip[] destructionSound;
    public bool canDistroy = true;

    [Header("부품")]
    public GameObject deadPartPrefab;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void OnAttack()
    {
        Destruction();
    }

    public bool CanDestructible()
    {
        return canDistroy;
    }

    void Destruction()
    {
        if (deadPartPrefab != null) Instantiate(deadPartPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    public void PlayRandomDestructionSound()
    {
        if (destructionSound == null || destructionSound.Length == 0) return;

        int randomIndex = Random.Range(0, destructionSound.Length);

        AudioClip clipToPlay = destructionSound[randomIndex];

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }
}
