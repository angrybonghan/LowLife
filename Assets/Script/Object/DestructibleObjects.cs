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
        AudioManager.Instance.PlayRandom3DSound(destructionSound, transform.position);
        if (deadPartPrefab != null) Instantiate(deadPartPrefab, transform.position, transform.localRotation);
        Destroy(gameObject);
    }

    public bool CanDestructible()
    {
        return canDistroy;
    }
}
