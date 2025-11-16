using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [Header("설정")]
    public GameObject prefabToSpawn;    // 생성할 프리팹
    public int numberOfPrefabs = 10;    // 프리팹 수
    public float spawnRadius = 5f;

    void Start()
    {
        SpawnPrefabsInCircle();
        Destroy(gameObject);
    }

    private void SpawnPrefabsInCircle()
    {
        if (prefabToSpawn == null)
        {
            return;
        }

        for (int i = 0; i < numberOfPrefabs; i++)
        {
            Vector2 randomCirclePoint = Random.insideUnitCircle * spawnRadius;

            Vector3 spawnPosition = new Vector3(
                transform.position.x + randomCirclePoint.x,
                transform.position.y + randomCirclePoint.y,
                transform.position.z
            );

            Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}