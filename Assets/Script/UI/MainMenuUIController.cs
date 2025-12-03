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

    [Header("메인 이미지들")]
    public RectTransform imageA;
    public RectTransform imageB;

    [Header("로고")]
    public Vector2 imageAPosition = new Vector2(-200, 0);
    public Vector2 imageASize = new Vector2(150, 150);

    [Header("방패")]
    public Vector2 imageBPosition = new Vector2(200, 0);
    public Vector2 imageBSize = new Vector2(200, 200);

    [Header("버튼들")]
    public RectTransform[] menuButtons;   // 메인 메뉴 버튼들
    public float buttonMoveOffset = -300; // 시작 시 숨겨진 위치 오프셋
    public float buttonAnimSpeed = 4f;
    public float buttonDelay = 0.3f;       // 버튼 등장 간격

    [Header("애니메이션 속도")]
    public float moveSpeed = 3f;
    public float sizeSpeed = 3f;

    [Header("팝업 UI")]
    public GameObject[] popups;

    private bool menuStarted = false;
    private Coroutine blinkCoroutine;

    [Header("버튼들")]
    public Button exitButton;       // 나가기 버튼
    public Button sceneMoveButton;  // 특정 씬 이동 버튼
    public string targetSceneName;  // 이동할 씬 이름

    private void Start()
    {
        // 시작 버튼 클릭 이벤트 연결
        if (startButton != null)
            startButton.onClick.AddListener(StartMenuAnimation);

        // 텍스트 깜빡임 시작
        if (startText != null)
            blinkCoroutine = StartCoroutine(BlinkText());

        // 시작 전에는 메뉴 버튼 꺼두기
        foreach (var btn in menuButtons)
        {
            btn.gameObject.SetActive(false);
        }
    }

    private IEnumerator BlinkText()
    {
        while (!menuStarted)
        {
            startText.enabled = !startText.enabled;
            yield return new WaitForSeconds(0.5f); // 0.5초마다 깜빡임
        }
        startText.enabled = true; // 메뉴 시작 후 텍스트는 켜진 상태로 고정
    }

    public void StartMenuAnimation()
    {
        if (menuStarted) return;
        menuStarted = true;

        // 깜빡임 종료
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        // 시작 버튼 숨김
        if (startButton != null)
            startButton.gameObject.SetActive(false);

        // 이미지 연출 시작
        if (imageA != null)
            StartCoroutine(AnimateImage(imageA, imageAPosition, imageASize));
        if (imageB != null)
            StartCoroutine(AnimateImage(imageB, imageBPosition, imageBSize));

        // 버튼 등장 애니메이션 실행 (순차적으로)
        StartCoroutine(ShowButtonsSequentially());
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

    private IEnumerator ShowButtonsSequentially()
    {
        foreach (var btn in menuButtons)
        {
            btn.gameObject.SetActive(true); // 버튼 켜기

            Vector2 startPos = btn.anchoredPosition + new Vector2(0, -200);
            Vector2 targetPos = btn.anchoredPosition;
            btn.anchoredPosition = startPos;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * buttonAnimSpeed;
                btn.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            btn.anchoredPosition = targetPos;

            // 다음 버튼 나오기 전 딜레이
            yield return new WaitForSeconds(buttonDelay);
        }
    }

    public void OnButtonClick(int buttonIndex)
    {
        if (buttonIndex >= 0 && buttonIndex < popups.Length)
        {
            popups[buttonIndex].SetActive(true);
        }
    }

    // 나가기 버튼 동작
    private void OnExitButtonClick()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 테스트 시 종료
#endif
    }

    // 특정 씬 이동 버튼 동작
    private void OnSceneMoveButtonClick()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }

}