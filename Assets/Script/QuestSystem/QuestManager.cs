using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트 상태 및 진행도 관리.
/// </summary>
public enum QuestState { NotStarted, InProgress, Completed }

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<QuestDataSO> activeQuests = new List<QuestDataSO>();
    public Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();

    //진행도 저장 (예: 처치 수)
    private Dictionary<string, int> questKillCounts = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        QuestSaveSystemJSON.LoadQuests(this);
    }

    public void AddToActiveQuests(QuestDataSO quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            if (!questStates.ContainsKey(quest.questID))
                questStates[quest.questID] = QuestState.NotStarted;
        }
    }

    public QuestDataSO GetQuestData(string questID) => activeQuests.Find(q => q.questID == questID);

    public QuestState GetQuestState(string questID) => questStates.TryGetValue(questID, out var state) ? state : QuestState.NotStarted;

    public void StartQuest(string questID)
    {
        var quest = GetQuestData(questID);
        if (quest == null) return;

        if (questStates[questID] == QuestState.NotStarted)
        {
            questStates[questID] = QuestState.InProgress;
            Debug.Log($"[퀘스트 시작] {quest.questName}");
            QuestSaveSystemJSON.SaveQuests(this);
            FindObjectOfType<UIManager>()?.UpdateQuestText();
        }
    }

    public void CompleteQuest(QuestDataSO quest)
    {
        if (questStates[quest.questID] != QuestState.InProgress) return;

        questStates[quest.questID] = QuestState.Completed;
        Debug.Log($"[퀘스트 완료] {quest.questName}");
        QuestSaveSystemJSON.SaveQuests(this);
        FindObjectOfType<UIManager>()?.UpdateQuestText();

        // 선행 퀘스트 완료 시 자동 시작
        foreach (var nextQuest in activeQuests)
        {
            if (!string.IsNullOrEmpty(nextQuest.prerequisiteQuestID) &&
                nextQuest.prerequisiteQuestID == quest.questID &&
                questStates[nextQuest.questID] == QuestState.NotStarted)
            {
                Debug.Log($"[자동 시작] {nextQuest.questName} 시작");
                StartQuest(nextQuest.questID);
            }
        }
    }

    public void ForceSetQuestState(string questID, QuestState state)
    {
        questStates[questID] = state;
        FindObjectOfType<UIManager>()?.UpdateQuestText();
    }

    //진행도 관리
    public void UpdateKillCount(string questID, int killed) =>
        questKillCounts[questID] = killed;

    public int GetKillCount(string questID) =>
        questKillCounts.TryGetValue(questID, out int count) ? count : 0;
}