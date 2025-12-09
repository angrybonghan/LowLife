using System.Collections;
using UnityEngine;

public class StartDialogueWithInteraction : MonoBehaviour, I_Interactable, I_DialogueCallback
{
    [Header("NPC ID (업적용)")]
    public string npcID; // 특정 NPC ID (업적 체크용)

    [Header("퀘스트 연결")]
    public string questID; // 연결된 퀘스트 ID (없으면 일반 대화만 실행)

    [Header("대화")]
    public DialogueSO defaultDialogue; // 선행 퀘스트 미완료 시 보여줄 대화
    public DialogueSO questDialogue;   // 퀘스트 진행용 대화

    [Header("플레이어 이동/연출")]
    public Vector3 playerPos;
    public float duration = 0.2f;
    public bool facingRight = true;

    public void InInteraction()
    {
        PlayerController.instance.AllStop();
        PlayerHandler.instance.PlayerGoto(playerPos, duration, facingRight);

        QuestDataSO quest = null;
        if (!string.IsNullOrEmpty(questID))
        {
            quest = QuestManager.Instance.GetQuestData(questID);
        }

        // 선행 퀘스트 확인
        if (quest != null && !string.IsNullOrEmpty(quest.prerequisiteQuestID))
        {
            var preState = QuestManager.Instance.GetQuestState(quest.prerequisiteQuestID);
            if (preState != QuestState.Completed)
            {
                // 선행 퀘스트 미완료 → 기본 대화 출력
                if (defaultDialogue != null)
                    DialogManager.instance.StartDialogue(defaultDialogue, gameObject);

                PlayerController.canControl = false;
                return;
            }
        }

        // 퀘스트가 있으면 퀘스트 대화, 없으면 기본 대화 실행
        if (quest != null && questDialogue != null)
        {
            DialogManager.instance.StartDialogue(questDialogue, gameObject);
        }
        else if (defaultDialogue != null)
        {
            DialogManager.instance.StartDialogue(defaultDialogue, gameObject);
        }

        PlayerController.canControl = false;
    }

    public void OnDialogueEnd()
    {
        if (!string.IsNullOrEmpty(npcID))
        {
            AchievementManager.Instance?.OnTalkToNPC(npcID);
        }

        if (!string.IsNullOrEmpty(questID))
        {
            var quest = QuestManager.Instance.GetQuestData(questID);
            if (quest != null)
            {
                if (string.IsNullOrEmpty(quest.prerequisiteQuestID) ||
                    QuestManager.Instance.GetQuestState(quest.prerequisiteQuestID) == QuestState.Completed)
                {
                    QuestManager.Instance.StartQuest(questID);

                    if (quest.questType == QuestType.Dialogue)
                    {
                        QuestManager.Instance.CompleteQuest(quest);
                    }

                    FindObjectOfType<UIManager>()?.UpdateQuestText();
                }
            }
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