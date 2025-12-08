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
/// - ESC 메뉴 및 서브 창 관리
/// - 볼륨 조절 (버튼 + 키보드)
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public bool IsPaused => isPaused;

    [Header("UI References")]
    public TextMeshProUGUI questText;
    public RectTransform achievementPopupPanel;
    public TextMeshProUGUI achievementTitleText;
    public TextMeshProUGUI achievementDescText;

    [Header("Achievement Popup Animation")]
    public Vector2 popupHiddenPos = new Vector2(0, -400);
    public Vector2 popupVisiblePos = new Vector2(0, 0);
    public float popupAnimSpeed = 6f;
    public float achievementPopupDuration = 3f;

    private Coroutine achievementAnimCoroutine;

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

    [Header("ESC Sub Windows")]
    public GameObject settingsWindow;
    public GameObject achievementWindow;
    private GameObject activeSubWindow;

    [Header("Sound UI Buttons")]
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        string lastTime = PlayerPrefs.GetString("LastQuestSaveTime", "저장 기록 없음");
        UpdateSaveTimeText(lastTime);

        // ESC 메뉴 기본 닫힘 상태
        if (escMenuPanel != null)
            escMenuPanel.anchoredPosition = hiddenPos;
        if (escCanvasGroup != null)
            escCanvasGroup.alpha = 0f;

        // 사이드 패널 기본 닫힘 상태
        if (sidePanel != null)
        {
            sidePanel.anchoredPosition = sideHiddenPos;
            sidePanel.localRotation = Quaternion.Euler(0f, 90f, 0f);
        }

        // ESC 관련 서브 창들 닫기
        if (settingsWindow != null) settingsWindow.SetActive(false);
        if (achievementWindow != null) achievementWindow.SetActive(false);

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

        SyncPresetButtonImages();
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
            Destroy(gameObject);
    }

    private void Start()
    {
        string lastTime = PlayerPrefs.GetString("LastQuestSaveTime", "저장 기록 없음");
        UpdateSaveTimeText(lastTime);
    }

    private void Update()
    {
        UpdateQuestText();

        string lastTime = PlayerPrefs.GetString("LastQuestSaveTime", "저장 기록 없음");
        UpdateSaveTimeText(lastTime);

        // ESC 메뉴 입력 처리
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

            if (Input.GetKeyDown(KeyCode.Return))
            {
                escMenuButtons[selectedIndex].onClick.Invoke();
            }
        }

        // Tab 키로 사이드 패널 열고 닫기
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!sideOpen)
            {
                sideOpen = true;
                if (sideAnimCoroutine != null) StopCoroutine(sideAnimCoroutine);
                sideAnimCoroutine = StartCoroutine(PlaySidePanelAnimation(true));
            }
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (sideOpen)
            {
                sideOpen = false;
                if (sideAnimCoroutine != null) StopCoroutine(sideAnimCoroutine);
                sideAnimCoroutine = StartCoroutine(PlaySidePanelAnimation(false));
            }
        }

        // 좌/우 키로 볼륨 조절
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SoundManager.instance.DecreaseVolume();
            SyncPresetButtonImages();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SoundManager.instance.IncreaseVolume();
            SyncPresetButtonImages();
        }
    }

    private void UpdateESCMenuSelection()
    {
        for (int i = 0; i < escMenuButtons.Length; i++)
        {
            var colors = escMenuButtons[i].colors;
            colors.normalColor = (i == selectedIndex) ? Color.yellow : Color.white;
            escMenuButtons[i].colors = colors;
        }
    }

    private void SyncPresetButtonImages()
    {
        float currentVolume = 1f; // 기본값

        // SoundManager 인스턴스 null 체크
        if (SoundManager.instance != null)
        {
            currentVolume = SoundManager.instance.GetVolume();
        }
        else
        {
            Debug.LogWarning("SoundManager 인스턴스가 없음! 기본 볼륨값 사용");
        }

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
                btnImage.sprite = (i <= activeIndex) ? onSprite : offSprite;
        }
    }

    public void ToggleESCMenu()
    {
        // ESC 누를 때 서브 창이 열려 있으면 먼저 닫기
        if (activeSubWindow != null)
        {
            CloseActiveSubWindow();
            return;
        }

        isPaused = !isPaused;
        if (escAnimCoroutine != null) StopCoroutine(escAnimCoroutine);

        if (isPaused)
        {
            Time.timeScale = 0f;
            escAnimCoroutine = StartCoroutine(PlayESCAnimation(visiblePos, 1f));
            SoundManager.instance.LowerVolumeForESC();
        }
        else
        {
            Time.timeScale = 1f;
            escAnimCoroutine = StartCoroutine(PlayESCAnimation(hiddenPos, 0f));
            SoundManager.instance.RestoreVolumeAfterESC();
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

        Vector2 targetPos = open ? sideVisiblePos : sideHiddenPos;
        Quaternion targetRot = open ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, 0f, 90f);

        if (open)
        {
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
            if (state != QuestState.InProgress) continue;

            string questLine = $"{quest.questName} - 진행 중";

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
                questLine += $" (목표까지 {distance:F1}cm)";
            }
            else if (quest.questType == QuestType.Escort && player != null && currentScene == quest.escortTargetSceneName)
            {
                float distance = Vector3.Distance(player.position, quest.escortTargetPosition);
                questLine += $" (목표까지 {distance:F1}cm)";
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
                        questLine += $" (NPC까지 {distance:F1}cm)";
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
                        questLine += $" (전달 NPC까지 {distance:F1}cm)";
                        break;
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

        if (!sideOpen)
        {
            sideOpen = true;
            if (sideAnimCoroutine != null) StopCoroutine(sideAnimCoroutine);
            sideAnimCoroutine = StartCoroutine(PlaySidePanelAnimation(true));
        }

        if (questText != null)
            questText.text = $"퀘스트 완료!\n{questName}\n다음 퀘스트를 진행하세요.";

        StartCoroutine(HideQuestClearEffectAfterDelay(3f));
    }

    private IEnumerator HideQuestClearEffectAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (sideOpen)
        {
            sideOpen = false;
            if (sideAnimCoroutine != null) StopCoroutine(sideAnimCoroutine);
            sideAnimCoroutine = StartCoroutine(PlaySidePanelAnimation(false));
        }

        UpdateQuestText();
    }

    public void ShowAchievementUnlocked(string title, string description)
    {
        achievementPopupPanel.gameObject.SetActive(true);
        achievementTitleText.text = title;
        achievementDescText.text = description;

        if (achievementAnimCoroutine != null) StopCoroutine(achievementAnimCoroutine);
        achievementAnimCoroutine = StartCoroutine(PlayAchievementPopupAnimation(true));

        StartCoroutine(HideAchievementPopupAfterDelay(achievementPopupDuration));
    }

    private IEnumerator HideAchievementPopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideAchievementPopup();
    }

    private IEnumerator PlayAchievementPopupAnimation(bool show)
    {
        Vector2 startPos = achievementPopupPanel.anchoredPosition;
        Vector2 targetPos = show ? popupVisiblePos : popupHiddenPos;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * popupAnimSpeed;
            achievementPopupPanel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        achievementPopupPanel.anchoredPosition = targetPos;

        if (!show)
            achievementPopupPanel.gameObject.SetActive(false);
    }

    public void HideAchievementPopup()
    {
        if (achievementAnimCoroutine != null) StopCoroutine(achievementAnimCoroutine);
        achievementAnimCoroutine = StartCoroutine(PlayAchievementPopupAnimation(false));
    }

    public void ExitToMainMenu()
    {
        string currentStage = SceneManager.GetActiveScene().name;
        SaveSystemJSON.DataSaveStage(currentStage);
        SaveSystemJSON.DataSaveQuests(QuestManager.Instance);
        SceneManager.LoadScene("MainMenu");
    }

    private void CloseActiveSubWindow()
    {
        if (activeSubWindow != null)
        {
            activeSubWindow.SetActive(false);
            activeSubWindow = null;
        }
    }

    public void OpenSettingsWindow()
    {
        CloseActiveSubWindow();
        settingsWindow.SetActive(true);
        activeSubWindow = settingsWindow;
    }

    public void OpenAchievementWindow()
    {
        CloseActiveSubWindow();
        achievementWindow.SetActive(true);
        activeSubWindow = achievementWindow;
    }
}