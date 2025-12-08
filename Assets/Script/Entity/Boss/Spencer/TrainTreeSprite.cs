using UnityEngine;

public class TrainTreeSprite : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float endXPosition = -20f;

    private void Update()
    {
        Vector2 move = new Vector2(-moveSpeed * Time.deltaTime, 0f);
        transform.position += (Vector3)move;

        if (transform.position.x <= endXPosition)
        {
            Destroy(gameObject);
        }
    }

    public void SetSprite(Sprite sprite, int sortLayer)
    {
        SpriteRenderer sr;

        if (!TryGetComponent<SpriteRenderer>(out sr))
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        sr.sortingOrder = sortLayer;
        sr.sprite = sprite;
    }

    public void SetRandomSprite(Sprite[] sprites, int sortLayer)
    {
        if (sprites == null || sprites.Length == 0)
        {
            return;
        }

        SpriteRenderer sr;

        if (!TryGetComponent<SpriteRenderer>(out sr))
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        sr.sortingOrder = sortLayer;
        int randomIndex = Random.Range(0, sprites.Length);
        sr.sprite = sprites[randomIndex];
    }

    public void SetSize(float size)
    {
        transform.localScale = new Vector2(size, size);
    }
}
