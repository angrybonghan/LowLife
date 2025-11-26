using System.Collections.Generic;
using UnityEngine;

public enum QuestState { NotStarted, InProgress, Completed }

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public List<QuestDataSO> activeQuests = new List<QuestDataSO>();
    public Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();

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

    public QuestDataSO GetQuestData(string questID)
    {
        return activeQuests.Find(q => q.questID == questID);
    }

    public QuestState GetQuestState(string questID)
    {
        if (questStates.TryGetValue(questID, out var state))
            return state;
        return QuestState.NotStarted;
    }

    public void StartQuest(string questID)
    {
        QuestDataSO quest = GetQuestData(questID);
        if (quest == null) return;

        if (questStates[questID] == QuestState.NotStarted)
        {
            questStates[questID] = QuestState.InProgress;
            Debug.Log($"[퀘스트 시작] {quest.questName}");

            QuestSaveSystemJSON.SaveQuests(this);
            FindObjectOfType<QuestUIController>()?.UpdateQuestText();
        }
    }

    public void CompleteQuest(QuestDataSO quest)
    {
        if (questStates[quest.questID] != QuestState.InProgress) return;

        questStates[quest.questID] = QuestState.Completed;
        Debug.Log($"[퀘스트 완료] {quest.questName}");

        QuestSaveSystemJSON.SaveQuests(this);
        FindObjectOfType<QuestUIController>()?.UpdateQuestText();

        foreach (var nextQuest in activeQuests)
        {
            if (!string.IsNullOrEmpty(nextQuest.prerequisiteQuestID) &&
                nextQuest.prerequisiteQuestID == quest.questID &&
                questStates[nextQuest.questID] == QuestState.NotStarted)
            {
                Debug.Log($"[자동 시작] 선행 퀘스트 {quest.questName} 완료 → {nextQuest.questName} 시작");
                StartQuest(nextQuest.questID);
            }
        }
    }

    public void ForceSetQuestState(string questID, QuestState state)
    {
        questStates[questID] = state;

        if (state == QuestState.InProgress)
        {
            Debug.Log($"[퀘스트 복원: 진행 중] {questID}");
            FindObjectOfType<QuestUIController>()?.UpdateQuestText();
        }
        else if (state == QuestState.Completed)
        {
            Debug.Log($"[퀘스트 복원: 완료] {questID}");
            FindObjectOfType<QuestUIController>()?.UpdateQuestText();
        }
    }
}