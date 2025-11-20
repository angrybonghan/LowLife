using UnityEngine;

public class ParticleShooter : MonoBehaviour
{
    [Header("파티클 범위, 수 설정")]
    public float maxRange = 2;
    public int maxParticleCount = 5;
    public int minParticleCount = 3;

    [Header("파티클 발사")]
    public float spreadAngle = 10;

    [Header("시간 설정")]
    public bool haveRetentionTime = true;   // 유지 시간을 설정할지 여부, false면 무한히 분사함.
    public float retentionTime = 3;

    [Header("이펙트")]
    public GameObject[] particlePrefabs;

    float currentRetentionTime;

    private void FixedUpdate()
    {
        ShootParticles();

        if (haveRetentionTime)
        {
            currentRetentionTime += Time.fixedDeltaTime;
            if (currentRetentionTime >= retentionTime)
            {
                Destroy(gameObject);
            }
        }
        
    }

    void ShootParticles()
    {
        int particleCount = GetRandomInt(minParticleCount, maxParticleCount);

        Vector2 direction = transform.right;

        for (int i = 0; i < particleCount; i++)
        {
            int randomIndex = GetRandomInt(0, particlePrefabs.Length - 1);
            GameObject prefabToSpawn = particlePrefabs[randomIndex];
            DirectionalParticle newParticle = Instantiate(prefabToSpawn, GetRandomPositionInCircle(transform.position, maxRange), Quaternion.identity).GetComponent<DirectionalParticle>();

            newParticle.initialDirection = direction;
            newParticle.spreadAngle = spreadAngle;
        }
    }


    Vector3 GetRandomPositionInCircle(Vector3 center, float range)
    {
        Vector2 randomCircle = Random.insideUnitCircle * range;
        Vector3 randomPosition = center + new Vector3(randomCircle.x, randomCircle.y, 0f);

        return randomPosition;
    }

    int GetRandomInt(int min, int max)
    {
        if (max == min)
        {
            return max;
        }
        else if (min < max)
        {
            return Random.Range(min, max + 1);
        }
        else
        {
            return Random.Range(max, min + 1);
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxRange);
    }
}
