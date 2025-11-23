using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    [Header("설정")]
    public float maxRange = 2;
    public int maxParticleCount = 5;
    public int minParticleCount = 3;

    [Header("이펙트")]
    public GameObject[] particlePrefabs;

    [Header("오브젝트 파괴")]
    public bool canDistroyObject = true;


    void Start()
    {
        int particleCount = GetRandomInt(minParticleCount, maxParticleCount);

        if (particlePrefabs != null && particlePrefabs.Length != 0 && particleCount > 0)
        {
            for (int i = 0; i < particleCount; i++)
            {
                int randomIndex = GetRandomInt(0, particlePrefabs.Length - 1);
                GameObject prefabToSpawn = particlePrefabs[randomIndex];
                Vector3 randomPosition = GetRandomPositionInCircle(transform.position, maxRange);

                Instantiate(prefabToSpawn, randomPosition, Quaternion.identity);
            }
        }

        if (canDistroyObject) Destroy(gameObject);
    }

    Vector3 GetRandomPositionInCircle(Vector3 center, float range)
    {
        if (maxRange == 0)
        {
            return transform.position;
        }

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
