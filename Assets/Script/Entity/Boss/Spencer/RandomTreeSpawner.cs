using System.Collections;
using UnityEngine;

public class RandomTreeSpawner : MonoBehaviour
{
    public static RandomTreeSpawner instance;

    [Header("Àü¸é")]
    public float minSpawnIntervalFront = 1f;
    public float maxSpawnIntervalFront = 5f;
    public float minFrontTreeScale = 0.8f;
    public float maxFrontTreeScale = 1.5f;
    public Vector2 frontTreeSpawnPoint;
    public Sprite[] frontTreeSprites;
    public int sortingOrderFront = 3;

    [Header("ÈÄ¸é")]
    public float minSpawnIntervalBack = 3f;
    public float maxSpawnIntervalBack = 7f;
    public float minBackTreeScale = 1.0f;
    public float maxBackTreeScale = 2.0f;
    public Vector2 backTreeSpawnPoint;
    public Sprite[] backTreeSprites;
    public int sortingOrderBack = -3;

    [Header("°øÅë")]
    public float endXPosition = -40f;
    public float moveSpeed = 300f;

    [HideInInspector] public bool canSpawn = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(FrontTreeSpawn());
        StartCoroutine(BackTreeSpawn());
    }

    IEnumerator FrontTreeSpawn()
    {
        while (true)
        {
            if (!canSpawn)
            {
                yield return new WaitUntil(() => canSpawn);
            }

            yield return new WaitForSeconds(GetRandom(minSpawnIntervalFront, maxSpawnIntervalFront));
            SummonTree(true);
        }
    }

    IEnumerator BackTreeSpawn()
    {
        while (true)
        {
            if (!canSpawn)
            {
                yield return new WaitUntil(() => canSpawn);
            }

            yield return new WaitForSeconds(GetRandom(minSpawnIntervalBack, maxSpawnIntervalBack));
            SummonTree(false);
        }
    }

    void SummonTree(bool isFront)
    {
        Vector2 summonPos = isFront ? frontTreeSpawnPoint : backTreeSpawnPoint;
        Sprite[] targetSprites = isFront ? frontTreeSprites : backTreeSprites;
        int sortingOrder = isFront ? sortingOrderFront : sortingOrderBack;
        float size = isFront ? GetRandom(minFrontTreeScale, maxFrontTreeScale) : GetRandom(minBackTreeScale, maxBackTreeScale);
        // »ï»ï»ï»ïÇ×Ç×Ç×Ç×¿¬¿¬¤·¿¬¿¬¤µ˜Î»ñ¤¥»ê¤·»ó???????!?!?!?!?!?!?

        GameObject go = new GameObject("TrainTree");
        TrainTreeSprite tree = go.AddComponent<TrainTreeSprite>();
        go.transform.position = summonPos;

        tree.SetRandomSprite(targetSprites, sortingOrder);
        tree.moveSpeed = moveSpeed;
        tree.endXPosition = endXPosition;
        tree.SetSize(size);
    }

    float GetRandom(float a, float b)
    {
        if (a == b) return a;
        else if (b > a) return Random.Range(a, b);
        else return Random.Range(b, a);
    }
}
