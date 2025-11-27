using UnityEngine;

/// <summary>
/// Combat 퀘스트 전용 Zone.
/// 특정 범위 내 적을 감지하고, 모두 처치되면 퀘스트 완료 처리.
/// </summary>
public class EnemyZone : MonoBehaviour
{
    [Header("퀘스트 연결")]
    public string questID;        // 연결할 퀘스트 ID
    public QuestDataSO questData; // QuestInitializer에서 연결됨

    private int lastRemainingEnemies = -1; // 이전 프레임의 적 수 저장

    private void Update()
    {
        if (questData == null) return;

        int remainingEnemies = GetRemainingEnemies();

        //남은 적 수가 변했을 때만 로그 출력
        if (remainingEnemies != lastRemainingEnemies)
        {
            Debug.Log($"[Combat 퀘스트] {questData.questID} 범위 내 남은 적: {remainingEnemies}");
            lastRemainingEnemies = remainingEnemies;
        }

        //모든 적 처치 시 퀘스트 완료 처리
        if (remainingEnemies == 0 && QuestManager.Instance.GetQuestState(questData.questID) == QuestState.InProgress)
        {
            QuestManager.Instance.CompleteQuest(questData);
        }
    }


    // 범위 내 적 감지
    public int GetRemainingEnemies()
    {
        Vector3 boxSize = new Vector3(questData.detectLeft + questData.detectRight, questData.detectUp + questData.detectDown, 1f);

        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            questData.questCenterPosition, boxSize, 0f, questData.enemyLayer);

        return enemies.Length;
    }

    // Scene 뷰에서 감지 범위를 시각화
    private void OnDrawGizmosSelected()
    {
        if (questData == null) return;

        Gizmos.color = Color.red;
        Vector3 boxSize = new Vector3(questData.detectLeft + questData.detectRight, questData.detectUp + questData.detectDown, 1f);
        Gizmos.DrawWireCube(questData.questCenterPosition, boxSize);
    }
}