using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트 상태 및 진행도 관리.
/// - 저장/불러오기 담당
/// - 퀘스트 시작/완료 시 상태 갱신 및 저장
/// </summary>
public enum QuestState { NotStarted, InProgress, Completed }

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("전체 퀘스트 데이터")]
    public QuestDataSO[] allQuests; // Inspector에서 등록

    public List<QuestDataSO> activeQuests = new List<QuestDataSO>();
    public Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();

    private Dictionary<string, int> questKillCounts = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 모든 퀘스트 등록 (한 번만)
            foreach (var quest in allQuests)
            {
                AddToActiveQuests(quest);
            }

            // 저장된 상태 불러오기
            SaveSystemJSON.DataLoadQuests(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        // 게임 종료 시 자동 저장
        SaveSystemJSON.DataSaveQuests(this);
    }

    public void AddToActiveQuests(QuestDataSO quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);

            // 이미 저장된 상태가 있으면 덮어쓰지 않음
            if (!questStates.ContainsKey(quest.questID))
                questStates[quest.questID] = QuestState.NotStarted;
            else
                Debug.Log($"[QuestManager] {quest.questName} 기존 상태 유지: {questStates[quest.questID]}");
        }
    }

    public QuestDataSO GetQuestData(string questID) =>
        activeQuests.Find(q => q.questID == questID);

    public QuestState GetQuestState(string questID) =>
        questStates.TryGetValue(questID, out var state) ? state : QuestState.NotStarted;

    public void StartQuest(string questID)
    {
        var quest = GetQuestData(questID);
        if (quest == null) return;

        if (questStates[questID] == QuestState.NotStarted)
        {
            questStates[questID] = QuestState.InProgress;
            Debug.Log($"[퀘스트 시작] {quest.questName}");
            SaveSystemJSON.DataSaveQuests(this);
            FindObjectOfType<UIManager>()?.UpdateQuestText();
        }
    }

    public void CompleteQuest(QuestDataSO quest)
    {
        UIManager.Instance?.ShowQuestCompleted(quest.questName);

        if (questStates[quest.questID] != QuestState.InProgress) return;

        questStates[quest.questID] = QuestState.Completed;
        Debug.Log($"[퀘스트 완료] {quest.questName}");
        SaveSystemJSON.DataSaveQuests(this);
        FindObjectOfType<UIManager>()?.UpdateQuestText();

        AchievementManager.Instance?.OnQuestCompleted(quest.questID);

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
        Debug.Log($"[로드 적용] {questID} → {state}");
        FindObjectOfType<UIManager>()?.UpdateQuestText();
    }

    // 진행도 관리
    public void UpdateKillCount(string questID, int killed) =>
        questKillCounts[questID] = killed;

    public int GetKillCount(string questID) =>
        questKillCounts.TryGetValue(questID, out int count) ? count : 0;
}