using UnityEngine;

public class TrainTunnelSpeedLine : MonoBehaviour
{
    [Header("Y 범위")]
    public float verticalRange;

    [Header("색")]
    public Color minColor;
    public Color maxColor;

    [Header("이동")]
    public Vector2 moveDirection;
    public float moveSpeed;

    [Header("크기")]
    public Vector2 minSize;
    public Vector2 maxSize;

    [Header("끝")]
    public float destroyXPosition;


    void Start()
    {
        transform.position = (Vector2)transform.position + Vector2.up * Random.Range(-verticalRange, verticalRange);
        moveDirection = moveDirection.normalized;
        SetRandomColor();
        SetRandomSize();
    }

    private void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        if (transform.position.x <= destroyXPosition)
        {
            Destroy(gameObject);
        }
    }

    void SetRandomColor()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.Lerp(minColor, maxColor, Random.value);
    }

    void SetRandomSize()
    {
        float randomScaleX = Random.Range(minSize.x, maxSize.x);
        float randomScaleY = Random.Range(minSize.y, maxSize.y);
        transform.localScale = new Vector2(randomScaleX, randomScaleY);
    }
}
