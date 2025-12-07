using UnityEngine;

public class TrainBackground : MonoBehaviour
{
    public float endXPosition = -20f;
    public TrainBackgroundMovement manager;

    void Update()
    {
        if (transform.position.x <= endXPosition)
        {
            Destroy(gameObject);
            if (manager != null) manager.SpawnNewSprite();
        }
    }

    public void SetSprite(Sprite sprite)
    {
        SpriteRenderer sr;

        if (!TryGetComponent<SpriteRenderer>(out sr))
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        sr.sprite = sprite;
    }

    public void SetSize(float size)
    {
        transform.localScale = new Vector2(size, size);
    }
}
