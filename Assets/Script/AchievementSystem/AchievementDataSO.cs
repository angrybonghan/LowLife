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

    // 단발(이벤트 기반)
    CompleteQuest,     // 특정 퀘스트 완료 (questID)
    DiscoverNPC,       // 숨겨진 NPC 발견 (이벤트 발생 시 즉시 달성)
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
    [Tooltip("업적 식별용 고유 ID (저장/네트워크/디버그용 키)")]
    public string achievementID;

    [Tooltip("UI에 표시될 업적 이름")]
    public string title;

    [Tooltip("UI에 표시될 업적 설명")]
    [TextArea] public string description;

    [Tooltip("업적 달성 시 표시/지급될 리워드 명")]
    public string rewardTitle;

    [Header("조건 타입")]
    [Tooltip("업적 달성 조건 타입")]
    public AchievementType type;

    [Header("조건 파라미터")]
    [Tooltip("KillEnemy 조건일 때 목표 적 타입 (Enemy.enemyType과 동일 문자열)")]
    public string targetEnemyType;

    [Tooltip("CollectItem 조건일 때 목표 아이템 ID")]
    public string targetItemID;

    [Tooltip("CompleteQuest 조건일 때 목표 퀘스트 ID")]
    public string targetQuestID;

    [Tooltip("EnterScene 조건일 때 목표 씬 이름 (Scene.name과 동일)")]
    public string targetSceneName;

    [Tooltip("반복(횟수 기반) 조건의 목표 수치 (예: 100번, 50개 등)")]
    public int targetCount;

    [Header("진행 상태 (런타임)")]
    [Tooltip("업적 달성 여부")]
    public bool isUnlocked;

    [Tooltip("현재 진행 카운트 (반복 조건용)")]
    public int currentCount;
}