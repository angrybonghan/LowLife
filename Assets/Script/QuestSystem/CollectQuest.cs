using UnityEngine;

public class CollectQuest : MonoBehaviour
{
    // 선행 퀘스트 완료 후 Collect 퀘스트 시작 시 아이템 확인
    public void CheckAndCompleteIfItemsReady(string questID)
    {
        QuestDataSO quest = QuestManager.Instance.GetQuestData(questID);
        if (quest == null || quest.questType != QuestType.Collect) return;

        // 현재 아이템 개수 확인
        int currentItemCount = ItemDatabase.Instance.GetItemCount(quest.requiredItemID);

        // 이미 목표 개수 이상이면 바로 완료 처리
        if (currentItemCount >= quest.requiredItemCount &&
            QuestManager.Instance.GetQuestState(questID) == QuestState.InProgress)
        {
            QuestManager.Instance.CompleteQuest(quest);
            Debug.Log($"[퀘스트 완료] {quest.questID} - {quest.questName} (선행 퀘스트 완료 후 아이템 보유)");
        }
    }

    // 아이템 획득 시 호출
    public void AddItem(string questID, string itemID)
    {
        QuestDataSO quest = QuestManager.Instance.GetQuestData(questID);
        if (quest == null || quest.questType != QuestType.Collect) return;
        if (QuestManager.Instance.GetQuestState(questID) != QuestState.InProgress) return;

        if (itemID == quest.requiredItemID)
        {
            int currentItemCount = ItemDatabase.Instance.GetItemCount(itemID);

            Debug.Log($"[퀘스트: {questID}] 아이템 {currentItemCount}/{quest.requiredItemCount}개 수집");

            if (currentItemCount >= quest.requiredItemCount)
            {
                QuestManager.Instance.CompleteQuest(quest);
                Debug.Log($"[퀘스트 완료] {quest.questID} - {quest.questName}");
            }

            FindObjectOfType<QuestUIController>()?.UpdateQuestText();
        }
    }
}