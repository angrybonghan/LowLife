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
}