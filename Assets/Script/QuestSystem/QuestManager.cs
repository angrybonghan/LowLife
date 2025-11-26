using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트 상태 정의
/// </summary>
public enum QuestState { NotStarted, InProgress, Completed }

/// <summary>
/// 퀘스트 상태와 진행을 관리하는 매니저.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private void Awake() => Instance = this;

    public List<QuestDataSO> activeQuests = new List<QuestDataSO>();
    private Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();

    // 퀘스트 등록
    public void AddToActiveQuests(QuestDataSO quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            if (!questStates.ContainsKey(quest.questID))
            {
                questStates[quest.questID] = QuestState.NotStarted;
                Debug.Log($"[퀘스트 등록] {quest.questID} 초기 상태: NotStarted");
            }
        }
    }

    // 퀘스트 상태 조회
    public QuestState GetQuestState(string questID)
    {
        if (questStates.TryGetValue(questID, out var state))
            return state;

        Debug.LogWarning($"[퀘스트 상태 없음] ID: {questID}");
        return QuestState.NotStarted;
    }

    // 퀘스트 시작
    public void StartQuest(string questID)
    {
        QuestDataSO quest = GetQuestData(questID);
        if (quest == null) return;

        if (questStates[questID] == QuestState.NotStarted)
        {
            questStates[questID] = QuestState.InProgress;
            Debug.Log($"[퀘스트 시작] {quest.questID} - {quest.questName}");
            FindObjectOfType<QuestUIController>()?.UpdateQuestText();
        }
    }

    public void CompleteQuest(QuestDataSO quest)
    {
        if (questStates[quest.questID] != QuestState.InProgress) return;

        questStates[quest.questID] = QuestState.Completed;
        Debug.Log($"[퀘스트 완료] {quest.questID} - {quest.questName}");

        // 업적 처리
        if (!string.IsNullOrEmpty(quest.achievementID))
            AchievementManager.Instance.UnlockAchievement(quest.achievementID);

        if (quest.achievementPopup != null)
            Instantiate(quest.achievementPopup);

        // 선행 퀘스트 완료 시 후속 퀘스트 자동 시작
        foreach (var nextQuest in activeQuests)
        {
            if (nextQuest.prerequisiteQuestID == quest.questID)
            {
                Debug.Log($"[다음 퀘스트 자동 시작] {nextQuest.questID}");
                StartQuest(nextQuest.questID);

                // Collect 퀘스트라면 ItemDatabase 확인 후 즉시 완료 처리
                if (nextQuest.questType == QuestType.Collect)
                {
                    CollectQuest collectQuest = FindObjectOfType<CollectQuest>();
                    if (collectQuest != null)
                    {
                        collectQuest.CheckAndCompleteIfItemsReady(nextQuest.questID);
                    }
                }
            }
        }

        FindObjectOfType<QuestUIController>()?.UpdateQuestText();
    }


    // 퀘스트 데이터 조회
    public QuestDataSO GetQuestData(string questID)
    {
        return activeQuests.Find(q => q.questID == questID);
    }
}