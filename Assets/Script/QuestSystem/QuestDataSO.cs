using UnityEngine;

/// <summary>
/// 퀘스트 정보를 담는 ScriptableObject.
/// 각 퀘스트별로 필요한 데이터(타입, 목표 위치, 적 Layer 등)를 저장.
/// </summary>
[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/New Quest")]
public class QuestDataSO : ScriptableObject
{
    public string questID;              // 퀘스트 고유 ID
    public string questName;            // 퀘스트 이름
    public string description;          // 퀘스트 설명
    public string prerequisiteQuestID;  // 선행 퀘스트 ID (없으면 비워둠)

    public QuestType questType = QuestType.Dialogue; // 퀘스트 종류

    // Combat 퀘스트용 (범위 내 적 처치)
    public float detectUp = 5f;
    public float detectDown = 5f;
    public float detectLeft = 5f;
    public float detectRight = 5f;
    public LayerMask enemyLayer;
    public Vector3 questCenterPosition;

    // Collect 퀘스트용 (아이템 수집)
    public string requiredItemID;
    public int requiredItemCount = 1;

    // Delivery 퀘스트용 (아이템 전달)
    public string deliveryTargetNPC;

    // Explore 퀘스트용 (특정 위치 도달)
    public string targetSceneName;
    public Vector3 exploreTargetPosition;
    public float exploreRadius = 3f;

    // Escort 퀘스트용 (NPC 호위)
    public string escortTargetSceneName;
    public Vector3 escortTargetPosition;
    public float escortCompleteRadius = 2f;

    // 완료 연출
    public string achievementID;
    public GameObject achievementPopup;
}

/// <summary>
/// 퀘스트 타입 정의
/// </summary>
public enum QuestType
{
    Dialogue,   // NPC 대화
    Combat,     // 적 처치
    Delivery,   // 아이템 전달
    Collect,    // 아이템 수집
    Explore,    // 특정 위치 도달
    Escort      // NPC 호위
}