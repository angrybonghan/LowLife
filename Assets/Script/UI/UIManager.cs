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

    public LockMouse lockMouse; // LockMouse 스크립트 참조

    [Header("UI References")]
    public TextMeshProUGUI questText;
    public GameObject achievementPopup;
    public TextMeshProUGUI achievementText;

    [Header("ESC Menu References")]
    public RectTransform escMenuPanel;
    public CanvasGroup escCanvasGroup;
    public Vector2 hiddenPos = new Vector2(0, -600);
    public Vector2 visiblePos = new Vector2(0, 0);
    public float animSpeed = 5f;
    public float fadeSpeed = 5f;

    [Header("Sound UI Buttons")]
    public Button volumeUpButton;
    public Button volumeDownButton;
    public Button[] volumePresetButtons;

    [Header("Side Panel References")]
    public RectTransform sidePanel;          // 사이드 패널 RectTransform
    public Vector2 sideHiddenPos = new Vector2(-600f, 0f); // 숨김 위치
    public Vector2 sideVisiblePos = new Vector2(0f, 0f);   // 표시 위치
    public float sideAnimSpeed = 6f;
    public float sideRotateSpeed = 6f;

    private bool isPaused = false;
    private Coroutine escAnimCoroutine;

    private bool sideOpen = false;
    private Coroutine sideAnimCoroutine;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        if (escMenuPanel != null)
            escMenuPanel.anchoredPosition = hiddenPos;
        if (escCanvasGroup != null)
            escCanvasGroup.alpha = 0f;

        if (sidePanel != null)
        {
            sidePanel.anchoredPosition = sideHiddenPos;
            sidePanel.localRotation = Quaternion.Euler(0f, 90f, 0f); // 처음엔 90도 회전된 상태
        }

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

            if (lockMouse != null)
            {
                lockMouse.UnlockMouseForUI(); // 커서 해제
            }
        }

        // Tab 누르고 있는 동안 열림
        if (Input.GetKey(KeyCode.Tab))
        {
            

            if (!sideOpen)
            {
                sideOpen = true;
                if (sideAnimCoroutine != null) StopCoroutine(sideAnimCoroutine);
                sideAnimCoroutine = StartCoroutine(PlaySidePanelAnimation(true));
            }
        }

        // Tab 뗄 때 닫힘
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (sideOpen)
            {
                sideOpen = false;
                if (sideAnimCoroutine != null) StopCoroutine(sideAnimCoroutine);
                sideAnimCoroutine = StartCoroutine(PlaySidePanelAnimation(false));
            }
        }

    }

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

    public void ToggleSidePanel()
    {
        sideOpen = !sideOpen;

        if (sideAnimCoroutine != null) StopCoroutine(sideAnimCoroutine);
        sideAnimCoroutine = StartCoroutine(PlaySidePanelAnimation(sideOpen));
    }

    private IEnumerator PlaySidePanelAnimation(bool open)
    {
        Vector2 startPos = sidePanel.anchoredPosition;
        Quaternion startRot = sidePanel.localRotation;

        // 목표 상태
        Vector2 targetPos = open ? sideVisiblePos : sideHiddenPos;
        Quaternion targetRot = open ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, 0f, 90f);

        if (!open)
        {
            // 열릴 때: 회전 먼저 → 이동
            float tRotate = 0f;
            while (tRotate < 1f)
            {
                tRotate += Time.unscaledDeltaTime * sideRotateSpeed;
                sidePanel.localRotation = Quaternion.Slerp(startRot, targetRot, Mathf.Clamp01(tRotate));
                yield return null;
            }

            float tSlide = 0f;
            while (tSlide < 1f)
            {
                tSlide += Time.unscaledDeltaTime * sideAnimSpeed;
                sidePanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.Clamp01(tSlide));
                yield return null;
            }
        }
        else
        {
            // 닫힐 때: 이동 먼저 → 회전
            float tSlide = 0f;
            while (tSlide < 1f)
            {
                tSlide += Time.unscaledDeltaTime * sideAnimSpeed;
                sidePanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, Mathf.Clamp01(tSlide));
                yield return null;
            }

            float tRotate = 0f;
            while (tRotate < 1f)
            {
                tRotate += Time.unscaledDeltaTime * sideRotateSpeed;
                sidePanel.localRotation = Quaternion.Slerp(startRot, targetRot, Mathf.Clamp01(tRotate));
                yield return null;
            }
        }

        // 최종 보정
        sidePanel.anchoredPosition = targetPos;
        sidePanel.localRotation = targetRot;
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

    public void LoadSceneByButton(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}