using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// UI 매니저
/// - 퀘스트 진행 상황 표시
/// - 업적 달성 팝업 표시
/// - 게임 종료 및 씬 이동 버튼 처리
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI questText;       // 퀘스트 상태 및 진행 상황 표시
    public GameObject achievementPopup;     // 업적 팝업 오브젝트
    public TextMeshProUGUI achievementText; // 업적 팝업 텍스트

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        UpdateQuestText();
    }

    public void UpdateQuestText()
    {
        if (questText == null) return;
        questText.text = "";

        Transform player = GameObject.FindWithTag("Player")?.transform;
        string currentScene = SceneManager.GetActiveScene().name;

        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            QuestState state = QuestManager.Instance.GetQuestState(quest.questID);
            string stateText = state == QuestState.NotStarted ? "미시작" :
                               state == QuestState.InProgress ? "진행 중" : "완료";

            string questLine = $"{quest.questName} - {stateText}";

            if (state == QuestState.InProgress)
            {
                if (quest.questType == QuestType.CombatCount)
                {
                    int killed = QuestManager.Instance.GetKillCount(quest.questID);
                    questLine += $" (처치: {killed}/{quest.targetKillCount})";
                }
                else if (quest.questType == QuestType.CombatClear)
                {
                    EnemyZone zone = FindObjectOfType<EnemyZone>();
                    if (zone != null && zone.questData == quest)
                    {
                        int remaining = zone.GetRemainingEnemies();
                        questLine += $" (남은 적: {remaining})";
                    }
                }
                else if (quest.questType == QuestType.Explore && player != null && currentScene == quest.targetSceneName)
                {
                    float distance = Vector3.Distance(player.position, quest.exploreTargetPosition);
                    questLine += $" (목표까지 {distance:F1}m)";
                }
                else if (quest.questType == QuestType.Escort && player != null && currentScene == quest.escortTargetSceneName)
                {
                    float distance = Vector3.Distance(player.position, quest.escortTargetPosition);
                    questLine += $" (목표까지 {distance:F1}m)";
                }
                else if (quest.questType == QuestType.Collect)
                {
                    int count = ItemDatabase.Instance.GetItemCount(quest.requiredItemID);
                    questLine += $" (수집: {count}/{quest.requiredItemCount})";
                }
                else if (quest.questType == QuestType.Dialogue && player != null)
                {
                    QuestDialogueTrigger[] triggers = GameObject.FindObjectsOfType<QuestDialogueTrigger>();
                    foreach (var trigger in triggers)
                    {
                        if (trigger.questID == quest.questID)
                        {
                            float distance = Vector3.Distance(player.position, trigger.transform.position);
                            questLine += $" (NPC까지 {distance:F1}m)";
                            break;
                        }
                    }
                }
                else if (quest.questType == QuestType.Delivery && player != null)
                {
                    DeliveryQuest[] deliveries = GameObject.FindObjectsOfType<DeliveryQuest>();
                    foreach (var delivery in deliveries)
                    {
                        if (delivery.questID == quest.questID)
                        {
                            float distance = Vector3.Distance(player.position, delivery.transform.position);
                            questLine += $" (전달 NPC까지 {distance:F1}m)";
                            break;
                        }
                    }
                }
            }

            questText.text += questLine + "\n";
        }
    }

    public void ShowAchievementUnlockedSO(AchievementDataSO achievement)
    {
        if (achievementPopup != null && achievementText != null)
        {
            achievementPopup.SetActive(true);
            achievementText.text = $"업적 달성!\n{achievement.title}\n칭호: {achievement.rewardTitle}";
            StartCoroutine(HideAchievementPopupAfterDelay(3f));
        }
    }

    private IEnumerator HideAchievementPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (achievementPopup != null) achievementPopup.SetActive(false);
    }

    public void ShowQuestCompleted(string questName)
    {
        Debug.Log($"[UI] 퀘스트 완료: {questName}");
        // 필요하다면 퀘스트 완료 팝업 UI 추가 가능
    }

    public void QuitGameByButton()
    {
        if (QuestManager.Instance != null)
            QuestSaveSystemJSON.SaveQuests(QuestManager.Instance);

        StartCoroutine(QuitAfterDelay(0.8f));
    }

    private IEnumerator QuitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadSceneByButton(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}