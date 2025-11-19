using UnityEngine;

// 퀘스트 정보를 담는 ScriptableObject
[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest/New Quest")]
public class QuestDataSO : ScriptableObject
{
    public string questID;
    public string questName;
    public string description;
    public string prerequisiteQuestID;

    [Header("퀘스트 타입")]
    public QuestType questType = QuestType.Dialogue;

    public float detectUp = 5f;
    public float detectDown = 5f;
    public float detectLeft = 5f;
    public float detectRight = 5f;
    public LayerMask enemyLayer;
    public Vector3 questCenterPosition;

    public string requiredItemID;

    public GameObject npcToRescue;
    public string achievementID;
    public GameObject achievementPopup;
}