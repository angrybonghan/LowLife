using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // 현재 등록된 퀘스트 목록
    public List<QuestDataSO> activeQuests = new List<QuestDataSO>();

    // 퀘스트 상태 저장 (ID → 상태)
    private Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();

    // 퀘스트 등록
    public void AddToActiveQuests(QuestDataSO quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            questStates[quest.questID] = QuestState.NotStarted;
        }
    }

    // 퀘스트 시작
    public void StartQuest(string questID)
    {
        QuestDataSO quest = GetQuestData(questID);
        if (quest == null) return;

        if (questStates[questID] == QuestState.NotStarted)
        {
            questStates[questID] = QuestState.InProgress;


            // UI 갱신
            FindObjectOfType<QuestUIController>()?.UpdateQuestText();
        }
    }

    // 퀘스트 완료
    public void CompleteQuest(QuestDataSO quest)
    {
        if (questStates[quest.questID] != QuestState.InProgress)
        {
            Debug.Log($"[퀘스트 완료 실패] {quest.questID}는 진행 중이 아닙니다.");
            return;
        }

        questStates[quest.questID] = QuestState.Completed;

        Debug.Log($"[퀘스트 완료] {quest.questID} - {quest.questName}");

        // 업적 처리
        if (!string.IsNullOrEmpty(quest.achievementID))
            AchievementManager.Instance.UnlockAchievement(quest.achievementID);

        if (quest.achievementPopup != null)
            Instantiate(quest.achievementPopup);

        // 다음 퀘스트 자동 시작
        foreach (var nextQuest in activeQuests)
        {
            if (nextQuest.prerequisiteQuestID == quest.questID)
            {
                Debug.Log($"[다음 퀘스트 자동 시작] {nextQuest.questID}");
                StartQuest(nextQuest.questID);
            }
        }

        FindObjectOfType<QuestUIController>()?.UpdateQuestText();
    }

    // 퀘스트 상태 확인
    public QuestState GetQuestState(string questID)
    {
        if (questStates.ContainsKey(questID))
            return questStates[questID];
        return QuestState.NotStarted;
    }

    // 퀘스트 데이터 가져오기
    public QuestDataSO GetQuestData(string questID)
    {
        return activeQuests.Find(q => q.questID == questID);
    }

    // 현재 진행 중인 퀘스트 목록 반환
    public List<QuestDataSO> GetActiveQuests()
    {
        return activeQuests;
    }
}
