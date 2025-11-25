using TMPro;
using UnityEngine;

public class DialogueBubble : MonoBehaviour
{
    public TextMeshPro dialogueText;
    public GameObject bubbleBody;
    public GameObject upperTail;
    public GameObject lowerTail;

    public float bubbleOffsetLimit = 1.0f;

    private void Awake()
    {
        if (bubbleOffsetLimit < 0)
        {
            bubbleOffsetLimit = Mathf.Abs(bubbleOffsetLimit);
        }
    }

    public void SetTailToLower(bool isLower)
    {
        if (isLower)
        {
            upperTail.SetActive(false);
            lowerTail.SetActive(true);
        }
        else
        {
            upperTail.SetActive(true);
            lowerTail.SetActive(false);
        }
    }

    public void SetBubbleOffset(float offsetDistance)
    {
        bubbleBody.transform.localPosition = new Vector3(
                Mathf.Clamp(offsetDistance, -bubbleOffsetLimit, bubbleOffsetLimit),
                0,
                0
            );
    }

    public void SetText(string text)
    {
        dialogueText.text = text;
    }

    public void SetPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    public void SetOrderInLayer(int value)
    {
        SetGameObjectOrder(bubbleBody, value);

        dialogueText.sortingOrder = value + 1;
        SetGameObjectOrder(upperTail, value + 1);
        SetGameObjectOrder(lowerTail, value + 1);
    }

    void SetGameObjectOrder(GameObject go, int order)
    {
        if (go != null)
        {
            SpriteRenderer spriteRenderer = go.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = order;
            }
            else
            {
                Debug.LogWarning(go.name + "에 SpriteRenderer 컴포넌트 없음");
            }
        }
    }
}