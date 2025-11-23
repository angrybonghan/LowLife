using UnityEngine;

public class DestructibleObjects : MonoBehaviour, I_Destructible
{
    [Header("파괴 설정")]
    public AudioClip[] destructionSound;
    public bool canDistroy = true;

    [Header("부품")]
    public GameObject deadPartPrefab;

    public void OnAttack()
    {
        PlayRandomDestructionSound();
        Destruction();
    }

    public bool CanDestructible()
    {
        return canDistroy;
    }

    void Destruction()
    {
        if (deadPartPrefab != null) Instantiate(deadPartPrefab, transform.position, transform.localRotation);
        
        Destroy(gameObject);
    }

    public void PlayRandomDestructionSound()
    {
        if (destructionSound == null || destructionSound.Length == 0) return;

        int randomIndex = Random.Range(0, destructionSound.Length);

        AudioClip clipToPlay = destructionSound[randomIndex];

        if (clipToPlay != null)
        {
            SoundManager.instance.PlaySoundAtPosition(transform.position, clipToPlay);
        }
    }
}
