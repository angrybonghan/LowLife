using UnityEngine;

public class TrainBackgroundSprite : MonoBehaviour
{
    [HideInInspector] public float endXPosition = -20f;
    [HideInInspector] public TrainBackgroundMovement manager;
    [HideInInspector] public TrainTunnelMovement tunnelManager;
    [HideInInspector] public bool isTunnel = false;

    void Update()
    {
        if (transform.position.x <= endXPosition)
        {
            Destroy(gameObject);
            if (isTunnel)
            {
                if (tunnelManager != null) tunnelManager.SpriteDeleted();
            }
            else
            {
                if (manager != null) manager.SpawnNewSprite();
            }
            
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

    public void SetSize(float size)
    {
        transform.localScale = new Vector2(size, size);
    }
}
