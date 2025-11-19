using UnityEngine;
using System.Collections.Generic;

// 퀘스트 상태를 관리하고 적 처치 여부를 감지하는 매니저
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>(); // 퀘스트 상태 저장
    private List<QuestDataSO> activeQuests = new List<QuestDataSO>(); // 등록된 퀘스트 목록

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // 진행 중인 퀘스트의 적 처치 여부 확인
        foreach (var quest in activeQuests)
        {
            if (questStates[quest.questID] == QuestState.InProgress)
            {
                bool enemyDetected = false;

                // 상
                enemyDetected |= Physics.Raycast(quest.questCenter.position, Vector3.up, quest.detectUp, quest.enemyLayer);
                // 하
                enemyDetected |= Physics.Raycast(quest.questCenter.position, Vector3.down, quest.detectDown, quest.enemyLayer);
                // 좌
                enemyDetected |= Physics.Raycast(quest.questCenter.position, Vector3.left, quest.detectLeft, quest.enemyLayer);
                // 우
                enemyDetected |= Physics.Raycast(quest.questCenter.position, Vector3.right, quest.detectRight, quest.enemyLayer);

                if (!enemyDetected)
                {
                    CompleteQuest(quest);
                }
            }
    }
}

    public void RegisterQuest(QuestDataSO quest)
    {
        // 퀘스트 상태 초기화
        if (!questStates.ContainsKey(quest.questID))
        {
            questStates.Add(quest.questID, QuestState.NotStarted);
        }
    }

    public void AddToActiveQuests(QuestDataSO quest)
    {
        // 퀘스트 목록에 추가 및 상태 등록
        if (!activeQuests.Contains(quest))
        {
            activeQuests.Add(quest);
            RegisterQuest(quest);
        }
    }

    public void StartQuest(string questID)
    {
        QuestDataSO quest = activeQuests.Find(q => q.questID == questID);
        if (quest == null) return;

        // 선행 퀘스트가 있다면 완료 여부 확인
        if (!string.IsNullOrEmpty(quest.prerequisiteQuestID))
        {
            if (GetQuestState(quest.prerequisiteQuestID) != QuestState.Completed)
            {
                Debug.Log($"[퀘스트 시작 실패] 선행 퀘스트 '{quest.prerequisiteQuestID}'가 완료되지 않았습니다.");
                return;
            }
        }

        // 퀘스트 시작 처리
        if (questStates[questID] == QuestState.NotStarted)
        {
            questStates[questID] = QuestState.InProgress;
            if (quest.npcToRescue != null)
                quest.npcToRescue.SetActive(false); // NPC 숨김

            //대화만으로 완료되는 퀘스트는 즉시 완료 처리
            if (quest.completeOnDialogueEnd)
            {
                CompleteQuest(quest);
            }

        }
    }

    private void CompleteQuest(QuestDataSO quest)
    {
        // 퀘스트 완료 처리
        questStates[quest.questID] = QuestState.Completed;

        if (quest.npcToRescue != null)
            quest.npcToRescue.SetActive(true); // NPC 구출

        AchievementManager.Instance.UnlockAchievement(quest.achievementID); // 업적 해제

        if (quest.achievementPopup != null)
            quest.achievementPopup.SetActive(true); // 업적 UI 표시

        // 선행 퀘스트 완료 시 다음 퀘스트 자동 시작
        foreach (var nextQuest in activeQuests)
        {
            if (nextQuest.prerequisiteQuestID == quest.questID &&
                questStates[nextQuest.questID] == QuestState.NotStarted)
            {
                StartQuest(nextQuest.questID);
                Debug.Log($"[자동 시작] 퀘스트 '{nextQuest.questID}'가 자동 시작되었습니다.");
            }
        }
    }

    public QuestState GetQuestState(string questID)
    {
        return questStates.ContainsKey(questID) ? questStates[questID] : QuestState.NotStarted;
    }
}