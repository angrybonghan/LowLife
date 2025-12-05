using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIController : MonoBehaviour
{
    [Header("시작 버튼")]
    public Button startButton;
    public TextMeshProUGUI startText;

    [Header("메인 이미지")]
    public RectTransform imageLogo;                // 로고 이미지 
    public RectTransform[] imageShield;            // 방패 이미지 

    [Header("로고 설정")]
    public Vector2 imageAPosition = new Vector2(-200, 0); // 로고 최종 위치
    public Vector2 imageASize = new Vector2(150, 150);    // 로고 최종 크기

    [Header("방패 위치 설정")]
    public Vector2 imageBPosition = new Vector2(200, 0);   // 방패 최종 위치
    public Vector3 shieldDefaultPosition = new Vector3(200, 0, 0); // 기본 방패 위치 (팝업 닫을 때 돌아올 위치)

    [Header("방패 크기 설정")]
    public Vector2 imageBSize = new Vector2(200, 200);     // 방패 최종 크기
    public Vector2 shieldStartSize = new Vector2(150, 150); // 방패 회전 시작 크기
    public Vector2 shieldEndSize = new Vector2(200, 200);   // 방패 회전 종료 크기

    [Header("방패 애니메이션 속도")]
    public float shieldMoveSpeed = 3f;                     // 방패 이동 속도
    public float shieldRotationTime = 1f;                  // 방패 회전 시간

    [Header("버튼 설정")]
    public RectTransform[] menuButtons;            // 메뉴 버튼들
    public Vector3[] targetPositions;              // 버튼별 최종 위치값
    public float buttonAnimSpeed = 4f;             // 버튼 애니메이션 속도

    [Header("애니메이션 속도")]
    public float moveSpeed = 3f;                   // 이미지 이동 속도
    public float sizeSpeed = 3f;                   // 이미지 크기 변경 속도

    [Header("팝업 설정")]
    public RectTransform[] popups;                 // 팝업 UI RectTransform 배열
    public Vector3[] popupTargetPositions;         // 팝업별 최종 위치값
    public float popupAnimSpeed = 4f;              // 팝업 애니메이션 속도
    public Vector3 popupStartScale = Vector3.zero; // 팝업 시작 크기
    public Vector3 popupEndScale = Vector3.one;    // 팝업 최종 크기
    public Vector3[] popupShieldPositions;         // 팝업별 방패 위치값

    [Header("내부 상태 관리")]
    private bool menuStarted = false;              // 메뉴 시작 여부 플래그
    private Coroutine blinkCoroutine;              // 텍스트 깜빡임 코루틴 참조

    [Header("사운드 조절")]
    public Button volumeUpButton;        // 볼륨 증가 버튼
    public Button volumeDownButton;      // 볼륨 감소 버튼
    public Button[] volumePresetButtons; // 볼륨 프리셋 버튼들
    public Sprite onSprite;              // 활성화 이미지
    public Sprite offSprite;             // 비활성화 이미지

    private void Awake()
    {
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

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartMenuAnimation);

        if (startText != null)
            blinkCoroutine = StartCoroutine(BlinkText());

        foreach (var btn in menuButtons)
            btn.gameObject.SetActive(false);

        foreach (var popup in popups)
            popup.gameObject.SetActive(false);
    }

    private void Update()
    {
        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 열려 있는 팝업이 있으면 닫기
            for (int i = 0; i < popups.Length; i++)
            {
                if (popups[i].gameObject.activeSelf)
                {
                    ClosePopup(i);
                    break; // 하나만 닫고 종료
                }
            }
        }
    }

    private IEnumerator BlinkText()
    {
        while (!menuStarted)
        {
            startText.enabled = !startText.enabled;
            yield return new WaitForSeconds(0.5f);
        }
        startText.enabled = true;
    }

    public void StartMenuAnimation()
    {
        if (menuStarted) return;
        menuStarted = true;

        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        if (startButton != null)
            startButton.gameObject.SetActive(false);

        if (imageLogo != null)
            StartCoroutine(AnimateImage(imageLogo, imageAPosition, imageASize));
        if (imageShield != null)
        {
            StartCoroutine(AnimateImage(imageShield[0], imageBPosition, imageBSize));
            StartCoroutine(AnimateImage(imageShield[1], imageBPosition, imageBSize));
        }

        StartCoroutine(ShowButtonsFromCenter());
    }

    private IEnumerator AnimateImage(RectTransform target, Vector2 targetPos, Vector2 targetSize)
    {
        Vector2 startPos = target.anchoredPosition;
        Vector2 startSize = target.sizeDelta;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            target.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            target.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
            yield return null;
        }

        target.anchoredPosition = targetPos;
        target.sizeDelta = targetSize;
    }

    private IEnumerator ShowButtonsFromCenter()
    {
        foreach (var btn in menuButtons)
        {
            btn.gameObject.SetActive(true);
            btn.localPosition = Vector3.zero;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * buttonAnimSpeed;
            for (int i = 0; i < menuButtons.Length; i++)
                menuButtons[i].localPosition = Vector3.Lerp(Vector3.zero, targetPositions[i], t);
            yield return null;
        }

        for (int i = 0; i < menuButtons.Length; i++)
            menuButtons[i].localPosition = targetPositions[i];
    }

    public void OnButtonClick(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < popups.Length)
            StartCoroutine(ButtonsIntoShield(buttonIndex));
    }

    private IEnumerator ButtonsIntoShield(int buttonIndex)
    {
        Vector3[] startPositions = new Vector3[menuButtons.Length];
        for (int i = 0; i < menuButtons.Length; i++)
            startPositions[i] = menuButtons[i].localPosition;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * buttonAnimSpeed;
            for (int i = 0; i < menuButtons.Length; i++)
                menuButtons[i].localPosition = Vector3.Lerp(startPositions[i], Vector3.zero, t);
            yield return null;
        }

        yield return StartCoroutine(RotateAndMoveShield(popupShieldPositions[buttonIndex], true));

        yield return StartCoroutine(ShowPopupFromCenter(buttonIndex));
    }

    private IEnumerator RotateAndMoveShield(Vector3 targetPos, bool forward)
    {
        Vector3 startPos = imageShield[0].anchoredPosition;

        // 회전 방향에 따라 크기 변화도 반대로 적용
        Vector2 startSize = forward ? shieldStartSize : shieldEndSize;
        Vector2 endSize = forward ? shieldEndSize : shieldStartSize;

        float elapsed = 0f;

        while (elapsed < shieldRotationTime)
        {
            elapsed += Time.deltaTime;
            float ratio = elapsed / shieldRotationTime;

            float angle = forward ? 360f * ratio : -360f * ratio;
            imageShield[0].localRotation = Quaternion.Euler(0, 0, angle);
            imageShield[1].localRotation = Quaternion.Euler(0, 0, angle);

            imageShield[0].anchoredPosition = Vector3.Lerp(startPos, targetPos, ratio);
            imageShield[1].anchoredPosition = Vector3.Lerp(startPos, targetPos, ratio);

            imageShield[0].sizeDelta = Vector2.Lerp(startSize, endSize, ratio);
            imageShield[1].sizeDelta = Vector2.Lerp(startSize, endSize, ratio);

            yield return null;
        }

        imageShield[0].localRotation = Quaternion.identity;
        imageShield[1].localRotation = Quaternion.identity;
        imageShield[0].anchoredPosition = targetPos;
        imageShield[1].anchoredPosition = targetPos;
        imageShield[0].sizeDelta = endSize;
        imageShield[1].sizeDelta = endSize;
    }

    private IEnumerator ShowPopupFromCenter(int popupIndex)
    {
        RectTransform popup = popups[popupIndex];
        popup.gameObject.SetActive(true);

        Vector3 startPos = Vector3.zero;
        Vector3 targetPos = popupTargetPositions[popupIndex];
        popup.localPosition = startPos;

        popup.localScale = popupStartScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * popupAnimSpeed;
            popup.localPosition = Vector3.Lerp(startPos, targetPos, t);
            popup.localScale = Vector3.Lerp(popupStartScale, popupEndScale, t);
            yield return null;
        }

        popup.localPosition = targetPos;
        popup.localScale = popupEndScale;
    }

    public void ClosePopup(int popupIndex)
    {
        if (popupIndex >= 0 && popupIndex < popups.Length)
            StartCoroutine(HidePopupIntoCenter(popupIndex));
    }

    private IEnumerator HidePopupIntoCenter(int popupIndex)
    {
        RectTransform popup = popups[popupIndex];
        Vector3 startPos = popup.localPosition;
        Vector3 targetPos = Vector3.zero;

        Vector3 startScale = popupEndScale;
        Vector3 targetScale = popupStartScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * popupAnimSpeed;
            popup.localPosition = Vector3.Lerp(startPos, targetPos, t);
            popup.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        popup.localPosition = targetPos;
        popup.localScale = targetScale;
        popup.gameObject.SetActive(false);

        // 방패 반대 방향 회전 + 원래 위치 복귀
        yield return StartCoroutine(RotateAndMoveShield(shieldDefaultPosition, false));

        // 버튼 다시 등장
        yield return StartCoroutine(ShowButtonsFromCenter());
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


    private IEnumerator QuitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void QuitGameByButton()
    {
        if (QuestManager.Instance != null)
            QuestSaveSystemJSON.SaveQuests(QuestManager.Instance);

        StartCoroutine(QuitAfterDelay(0.8f));
    }

    public void LoadSceneByButton(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}