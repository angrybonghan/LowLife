using UnityEngine;

public class QuestDialogueTrigger : MonoBehaviour, I_DialogueCallback
{
    public string questID;

    public void OnDialogueEnd()
    {
        // 퀘스트 시작
        QuestManager.Instance.StartQuest(questID);

        // Dialogue 타입이면 여기서 수동으로 완료 처리
        QuestDataSO quest = QuestManager.Instance.GetQuestData(questID);
        if (quest != null && quest.questType == QuestType.Dialogue)
        {
            QuestManager.Instance.CompleteQuest(quest);
        }

        // UI 갱신
        FindObjectOfType<QuestUIController>()?.UpdateQuestText();
    }
}