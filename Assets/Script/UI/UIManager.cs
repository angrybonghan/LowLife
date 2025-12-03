using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

    [Header("ESC Menu References")]
    public RectTransform escMenuPanel;      // ESC 메뉴 패널 RectTransform
    public CanvasGroup escCanvasGroup;      // ESC 메뉴 페이드용 CanvasGroup
    public Vector2 hiddenPos = new Vector2(0, -600); // 숨겨진 위치
    public Vector2 visiblePos = new Vector2(0, 0);   // 보이는 위치
    public float animSpeed = 5f;            // 슬라이드 속도
    public float fadeSpeed = 5f;            // 페이드 속도

    [Header("Sound UI Buttons")]
    public Button volumeUpButton;
    public Button volumeDownButton;
    public Button[] volumePresetButtons; // 프리셋 버튼들 (예: 20%, 50%, 80%, 100%)

    private bool isPaused = false;
    private Coroutine escAnimCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        if (escMenuPanel != null)
            escMenuPanel.anchoredPosition = hiddenPos;

        if (escCanvasGroup != null)
            escCanvasGroup.alpha = 0f;

        // 버튼 이벤트 연결 (SoundManager 참조)
        if (volumeUpButton != null)
            volumeUpButton.onClick.AddListener(() => SoundManager.instance.IncreaseVolume());

        if (volumeDownButton != null)
            volumeDownButton.onClick.AddListener(() => SoundManager.instance.DecreaseVolume());

        if (volumePresetButtons != null)
        {
            for (int i = 0; i < volumePresetButtons.Length; i++)
            {
                int index = i;
                volumePresetButtons[index].onClick.AddListener(() =>
                {
                    float presetValue = (index + 1) / (float)volumePresetButtons.Length;
                    SoundManager.instance.SetVolume(presetValue);
                });
            }
        }
    }

    private void Update()
    {
        UpdateQuestText();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleESCMenu();
        }
    }


    /// <summary>
    /// ESC 메뉴 토글 (슬라이드 + 페이드 인/아웃)
    /// </summary>
    public void ToggleESCMenu()
    {
        isPaused = !isPaused;

        if (escAnimCoroutine != null) StopCoroutine(escAnimCoroutine);

        if (isPaused)
        {
            Time.timeScale = 0f;
            escAnimCoroutine = StartCoroutine(PlayESCAnimation(visiblePos, 1f));
            SoundManager.instance.LowerVolumeForESC(); // ESC 열릴 때 볼륨 줄임
        }
        else
        {
            Time.timeScale = 1f;
            escAnimCoroutine = StartCoroutine(PlayESCAnimation(hiddenPos, 0f));
            SoundManager.instance.RestoreVolumeAfterESC(); // ESC 닫힐 때 볼륨 복원
        }
    }

    /// <summary>
    /// ESC 메뉴 애니메이션 (슬라이드 + 페이드)
    /// </summary>
    private IEnumerator PlayESCAnimation(Vector2 targetPos, float targetAlpha)
    {
        Vector2 startPos = escMenuPanel.anchoredPosition;
        float startAlpha = escCanvasGroup.alpha;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * animSpeed;
            escMenuPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            escCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t * fadeSpeed);
            yield return null;
        }

        escMenuPanel.anchoredPosition = targetPos;
        escCanvasGroup.alpha = targetAlpha;
    }

    public bool IsPaused => isPaused;


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