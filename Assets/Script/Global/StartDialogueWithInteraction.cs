using System.Collections;
using UnityEngine;

public class StartDialogueWithInteraction : MonoBehaviour, I_Interactable, I_DialogueCallback
{
    [Header("이름(특수 NPC(업적)일 경우)")]
    public string npcID; // Inspector에서 특정 NPC ID 지정

    [Header("대화")]
    public DialogueSO dialogue;

    [Header("플레이어")]
    public Vector3 playerPos;
    public float duration = 0.2f;
    public bool facingRight = true;


    public void InInteraction()
    {
        if (dialogue == null) return;

        PlayerController.instance.AllStop();
        PlayerHandler.instance.PlayerGoto(playerPos, duration, facingRight);

        DialogManager.instance.StartDialogue(dialogue, gameObject);

        PlayerController.canControl = false;
    }

    public void OnDialogueEnd()
    {
        if (!string.IsNullOrEmpty(npcID))
        {
            AchievementManager.Instance?.OnTalkToNPC(npcID);
        }

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
