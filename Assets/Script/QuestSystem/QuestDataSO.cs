using UnityEngine;

// 퀘스트 정보를 담는 ScriptableObject
[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/New Quest")]
public class QuestDataSO : ScriptableObject
{
    public string questID;                  // 퀘스트 고유 ID
    public string questName;               // 퀘스트 이름
    public string description;             // 퀘스트 설명
    public string prerequisiteQuestID;     // 선행 퀘스트 ID (없으면 비워둠)

    [Header("퀘스트 타입")]
    public QuestType questType = QuestType.Dialogue; // 퀘스트 종류 (Dialogue, Combat, Delivery)

    public float detectUp = 5f;            // 위 방향 감지 거리
    public float detectDown = 5f;          // 아래 방향 감지 거리
    public float detectLeft = 5f;          // 왼쪽 방향 감지 거리
    public float detectRight = 5f;         // 오른쪽 방향 감지 거리
    public LayerMask enemyLayer;           // 감지할 적의 레이어
    public Vector3 questCenterPosition;    // 감지 기준 위치 (월드 좌표)

    public string requiredItemID;          // 필요한 아이템 ID (예: "herb_bundle")

    public GameObject npcToRescue;         // 퀘스트 완료 시 등장할 NPC (초기 비활성화)
    public string achievementID;           // 업적 ID (예: "ACH_HERB_DELIVERY")
    public GameObject achievementPopup;    // 업적 팝업 UI 오브젝트
}