using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class InteractionManager : MonoBehaviour
{
    [Header("키코드")]
    public KeyCode interactionKey = KeyCode.F;
    public bool canHoldInput = true;

    [Header("사용 설정")]
    public bool isReusable = false;

    [Header("UI")]
    public GameObject triggerUI; // 상호작용 UI

    bool canInteraction = true;
    bool isPlayerInRange = false;
    bool hasUI = false;

    BoxCollider2D boxCol;

    void Awake()
    {
        boxCol = GetComponent<BoxCollider2D>();
        boxCol.isTrigger = true;
    }

    void Start()
    {
        hasUI = triggerUI != null;
        InteractionOff();
    }

    void Update()
    {
        if (!canInteraction || !isPlayerInRange) return;
        if (canHoldInput ? Input.GetKey(interactionKey) : Input.GetKeyDown(interactionKey))
        {
            if (!isReusable)
            {
                InteractionOff();
                canInteraction = false;
            }
            CallInteractionEfficiently();
        }

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) InteractionOn();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) InteractionOff();
    }

    void InteractionOn()
    {
        isPlayerInRange = true;
        
        if (canInteraction && hasUI)
        {
            triggerUI.SetActive(true);
        }
    }

    void InteractionOff()
    {
        isPlayerInRange = false;
        if (hasUI) triggerUI.SetActive(false);
    }

    public void ResetInteraction()
    {
        canInteraction = true;
        if (IsPlayerInArea())
        {
            InteractionOn();
        }
    }

    void CallInteractionEfficiently()
    {
        I_Interactable[] interactableComponents = GetComponents<I_Interactable>();

        foreach (I_Interactable interactable in interactableComponents)
        {
            interactable.InInteraction();
        }
    }

    bool IsPlayerInArea()
    {
        Vector2 position = boxCol.bounds.center;
        Vector2 size = boxCol.bounds.size;
        float angle = transform.eulerAngles.z;

        Collider2D hit = Physics2D.OverlapBox(
            position,
            size,
            angle
            // LayerMask
        );

        if (hit != null)
        {
            if (hit.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    public void canInteract(bool isTrue)
    {
        canInteraction = isTrue;
        if (isTrue && IsPlayerInArea())
        {
            InteractionOn();
        }
        else
        {
            InteractionOff();
        }
    }
}
