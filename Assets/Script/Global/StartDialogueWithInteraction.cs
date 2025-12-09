using System.Collections;
using UnityEngine;

public class StartDialogueWithInteraction : MonoBehaviour, I_Interactable, I_DialogueCallback
{
    [Header("NPC ID (업적용)")]
    public string npcID;

    [Header("퀘스트 연결")]
    public string questID;

    [Header("대화")]
    public DialogueSO defaultDialogue; // 선행 퀘스트 미완료 시 보여줄 대화
    public DialogueSO questDialogue;   // 퀘스트 진행용 대화

    [Header("플레이어 이동/연출")]
    public Vector3 playerPos;
    public float duration = 0.2f;
    public bool facingRight = true;

    [Header("아이템 보상")]
    public string rewardItemID;     // 대화 종료 후 지급할 아이템 ID
    public int rewardItemCount = 1; // 지급할 개수

    [Header("NPC 이미지 변경 (SpriteRenderer)")]
    public SpriteRenderer npcRenderer;   // 월드 오브젝트 SpriteRenderer 연결
    public Sprite startDialogueSprite;   // 대화 시작 시 스프라이트
    public Sprite endDialogueSprite;     // 대화 종료 시 스프라이트

    public void InInteraction()
    {
        PlayerController.instance.AllStop();
        PlayerHandler.instance.PlayerGoto(playerPos, duration, facingRight);

        // 대화 시작 시 이미지 변경
        if (npcRenderer != null && startDialogueSprite != null)
        {
            npcRenderer.sprite = startDialogueSprite;
        }

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
                if (defaultDialogue != null)
                    DialogManager.instance.StartDialogue(defaultDialogue, gameObject);

                PlayerController.canControl = false;
                return;
            }
        }

        // 퀘스트 대화 또는 기본 대화 실행
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

        // 퀘스트 처리
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

        // 아이템 보상 처리
        if (!string.IsNullOrEmpty(rewardItemID) && rewardItemCount > 0)
        {
            if (ItemDatabase.Instance != null)
            {
                ItemDatabase.Instance.AddItem(rewardItemID, rewardItemCount);
                Debug.Log($"[아이템 획득] {rewardItemID} x{rewardItemCount}");
                UIManager.Instance?.UpdateQuestText();
            }
            else
            {
                Debug.LogWarning("[아이템 획득 실패] ItemDatabase가 존재하지 않습니다.");
            }
        }

        // 대화 종료 시 이미지 변경
        if (npcRenderer != null && endDialogueSprite != null)
        {
            npcRenderer.sprite = endDialogueSprite;
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