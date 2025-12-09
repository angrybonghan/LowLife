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
        // 좌/우/상/하 각각 거리 반영
        Vector3 boxSize = new Vector3(
            questData.detectLeft + questData.detectRight,
            questData.detectUp + questData.detectDown,
            1f);

        Vector3 center = questData.questCenterPosition + new Vector3(
            (questData.detectRight - questData.detectLeft) * 0.5f,
            (questData.detectUp - questData.detectDown) * 0.5f,
            0f);

        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            center,
            boxSize,
            0f,
            questData.enemyLayer);

        return enemies.Length;
    }

    // Scene 뷰에서 범위 확인용 Gizmos
    private void OnDrawGizmos()
    {
        if (questData == null) return;

        Vector3 boxSize = new Vector3(
            questData.detectLeft + questData.detectRight,
            questData.detectUp + questData.detectDown,
            1f);

        Vector3 center = questData.questCenterPosition + new Vector3(
            (questData.detectRight - questData.detectLeft) * 0.5f,
            (questData.detectUp - questData.detectDown) * 0.5f,
            0f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, boxSize);

        // 각 방향 Ray 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(questData.questCenterPosition, Vector3.left * questData.detectLeft);
        Gizmos.DrawRay(questData.questCenterPosition, Vector3.right * questData.detectRight);
        Gizmos.DrawRay(questData.questCenterPosition, Vector3.up * questData.detectUp);
        Gizmos.DrawRay(questData.questCenterPosition, Vector3.down * questData.detectDown);
    }
}