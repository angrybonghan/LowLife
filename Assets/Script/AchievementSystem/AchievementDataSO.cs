using UnityEngine;

/// <summary>
/// 업적 조건 타입 정의
/// - 반복(횟수 기반)과 단발(이벤트 기반) 타입을 모두 포함
/// </summary>
public enum AchievementType
{
    // 반복(횟수 기반)
    KillEnemy,         // 특정 적 처치 (enemyType + targetCount)
    CollectItem,       // 특정 아이템 수집 (itemID + targetCount)
    ParrySuccess,      // 패링 성공 (targetCount)
    BlockSuccess,      // 방어 성공 (targetCount)
    TeleportUse,       // 순간이동 사용 (targetCount)
    AttackSuccess,     // 공격 성공 (targetCount)
    DiscoverSecretRoom,// 비밀의 방 발견 (targetCount = 전체 방 수)
    PlayerDeathCount,  //플레이어 죽음 횟수 업적 추가

    // 단발(이벤트 기반)
    CompleteQuest,     // 특정 퀘스트 완료 (questID)
    TalkToNPC,       // 숨겨진 NPC 발견 (이벤트 발생 시 즉시 달성)
    ViewEnding,        // 엔딩 보기 (이벤트 발생 시 즉시 달성)
    EnterScene         // 특정 씬 진입 (sceneName)
}

/// <summary>
/// 업적 데이터 ScriptableObject
/// - 업적 기본 정보, 조건, 진행 상태 저장
/// - 조건 타입에 따라 사용하는 필드가 달라짐
/// </summary>
[CreateAssetMenu(fileName = "NewAchievement", menuName = "Game/Achievement")]
public class AchievementDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string achievementID;   // 업적 고유 ID
    public string title;           // 업적 이름
    [TextArea] public string description; // 업적 설명
    [TextArea] public string hint;        // 힌트

    [Header("조건 타입")]
    public AchievementType type;

    [Header("조건 파라미터")]
    public string targetEnemyType;
    public string targetItemID;
    public string targetQuestID;
    public string targetSceneName;
    public string targetNPCID;
    public int targetCount;

    [Header("아이콘 설정")]
    public Sprite unlockedIcon;

    [Header("진행 상태 (런타임)")]
    public bool isUnlocked;
    public int currentCount;
}