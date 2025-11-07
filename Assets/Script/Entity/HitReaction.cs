using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class HitReaction : MonoBehaviour, I_Destructible
{
    [Header("리액션")]
    public float reactionlength = 2.5f;
    public string[] reactionText;
    public bool isTextRandom = true;

    [Header("말풍선")]
    public DialogueBubble dialogueBubblePrefab;
    public bool isTailLower = true;
    public Vector2 bubbleOffset;

    [Header("상호작용 제한")]
    public InteractionManager interactionObj;

    bool haveInteractionObj;

    int TextIndex;
    Coroutine reactionCoroutine;
    DialogueBubble currentBubbleInstance;
    BoxCollider2D boxCol;

    private void Awake()
    {
        boxCol = GetComponent<BoxCollider2D>();
        boxCol.isTrigger = true;
    }

    private void Start()
    {
        if (reactionText == null || reactionText.Length == 0)
        {
            this.enabled = false;
            return;
        }

        haveInteractionObj = interactionObj != null;
    }

    public void OnAttack()
    {
        if (reactionCoroutine != null)
        {
            Destroy(currentBubbleInstance.gameObject);
            StopCoroutine(reactionCoroutine);
        }

        currentBubbleInstance = Instantiate(dialogueBubblePrefab, transform.position + (Vector3)bubbleOffset, Quaternion.identity);
        currentBubbleInstance.SetTailToLower(isTailLower);

        currentBubbleInstance.SetText(reactionText[TextIndex]);
        TextIndex = isTextRandom ? Random.Range(0, reactionText.Length) : TextIndex >= reactionText.Length - 1 ? 0 : TextIndex + 1;
        // 삼삼항항연연산산자자 !!!!!!!!!!!!!!

        reactionCoroutine = StartCoroutine(StartReaction());
        if(haveInteractionObj) interactionObj.canInteract(false);
    }

    IEnumerator StartReaction()
    {
        yield return new WaitForSeconds(reactionlength);
        Destroy(currentBubbleInstance.gameObject);
        if (haveInteractionObj) interactionObj.canInteract(true);
        reactionCoroutine = null;
    }


    public bool CanDestructible()
    {
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + (Vector3)bubbleOffset, 0.1f);
    }
}
