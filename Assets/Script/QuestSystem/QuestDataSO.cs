using UnityEngine;

// 퀘스트 정보를 담는 ScriptableObject
[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/New Quest")]
public class QuestDataSO : ScriptableObject
{
    [Header("퀘스트")]
    public string questID;                    // 퀘스트 고유 ID
    public string questName;                  // 퀘스트 이름
    public string description;                // 퀘스트 설명
    public bool completeOnDialogueEnd = true; // 대화만으로 완료되는 퀘스트 여부

    public string prerequisiteQuestID;        // 선행 퀘스트 ID (연계 퀘스트용)

    [Header("업적")]
    public string achievementID;              // 업적 ID
    public GameObject achievementPopup;       // 업적 달성 시 표시할 UI 오브젝트

    [Header("구출 퀘스트용")]
    public GameObject npcToRescue;            // 구출할 NPC 오브젝트

    public float detectUp = 5f;
    public float detectDown = 5f;
    public float detectLeft = 5f;
    public float detectRight = 5f;

    public LayerMask enemyLayer;
    public Transform questCenter;
}