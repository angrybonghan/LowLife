using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/New Quest")]
public class QuestDataSO : ScriptableObject
{
    public string questID;              // 퀘스트 고유 ID
    public string questName;            // 퀘스트 이름
    public string description;          // 퀘스트 설명
    public string prerequisiteQuestID;  // 선행 퀘스트 ID

    public QuestType questType = QuestType.Dialogue; // 퀘스트 종류

    // CombatClear / CombatCount
    public float detectUp = 5f, detectDown = 5f, detectLeft = 5f, detectRight = 5f;
    public LayerMask enemyLayer;
    public Vector3 questCenterPosition;
    public int targetKillCount; // CombatCount 목표 처치 수

    // Collect
    public string requiredItemID;
    public int requiredItemCount = 1;

    // Delivery
    public string deliveryTargetNPC;

    // Explore
    public string targetSceneName;
    public Vector3 exploreTargetPosition;
    public float exploreRadius = 3f;

    // Escort
    public string escortTargetSceneName;
    public Vector3 escortTargetPosition;
    public float escortCompleteRadius = 2f;

    // 완료 연출
    public string achievementID;
    public GameObject achievementPopup;
}

public enum QuestType
{
    Dialogue,       // NPC 대화
    CombatClear,    // 범위 내 모든 적 처치
    CombatCount,    // 범위 내 적 목표 처치 수
    Delivery,       // 아이템 전달
    Collect,        // 아이템 수집
    Explore,        // 특정 위치 도달
    Escort          // NPC 호위
}