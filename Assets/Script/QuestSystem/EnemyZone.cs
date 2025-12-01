using UnityEngine;

public class EnemyZone : MonoBehaviour
{
    public QuestDataSO questData;
    private int lastRemainingEnemies = -1;

    private void Update()
    {
        if (questData == null) return;

        int remainingEnemies = GetRemainingEnemies();

        if (remainingEnemies != lastRemainingEnemies)
        {
            Debug.Log($"[Combat 퀘스트] {questData.questID} 남은 적: {remainingEnemies}");
            lastRemainingEnemies = remainingEnemies;
        }

        switch (questData.questType)
        {
            case QuestType.CombatClear:
                if (remainingEnemies == 0 && QuestManager.Instance.GetQuestState(questData.questID) == QuestState.InProgress)
                {
                    QuestManager.Instance.CompleteQuest(questData);
                }
                break;

            case QuestType.CombatCount:
                int killed = questData.targetKillCount - remainingEnemies;
                QuestManager.Instance.UpdateKillCount(questData.questID, killed);

                if (killed >= questData.targetKillCount && QuestManager.Instance.GetQuestState(questData.questID) == QuestState.InProgress)
                {
                    QuestManager.Instance.CompleteQuest(questData);
                }
                break;
        }
    }

    public int GetRemainingEnemies()
    {
        Vector3 boxSize = new Vector3(questData.detectLeft + questData.detectRight, questData.detectUp + questData.detectDown, 1f);

        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            questData.questCenterPosition, boxSize, 0f, questData.enemyLayer);

        return enemies.Length;
    }
}