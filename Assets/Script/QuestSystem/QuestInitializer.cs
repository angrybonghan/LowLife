using UnityEngine;

/// <summary>
/// 씬 시작 시 여러 퀘스트를 초기화하고 QuestManager에 등록.
/// Combat 퀘스트는 EnemyZone과 연결.
/// </summary>
public class QuestInitializer : MonoBehaviour
{
    [Header("퀘스트 데이터들")]
    public QuestDataSO[] questDatas;   // 여러 퀘스트 데이터 SO 배열

    [Header("Combat 퀘스트 Zone들")]
    public EnemyZone[] enemyZones;     // 여러 Combat 퀘스트 Zone 배열

    private void Start()
    {
        //모든 퀘스트 등록
        foreach (var questData in questDatas)
        {
            if (questData == null) continue;

            QuestManager.Instance.AddToActiveQuests(questData);
            Debug.Log($"[QuestInitializer] {questData.questName} 등록 완료 (타입: {questData.questType})");

            //Combat 퀘스트라면 EnemyZone 연결
            if (questData.questType == QuestType.CombatClear ||
                questData.questType == QuestType.CombatCount)
            {
                foreach (var zone in enemyZones)
                {
                    if (zone == null) continue;

                    // Zone에 연결할 퀘스트 ID가 같으면 매칭
                    if (zone.questData != null && zone.questData.questID == questData.questID)
                    {
                        zone.questData = questData;
                        Debug.Log($"[QuestInitializer] Combat 퀘스트 {questData.questName} → EnemyZone 연결 완료");
                    }
                }
            }
        }
    }
}