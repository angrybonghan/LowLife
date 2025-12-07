using UnityEngine;

public class TrainBackgroundMovement : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 5f;

    [Header("위치")]
    public float endXPosition = -20f;

    [Header("스프라이트 배경")]
    public float spriteInterval = 10f;

    [Header("스프라이트")]
    public float spriteScale = 3f;
    public Sprite[] backgroundSprites;

    [Header("시작")]
    public int startSpriteCount = 5;

    int spriteCount = 0;
    int currentSpriteIndex = 0;
    const float resetDistance = 3000f;
    Vector2 spriteLocalposition;

    void Start()
    {
        spriteCount = backgroundSprites.Length;
        StartSet();
    }

    void Update()
    {
        Vector2 move = new Vector2(-moveSpeed * Time.deltaTime, 0f);
        transform.position += (Vector3)move;

        if (transform.position.x <= -resetDistance)
        {
            float overshoot = transform.position.x;
            transform.position = new Vector3(0, 0, transform.position.z);

            foreach (Transform child in transform)
            {
                child.localPosition += new Vector3(overshoot, 0, 0);
            }

            spriteLocalposition += new Vector2(overshoot, 0f);
        }
    }

    void StartSet()
    {
        for (int i = 0; i < startSpriteCount; i++)
        {
            SpawnNewSprite();
        }
    }

    public void SpawnNewSprite()
    {
        GameObject go = new GameObject("TrainBackground");
        TrainBackgroundSprite bg = go.AddComponent<TrainBackgroundSprite>();
        go.transform.SetParent(transform);
        go.transform.localPosition = spriteLocalposition;

        bg.SetSize(spriteScale);
        bg.SetSprite(backgroundSprites[currentSpriteIndex], -100);
        bg.endXPosition = endXPosition;
        bg.isTunnel = false;
        bg.manager = this;

        SetNextSpritePosition();
    }

    void SetNextSpritePosition()
    {
        spriteLocalposition = new Vector2(spriteLocalposition.x + spriteInterval, spriteLocalposition.y);
        currentSpriteIndex++;
        if (currentSpriteIndex >= spriteCount)
        {
            currentSpriteIndex = 0;
        }
    }
}
