using UnityEngine;

public class ExclamationMarkHandler_Old : MonoBehaviour
{
    [Header("참조 OBJ")]
    public Transform sizeObj;
    public SpriteRenderer valueSpriteRenderer;

    [Header("색상")]
    public Color MaxValueColor = Color.red;
    public Color MinValueColor = Color.green;


    public void SetPos(Vector2 newPos)
    {
        transform.position = newPos;
    }

    public void SetGaugeValue(float value)
    {
        value = Mathf.Clamp01(value);

        float originalXSize = sizeObj.transform.localScale.x;
        sizeObj.transform.localScale = new Vector2(originalXSize, value);
        UpdateColor(value);
    }

    private void UpdateColor(float value)
    {
        float clampedValue = Mathf.Clamp01(value);

        Color targetColor = Color.Lerp(MinValueColor, MaxValueColor, clampedValue);

        valueSpriteRenderer.color = targetColor;
    }
}
