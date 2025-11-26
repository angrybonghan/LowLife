using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 퀘스트 상태 정의
/// </summary>
public enum QuestState { NotStarted, InProgress, Completed }

/// <summary>
/// 퀘스트 상태와 진행을 관리하는 매니저.
/// 씬이 바뀌어도 유지되며, QuestSaveSystemJSON과 연동해 저장/불러오기 처리.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    // 현재 진행 중인 퀘스트 목록
    public List<QuestDataSO> activeQuests = new List<QuestDataSO>();

    // 퀘스트 상태 관리 (questID → QuestState)
    public Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 QuestManager 유지
    }

    private void Start()
    {
        // 씬 시작 시 저장된 퀘스트 불러오기
        QuestSaveSystemJSON.LoadQuests(this);
    }

    /// <summary>
    /// 퀘스트 등록 (activeQuests에 추가)
    /// </summary>
    public void AddToActiveQuests(QuestDataSO quest)
    {
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            if (!questStates.ContainsKey(quest.questID))
                questStates[quest.questID] = QuestState.NotStarted;
        }
    }

    /// <summary>
    /// 퀘스트 데이터 조회
    /// </summary>
    public QuestDataSO GetQuestData(string questID)
    {
        return activeQuests.Find(q => q.questID == questID);
    }

    /// <summary>
    /// 퀘스트 상태 조회
    /// </summary>
    public QuestState GetQuestState(string questID)
    {
        if (questStates.TryGetValue(questID, out var state))
            return state;
        return QuestState.NotStarted;
    }

    /// <summary>
    /// 퀘스트 시작
    /// </summary>
    public void StartQuest(string questID)
    {
        QuestDataSO quest = GetQuestData(questID);
        if (quest == null) return;

        if (questStates[questID] == QuestState.NotStarted)
        {
            questStates[questID] = QuestState.InProgress;
            Debug.Log($"[퀘스트 시작] {quest.questName}");

            // 저장
            QuestSaveSystemJSON.SaveQuests(this);
        }
    }

    /// <summary>
    /// 퀘스트 완료 처리
    /// </summary>
    public void CompleteQuest(QuestDataSO quest)
    {
        if (questStates[quest.questID] != QuestState.InProgress) return;

        questStates[quest.questID] = QuestState.Completed;
        Debug.Log($"[퀘스트 완료] {quest.questName}");

        // 저장
        QuestSaveSystemJSON.SaveQuests(this);
    }

    /// <summary>
    /// 불러오기 시 강제로 상태 세팅
    /// </summary>
    public void ForceSetQuestState(string questID, QuestState state)
    {
        questStates[questID] = state;
    }
}