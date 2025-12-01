using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 업적 처리 매니저
/// - 씬 간 유지(DontDestroyOnLoad)
/// - 이벤트 기반으로 업적 진행/달성 처리
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;

    [Header("등록된 업적들 (ScriptableObject 리스트)")]
    public List<AchievementDataSO> achievements = new List<AchievementDataSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // 공통: 반복(횟수 기반) 진행 처리 유틸리티
    private void IncrementProgress(AchievementType type, System.Func<AchievementDataSO, bool> filter = null, int add = 1)
    {
        foreach (var ach in achievements)
        {
            if (ach.isUnlocked || ach.type != type) continue;
            if (filter != null && !filter(ach)) continue;

            ach.currentCount += add;
            if (ach.currentCount >= ach.targetCount)
            {
                UnlockAchievement(ach);
            }
        }
    }

    // 공통: 단발(즉시 달성) 처리 유틸리티
    private void CompleteInstant(AchievementType type, System.Func<AchievementDataSO, bool> filter = null)
    {
        foreach (var ach in achievements)
        {
            if (ach.isUnlocked || ach.type != type) continue;
            if (filter != null && !filter(ach)) continue;

            UnlockAchievement(ach);
        }
    }

    // 적 처치
    public void OnEnemyKilled(string enemyType)
    {
        IncrementProgress(AchievementType.KillEnemy, ach => ach.targetEnemyType == enemyType);
    }

    // 아이템 수집 (여러 개 한 번에)
    public void OnItemCollected(string itemID, int count = 1)
    {
        IncrementProgress(AchievementType.CollectItem, ach => ach.targetItemID == itemID, count);
    }

    // 퀘스트 완료 (즉시 달성)
    public void OnQuestCompleted(string questID)
    {
        CompleteInstant(AchievementType.CompleteQuest, ach => ach.targetQuestID == questID);
    }

    // 패링/방어/순간이동/공격 (횟수 기반)
    public void OnParrySuccess() => IncrementProgress(AchievementType.ParrySuccess);
    public void OnBlockSuccess() => IncrementProgress(AchievementType.BlockSuccess);
    public void OnTeleportUse() => IncrementProgress(AchievementType.TeleportUse);
    public void OnAttackSuccess() => IncrementProgress(AchievementType.AttackSuccess);

    // 비밀의 방 발견 (씬이 달라도 누적)
    public void OnDiscoverSecretRoom() => IncrementProgress(AchievementType.DiscoverSecretRoom);

    // 숨겨진 NPC/엔딩 보기 (즉시 달성)
    public void OnDiscoverNPC() => CompleteInstant(AchievementType.DiscoverNPC);
    public void OnViewEnding() => CompleteInstant(AchievementType.ViewEnding);

    // 특정 씬 진입 (씬 이름 정확히 비교)
    public void OnSceneEntered(string sceneName)
    {
        CompleteInstant(AchievementType.EnterScene, ach => ach.targetSceneName == sceneName);
    }

    // 업적 달성 처리
    private void UnlockAchievement(AchievementDataSO ach)
    {
        ach.isUnlocked = true;
        Debug.Log($"[업적 달성] {ach.title} - {ach.description}");

        UIManager.Instance?.ShowAchievementUnlockedSO(ach);
    }
}