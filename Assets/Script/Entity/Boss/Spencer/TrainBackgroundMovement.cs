using UnityEngine;

public class TrainBackgroundMovement : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 5f;

    [Header("위치")]
    public float endXPosition = -20f;

    [Header("스프라이트 배경")]
    public float spriteInterval = 10f;
    public float spriteSize = 3f;

    [Header("스프라이트")]
    public float spriteScale = 3f;
    public Sprite[] backgroundSprites;

    [Header("시작")]
    public int startSpriteCount = 5;

    int spriteCount = 0;
    int currentSpriteIndex = 0;
    Vector2 SpriteLocalposition;

    void Start()
    {
        spriteCount = backgroundSprites.Length;
        StartSet();
    }

    void Update()
    {
        Vector2 move = new Vector2(-moveSpeed * Time.deltaTime, 0f);
        transform.position += (Vector3)move;
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
        TrainBackground bg = go.AddComponent<TrainBackground>();
        go.transform.SetParent(transform);
        go.transform.localPosition = SpriteLocalposition;

        bg.SetSize(spriteScale);
        bg.SetSprite(backgroundSprites[currentSpriteIndex]);
        bg.endXPosition = endXPosition;
        bg.manager = this;

        SetNextSpritePosition();
    }

    void SetNextSpritePosition()
    {
        SpriteLocalposition = new Vector2(SpriteLocalposition.x + spriteInterval, SpriteLocalposition.y);
        currentSpriteIndex++;
        if (currentSpriteIndex >= spriteCount)
        {
            currentSpriteIndex = 0;
        }
    }
}
