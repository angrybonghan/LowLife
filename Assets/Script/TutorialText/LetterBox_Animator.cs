using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LetterBox_Animator : MonoBehaviour
{
    [Header("À§Ä¡")]
    public float enableYPos = 0f;
    public float disableYPos = 0f;
    public bool isEnable = false;

    private Image img;
    private Coroutine colorChangeCoroutine;
    private Coroutine posChangeCoroutine;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    public void SetSprite(Sprite sprite)
    {
        img.sprite = sprite;
    }

    public void SetColor(Color targetColor, float duration)
    {
        if (colorChangeCoroutine != null)
        {
            StopCoroutine(colorChangeCoroutine);
        }

        colorChangeCoroutine = StartCoroutine(ChangeColorCoroutine(targetColor, duration));
    }
    
    IEnumerator ChangeColorCoroutine(Color targetColor, float duration)
    {
        if (duration > 0)
        {
            Color startColor = img.color;
            float timeElapsed = 0f;

            while (timeElapsed < duration)
            {
                img.color = Color.Lerp(startColor, targetColor, timeElapsed / duration);

                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        img.color = targetColor;
        colorChangeCoroutine = null;
    }

    public void SetLetterBoxEnable(bool isEnable, float duration)
    {
        if (isEnable != this.isEnable)
        {
            this.isEnable = isEnable;
        }
        else return;

        if (posChangeCoroutine != null) StopCoroutine(posChangeCoroutine);
        float targetY = isEnable ? enableYPos : disableYPos;
        posChangeCoroutine = StartCoroutine(MoveVerticalTo(targetY, duration));
    }

    public void ToggleBoxEnable(float duration)
    {
        isEnable = !isEnable;

        if (posChangeCoroutine != null) StopCoroutine(posChangeCoroutine);
        float targetY = isEnable ? enableYPos : disableYPos;
        posChangeCoroutine = StartCoroutine(MoveVerticalTo(targetY, duration));
    }


    IEnumerator MoveVerticalTo(float targetY, float duration)
    {
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos;
        targetPos.y = targetY;

        if (duration > 0)
        {
            float timeElapsed = 0f;
            
            while (timeElapsed < duration)
            {
                transform.position = Vector2.Lerp(startPos, targetPos, timeElapsed / duration);

                timeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        transform.position = targetPos;
        posChangeCoroutine = null;
    }
}
