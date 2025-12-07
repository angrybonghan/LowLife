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

    [Header("ESC Menu Buttons")]
    public Button[] escMenuButtons;
    private int selectedIndex = 0;

    [Header("Sound UI Buttons")]
    public Button volumeUpButton;
    public Button volumeDownButton;
    public Button[] volumePresetButtons;
    public Sprite onSprite;
    public Sprite offSprite;

    [Header("Side Panel References")]
    public RectTransform sidePanel;
    public Vector2 sideHiddenPos = new Vector2(-600f, 0f);
    public Vector2 sideVisiblePos = new Vector2(0f, 0f);
    public float sideAnimSpeed = 6f;
    public float sideRotateSpeed = 6f;

    private bool isPaused = false;
    private Coroutine escAnimCoroutine;

    private bool sideOpen = false;
    private Coroutine sideAnimCoroutine;

    public TextMeshProUGUI saveTimeText; // 마지막 저장 시간 표시용



    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        // 저장된 마지막 퀘스트 저장 시간 불러와서 UI에 반영
        string lastTime = PlayerPrefs.GetString("LastQuestSaveTime", "저장 기록 없음");
        UpdateSaveTimeText(lastTime);

        if (escMenuPanel != null)
            escMenuPanel.anchoredPosition = hiddenPos;
        if (escCanvasGroup != null)
            escCanvasGroup.alpha = 0f;

        if (sidePanel != null)
        {
            sidePanel.anchoredPosition = sideHiddenPos;
            sidePanel.localRotation = Quaternion.Euler(0f, 90f, 0f); // 처음엔 90도 회전된 상태
        }

        // 볼륨 업 버튼
        if (volumeUpButton != null)
            volumeUpButton.onClick.AddListener(() =>
            {
                SoundManager.instance.IncreaseVolume();
                SyncPresetButtonImages();
            });

        // 볼륨 다운 버튼
        if (volumeDownButton != null)
            volumeDownButton.onClick.AddListener(() =>
            {
                SoundManager.instance.DecreaseVolume();
                SyncPresetButtonImages();
            });

        // 프리셋 버튼들
        if (volumePresetButtons != null)
        {
            for (int i = 0; i < volumePresetButtons.Length; i++)
            {
                int index = i;
                volumePresetButtons[index].onClick.AddListener(() =>
                {
                    float presetValue = (index + 1) / (float)volumePresetButtons.Length;
                    SoundManager.instance.SetVolume(presetValue);
                    UpdatePresetButtonImages(index);
                });
            }
        }

        // 씬 시작 시 저장된 볼륨 값에 맞춰 버튼 이미지 초기화
        SyncPresetButtonImages();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 씬 로드 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 특정 씬(MainMenu)에서는 UIManager 제거
        if (scene.name == "MainMenu") // 메인 메뉴 씬 이름에 맞게 수정
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 게임 시작 시 퀘스트 저장 기록 불러오기
        string lastTime = PlayerPrefs.GetString("LastQuestSaveTime", "저장 기록 없음");
        UpdateSaveTimeText(lastTime);
    }

    private void Update()
    {
        UpdateQuestText();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleESCMenu();
        }

        if (isPaused && escMenuButtons.Length > 0)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                selectedIndex = (selectedIndex - 1 + escMenuButtons.Length) % escMenuButtons.Length;
                UpdateESCMenuSelection();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                selectedIndex = (selectedIndex + 1) % escMenuButtons.Length;
                UpdateESCMenuSelection();
            }

            // Enter 키로 선택된 버튼 실행
            if (Input.GetKeyDown(KeyCode.Return))
            {
                escMenuButtons[selectedIndex].onClick.Invoke();
            }
        }

        // Tab 누르고 있는 동안 열림
        if (Input.GetKeyDown(KeyCode.Tab))
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

    private void UpdateESCMenuSelection()
    {
        for (int i = 0; i < escMenuButtons.Length; i++)
        {
            var colors = escMenuButtons[i].colors;
            if (i == selectedIndex)
            {
                colors.normalColor = Color.yellow; // 선택된 버튼 강조 색
            }
            else
            {
                colors.normalColor = Color.white; // 기본 색
            }
            escMenuButtons[i].colors = colors;
        }
    }

    private void SyncPresetButtonImages()
    {
        float currentVolume = SoundManager.instance.GetVolume();
        int activeIndex = Mathf.RoundToInt(currentVolume * volumePresetButtons.Length) - 1;
        activeIndex = Mathf.Clamp(activeIndex, 0, volumePresetButtons.Length - 1);

        UpdatePresetButtonImages(activeIndex);
    }

    private void UpdatePresetButtonImages(int activeIndex)
    {
        for (int i = 0; i < volumePresetButtons.Length; i++)
        {
            Image btnImage = volumePresetButtons[i].GetComponent<Image>();
            if (btnImage != null)
            {
                btnImage.sprite = (i <= activeIndex) ? onSprite : offSprite;
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

        if (open)
        {
            
            // 이동 먼저 -> 회전
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
        else
        {
            // 회전 먼저 -> 이동
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

            if (state != QuestState.InProgress) continue;


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

    public void UpdateSaveTimeText(string time)
    {
        if (saveTimeText != null)
            saveTimeText.text = $"마지막 저장: {time}";
    }

    public void ShowQuestCompleted(string questName)
    {
        Debug.Log($"[UI] 퀘스트 완료: {questName}");

        // 사이드 패널 열기 애니메이션
        if (!sideOpen)
        {
            sideOpen = true;
            if (sideAnimCoroutine != null) StopCoroutine(sideAnimCoroutine);
            sideAnimCoroutine = StartCoroutine(PlaySidePanelAnimation(true));
        }

        // 클리어 메시지 표시
        if (questText != null)
        {
            questText.text = $"퀘스트 완료!\n{questName}\n다음 퀘스트를 진행하세요.";
        }

        // 일정 시간 뒤 닫고 다음 퀘스트 표시
        StartCoroutine(HideQuestClearEffectAfterDelay(3f));
    }

    private IEnumerator HideQuestClearEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 사이드 패널 닫기
        if (sideOpen)
        {
            sideOpen = false;
            if (sideAnimCoroutine != null) StopCoroutine(sideAnimCoroutine);
            sideAnimCoroutine = StartCoroutine(PlaySidePanelAnimation(false));
        }

        // 다음 진행 중인 퀘스트 표시
        UpdateQuestText();
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

    public void ExitToMainMenu()
    {
        // 현재 진행 중인 스테이지 저장
        string currentStage = SceneManager.GetActiveScene().name;
        SaveSystemJSON.DataSaveStage(currentStage);

        // 퀘스트 상태 저장
        SaveSystemJSON.DataSaveQuests(QuestManager.Instance);

        // 메인 메뉴 씬으로 이동
        SceneManager.LoadScene("MainMenu");
    }
}