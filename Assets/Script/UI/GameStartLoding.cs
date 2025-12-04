using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartManager : MonoBehaviour
{
    [Header("애니메이션 설정")]
    public Animator startAnimator;            // 시작 애니메이터
    public string startTriggerName = "Start"; // 애니메이션 트리거 이름
    public float animationDuration = 3f;      // 애니메이션 길이 직접 입력

    [Header("씬 설정")]
    public string mainSceneName = "MainScene"; // 이동할 메인 씬 이름

    [Header("페이드 아웃 설정")]
    public CanvasGroup fadeCanvas;            // 페이드용 CanvasGroup
    public float fadeDuration = 1.5f;         // 페이드 아웃 시간

    private bool isSkipping = false;

    private void Start()
    {
        // 게임 시작과 동시에 애니메이션 실행
        if (startAnimator != null)
        {
            startAnimator.SetTrigger(startTriggerName);
        }

        // 애니메이션 끝나면 씬 이동
        StartCoroutine(LoadSceneAfterAnimation());
    }

    private void Update()
    {
        // 아무 키나 누르면 애니메이션 스킵 + 페이드 아웃
        if (!isSkipping && Input.anyKeyDown)
        {
            isSkipping = true;
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private IEnumerator LoadSceneAfterAnimation()
    {
        yield return new WaitForSeconds(animationDuration);

        if (!isSkipping) // 스킵하지 않았다면 페이드 없이 바로 씬 이동
        {
            SceneManager.LoadScene(mainSceneName);
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        if (fadeCanvas != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                fadeCanvas.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
        }

        SceneManager.LoadScene(mainSceneName);
    }
}