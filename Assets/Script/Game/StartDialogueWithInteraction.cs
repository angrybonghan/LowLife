using System.Collections;
using UnityEngine;

public class StartDialogueWithInteraction : MonoBehaviour, I_Interactable, I_DialogueCallback
{
    [Header("대화")]
    public DialogueSO dialogue;

    [Header("플레이어")]
    public bool movePlayer;
    public Vector3 playerPos;
    public float duration = 0.2f;
    public bool facingRight = true;


    public void InInteraction()
    {
        if (dialogue == null) return;

        if (movePlayer)
        {
            PlayerHandler.instance.PlayerGoto(playerPos, duration, facingRight);
            PlayerController.instance.AllStop();
        }

        DialogManager.instance.StartDialogue(dialogue, gameObject);

        PlayerController.canControl = false;
    }

    public void OnDialogueEnd()
    {
        StartCoroutine(DialogueEndFunc());
    }

    IEnumerator DialogueEndFunc()
    {
        yield return new WaitForSeconds(0.2f);

        PlayerController.canControl = true;

        if (TryGetComponent<InteractionManager>(out InteractionManager interactionManager))
        {
            interactionManager.ResetInteraction();
        }
    }

}
