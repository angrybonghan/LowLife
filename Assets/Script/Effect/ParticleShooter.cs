using UnityEngine;

public class ParticleShooter : MonoBehaviour
{
    [Header("파티클 범위, 수 설정")]
    public float maxRange = 2;
    public int maxParticleCount = 5;
    public int minParticleCount = 3;

    [Header("파티클 발사")]
    public float spreadAngle = 10;
    public bool showGizmo = true;

    [Header("시간 설정")]
    public bool haveRetentionTime = true;   // 유지 시간을 설정할지 여부, false면 무한히 분사함.
    public float retentionTime = 3;

    [Header("이펙트")]
    public GameObject[] particlePrefabs;

    [Header("오브젝트 파괴 여부")]
    public bool canDestroyObj;

    float currentRetentionTime;

    private void FixedUpdate()
    {
        ShootParticles();

        if (haveRetentionTime)
        {
            currentRetentionTime += Time.fixedDeltaTime;
            if (currentRetentionTime >= retentionTime)
            {
                if (canDestroyObj) Destroy(gameObject);
                else this.enabled = false;
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

            newParticle.initialDirection = transform.right;

            if (-360 > spreadAngle || spreadAngle > 360) newParticle.spreadAngle = 360;
            else newParticle.spreadAngle = spreadAngle;
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
        if (maxRange < 0) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxRange);

        if (-360 < spreadAngle && spreadAngle < 360 && showGizmo)
        {
            Vector3 centerDirection = transform.right;

            Quaternion rotationLeft = Quaternion.AngleAxis(90f, Vector3.forward);
            Vector3 leftDirection = rotationLeft * centerDirection;

            Quaternion rotationRight = Quaternion.AngleAxis(-90f, Vector3.forward);
            Vector3 rightDirection = rotationRight * centerDirection;

            Vector3 leftOffsetPosition = transform.position + leftDirection * maxRange;
            Vector3 rightOffsetPosition = transform.position + rightDirection * maxRange;

            float halfAngle = spreadAngle * 0.5f;
            Quaternion rotationUp = Quaternion.AngleAxis(halfAngle, Vector3.forward);
            Quaternion rotationDown = Quaternion.AngleAxis(-halfAngle, Vector3.forward);

            Vector3 spreadDirectionUp;
            Vector3 spreadDirectionDown;

            Gizmos.color = Color.cyan;

            if (spreadAngle > 0)
            {
                spreadDirectionUp = leftOffsetPosition + rotationUp * centerDirection * (maxRange + 2f);
                spreadDirectionDown = rightOffsetPosition + rotationDown * centerDirection * (maxRange + 2f);

                Gizmos.DrawLine(leftOffsetPosition, spreadDirectionUp);
                Gizmos.DrawLine(rightOffsetPosition, spreadDirectionDown);
            }
            else
            {
                spreadDirectionUp = rightOffsetPosition + rotationUp * centerDirection * (maxRange + 2f);
                spreadDirectionDown = leftOffsetPosition + rotationDown * centerDirection * (maxRange + 2f);

                Gizmos.DrawLine(rightOffsetPosition, spreadDirectionUp);
                Gizmos.DrawLine(leftOffsetPosition, spreadDirectionDown);
            }

            if (spreadAngle > 180 || -180 > spreadAngle)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(spreadDirectionUp, spreadDirectionDown);
            }
            else
            {
                Gizmos.DrawLine(spreadDirectionUp, spreadDirectionDown);
            }
            
        }
    }
}
