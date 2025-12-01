using UnityEngine;

/// <summary>
/// Collect 타입 퀘스트 진행 확인
/// - 아이템 획득 시 호출
/// - 퀘스트 시작 시에도 인벤토리 확인
/// </summary>
public class CollectQuest : MonoBehaviour
{
    /// <summary>
    /// 아이템 획득 시 모든 Collect 퀘스트 확인
    /// </summary>
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

                    // UI 반영
                    UIManager.Instance?.ShowQuestCompleted(quest.questName);
                }
            }
        }
        UIManager.Instance?.UpdateQuestText();
    }

    public void CheckQuestOnStart()
    {
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            if (quest.questType == QuestType.Collect)
            {
                int currentItemCount = ItemDatabase.Instance.GetItemCount(quest.requiredItemID);

                if (currentItemCount >= quest.requiredItemCount &&
                    QuestManager.Instance.GetQuestState(quest.questID) == QuestState.InProgress)
                {
                    QuestManager.Instance.CompleteQuest(quest);
                    Debug.Log($"[퀘스트 즉시 완료] {quest.questID} - {quest.questName}");

                    // UI 반영
                    UIManager.Instance?.ShowQuestCompleted(quest.questName);
                }
            }
        }
        UIManager.Instance?.UpdateQuestText();
    }
}