using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenTransition : MonoBehaviour
{
    public static ScreenTransition Instance { get; private set; }
    public static bool isTransitioning = false;
    public static bool isRoading = false;
    public static bool isNextSceneLoaded = false;

    private GameObject[] uiObjects;

    private string SceneName;
    private string LoadingSceneName;
    private Color CurtainColor;
    private float WaitTime1;
    private float FadeOutTime;
    private float LoadingTime;
    private float FadeInTime;
    private float WaitTime2;

    private GameObject currentCurtain;
    private Image curtainImage;

    // 저장 여부 플래그
    private bool shouldSaveData = true;

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
        }
    }

    public static void ScreenTransitionGoto(string SceneName, string LoadingSceneName, Color CurtainColor,
        float WaitTime1, float FadeOutTime, float LoadingTime, float FadeInTime, float WaitTime2, bool saveData = true)
    {
        if (Instance == null)
        {
            Debug.LogError("ScreenTransition 인스턴스가 존재하지 않음. DontDestroyOnLoad된 오브젝트를 확인하세요.");
            return;
        }

        if (isTransitioning) return;

        DialogManager.instance?.StopDialogue();

        Instance.SceneName = SceneName;
        Instance.LoadingSceneName = LoadingSceneName;
        Instance.CurtainColor = CurtainColor;
        Instance.WaitTime1 = WaitTime1;
        Instance.FadeOutTime = FadeOutTime;
        Instance.LoadingTime = LoadingTime;
        Instance.FadeInTime = FadeInTime;
        Instance.WaitTime2 = WaitTime2;
        Instance.shouldSaveData = saveData; // 저장 여부 설정

        Instance.StartTransitionInternal();
    }

    private void StartTransitionInternal()
    {
        isTransitioning = true;
        isRoading = false;
        isNextSceneLoaded = false;

        if (!FindCurtain())
        {
            Debug.LogError("현재 씬에서 'Curtain' 태그를 가진 Image 컴포넌트를 찾을 수 없음");
            isTransitioning = false;
            return;
        }

        Color initialColor = CurtainColor;
        curtainImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);

        StartCoroutine(StartTransition());
    }

    private IEnumerator StartTransition()
    {
        SetUI();
        SetUIObjectsActive(false);

        if (WaitTime1 > 0)
        {
            yield return new WaitForSeconds(WaitTime1);
        }

        yield return StartCoroutine(FadeCurtain(curtainImage, 1f, FadeOutTime));

        if (LoadingTime <= 0f)
        {
            SceneManager.LoadScene(SceneName);
            yield return null;

            yield return StartCoroutine(FinishTransition());
        }
        else
        {
            SceneManager.LoadScene(LoadingSceneName);
            yield return null;

            StartCoroutine(Loading());
        }
    }

    private IEnumerator Loading()
    {
        isRoading = true;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneName);
        asyncLoad.allowSceneActivation = false;

        yield return new WaitForSeconds(LoadingTime);

        while (asyncLoad.progress < 0.9f) yield return null;

        asyncLoad.allowSceneActivation = true;
        yield return null;
        isRoading = false;

        StartCoroutine(FinishTransition());
    }

    private IEnumerator FinishTransition()
    {
        isNextSceneLoaded = true;

        if (!FindCurtain())
        {
            Debug.LogError("현재 씬에서 'Curtain' 태그를 가진 Image 컴포넌트를 찾을 수 없음");
            isTransitioning = false;
            yield break;
        }

        SetUI();
        SetUIObjectsActive(false);

        Color initialColor = CurtainColor;
        curtainImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, 1f);

        yield return StartCoroutine(FadeCurtain(curtainImage, 0f, FadeInTime));

        if (WaitTime2 > 0)
        {
            yield return new WaitForSeconds(WaitTime2);
        }

        SetUIObjectsActive(true);
        isTransitioning = false;
    }

    private bool FindCurtain()
    {
        currentCurtain = GameObject.FindWithTag("Curtain");
        if (currentCurtain != null)
        {
            curtainImage = currentCurtain.GetComponent<Image>();
            return curtainImage != null;
        }
        curtainImage = null;
        return false;
    }

    private void SetUI()
    {
        uiObjects = GameObject.FindGameObjectsWithTag("UI");
    }

    private void SetUIObjectsActive(bool isActive)
    {
        foreach (GameObject ui in uiObjects)
        {
            if (ui != null)
            {
                ui.SetActive(isActive);
            }
        }
    }

    private IEnumerator FadeCurtain(Image image, float targetAlpha, float duration)
    {
        if (duration > 0)
        {
            float startAlpha = image.color.a;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);

                image.color = new Color(image.color.r, image.color.g, image.color.b, newAlpha);
                yield return null;
            }
        }
        image.color = new Color(image.color.r, image.color.g, image.color.b, targetAlpha);
    }
}