using System.Collections;
using UnityEngine;

public class FussZone : MonoBehaviour
{
    [Header("말풍선")]
    public DialogueBubble dialogueBubblePrefab;
    public bool isTailLower = true;
    public int maxBubbleCount = 2;

    [Header("범위")]
    public Vector2 bubbleRangeOffset = Vector2.zero;    // 버블 범위 중심 오프셋
    public Vector2 bubbleRangeSize = new Vector2(4.0f, 2.25f); // 범위 (width, height)

    [Header("문자")]
    public string[] fussText;
    public bool isTextRandom = true;

    [Header("시간 설정")]
    public float minFussTime = 1.5f;
    public float maxFussTime = 2f;
    public float maxWaitTime = 0.5f;

    int currentBubbleCount;
    int TextIndex;
    int layerOrder = -32768;

    void Start()
    {
        if (fussText == null || fussText.Length == 0)
        {
            this.enabled = false;
            return;
        }

        StartCoroutine(FussSequence());
    }

    IEnumerator FussSequence()
    {
        while (true == true) // 개고수 코딩법
        {
            SummonBubble();
            if (currentBubbleCount >= maxBubbleCount) yield return new WaitUntil(() => currentBubbleCount < maxBubbleCount);

            yield return new WaitForSeconds(GetRandomFloat(0.1f, maxWaitTime));
        }
    }

    void SummonBubble()
    {
        currentBubbleCount++;
        DialogueBubble newBubble = Instantiate(dialogueBubblePrefab, GetRandomBubblePos(), Quaternion.identity);
        newBubble.SetTailToLower(isTailLower);
        newBubble.SetText(GetRandomFussText());
        newBubble.SetOrderInLayer(layerOrder);
        layerOrder = layerOrder >= 32760 ? -32767 : layerOrder + 2;
        StartCoroutine(DestroyBubbleSequence(GetRandomFloat(minFussTime, maxFussTime), newBubble.gameObject));
    }

    IEnumerator DestroyBubbleSequence(float waitTime, GameObject destroyTarget)
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(destroyTarget);
        currentBubbleCount--;
    }

    Vector2 GetRandomBubblePos()
    {
        Vector2 currentPosition = transform.position;

        float minX = currentPosition.x + bubbleRangeOffset.x - (bubbleRangeSize.x / 2.0f);
        float minY = currentPosition.y + bubbleRangeOffset.y - (bubbleRangeSize.y / 2.0f);
        float randomX = GetRandomFloat(minX, minX + bubbleRangeSize.x);
        float randomY = GetRandomFloat(minY, minY + bubbleRangeSize.y);

        return new Vector2(randomX, randomY);
    }

    string GetRandomFussText()
    {
        string targetText = fussText[TextIndex];
        TextIndex = isTextRandom ? GetRandomInt(0, fussText.Length) : TextIndex >= fussText.Length - 1 ? 0 : TextIndex + 1;
        // 삼항연산 안에 삼항연산이 있는거임.
        // 그럼 이 삼항연산이 삼항연산을 임신했다고 볼 수 있는 거임???????

        return targetText;
    }

    float GetRandomFloat(float num1, float num2)
    {
        if (num1 == num2) return num1;
        else if (num1 < num2) return Random.Range(num1, num2);
        else return Random.Range(num2, num1);
    }

    int GetRandomInt(int num1, int num2)
    {
        if (num1 == num2) return num1;
        else if (num1 < num2) return Random.Range(num1, num2);
        else return Random.Range(num2, num1);
    }


    private void OnDrawGizmosSelected()
    {
        // 1. 기즈모 색상 설정
        Gizmos.color = Color.blue;
        
        Vector3 center = transform.position + (Vector3)bubbleRangeOffset;
        Gizmos.DrawWireCube(center, (Vector3)bubbleRangeSize);
    }
}
