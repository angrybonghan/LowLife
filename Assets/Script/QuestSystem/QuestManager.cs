using UnityEngine;
using System.Collections.Generic;

// 퀘스트 상태를 관리하고 퀘스트 진행을 처리하는 매니저
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    private Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();
    private List<QuestDataSO> activeQuests = new List<QuestDataSO>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        foreach (var quest in activeQuests)
        {
            if (questStates[quest.questID] != QuestState.InProgress) continue;

            switch (quest.questType)
            {
                case QuestType.Combat:
                    Vector3 origin = quest.questCenterPosition;
                    bool enemyDetected = false;
                    enemyDetected |= Physics.Raycast(origin, Vector3.up, quest.detectUp, quest.enemyLayer);
                    enemyDetected |= Physics.Raycast(origin, Vector3.down, quest.detectDown, quest.enemyLayer);
                    enemyDetected |= Physics.Raycast(origin, Vector3.left, quest.detectLeft, quest.enemyLayer);
                    enemyDetected |= Physics.Raycast(origin, Vector3.right, quest.detectRight, quest.enemyLayer);

                    if (!enemyDetected)
                        CompleteQuest(quest);
                    break;
            }
        }
    }

    public void RegisterQuest(QuestDataSO quest)
    {
        if (!questStates.ContainsKey(quest.questID))
            questStates.Add(quest.questID, QuestState.NotStarted);
    }

    public void AddToActiveQuests(QuestDataSO quest)
    {
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

        if (!string.IsNullOrEmpty(quest.prerequisiteQuestID) &&
            GetQuestState(quest.prerequisiteQuestID) != QuestState.Completed)
            return;

        if (questStates[questID] == QuestState.NotStarted)
        {
            questStates[questID] = QuestState.InProgress;

            if (quest.npcToRescue != null)
                quest.npcToRescue.SetActive(false);

            if (quest.questType == QuestType.Dialogue)
                CompleteQuest(quest);
        }
    }

    public void DeliverItem(string itemID)
    {
        foreach (var quest in activeQuests)
        {
            if (quest.questType == QuestType.Delivery &&
                quest.requiredItemID == itemID &&
                questStates[quest.questID] == QuestState.InProgress)
            {
                CompleteQuest(quest);
            }
        }
    }

    private void CompleteQuest(QuestDataSO quest)
    {
        questStates[quest.questID] = QuestState.Completed;

        if (quest.npcToRescue != null)
            quest.npcToRescue.SetActive(true);

        AchievementManager.Instance.UnlockAchievement(quest.achievementID);

        if (quest.achievementPopup != null)
            quest.achievementPopup.SetActive(true);

        foreach (var nextQuest in activeQuests)
        {
            if (nextQuest.prerequisiteQuestID == quest.questID &&
                questStates[nextQuest.questID] == QuestState.NotStarted)
            {
                StartQuest(nextQuest.questID);
            }
        }
    }

    public QuestState GetQuestState(string questID)
    {
        return questStates.ContainsKey(questID) ? questStates[questID] : QuestState.NotStarted;
    }

    public List<QuestDataSO> GetActiveQuests() => activeQuests;
}