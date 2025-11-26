using UnityEngine;

public class CollectQuest : MonoBehaviour
{
    // 아이템 획득 시 모든 Collect 퀘스트 확인
    public void CheckAllCollectQuests(string itemID)
    {
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.questType == QuestType.Collect && quest.requiredItemID == itemID)
            {
                int currentItemCount = ItemDatabase.Instance.GetItemCount(itemID);

                Debug.Log($"[퀘스트: {quest.questID}] 아이템 {currentItemCount}/{quest.requiredItemCount}개 수집");

                if (currentItemCount >= quest.requiredItemCount &&
                    QuestManager.Instance.GetQuestState(quest.questID) == QuestState.InProgress)
                {
                    QuestManager.Instance.CompleteQuest(quest);
                    Debug.Log($"[퀘스트 완료] {quest.questID} - {quest.questName}");
                }
            }
        }

        FindObjectOfType<QuestUIController>()?.UpdateQuestText();
    }
}