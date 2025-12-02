using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    public float nomalTimeScale = 1;

    float timedSlowMotionValue = 1;
    float fadeTimeScaleValue = 1;

    Coroutine fadeTimeScaleCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        nomalTimeScale = Mathf.Max(nomalTimeScale, 0);
        Time.timeScale = nomalTimeScale;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            FadeTimeScale(1, 0);
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            FadeTimeScale(1, 0.2f);
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            FadeTimeScale(1, 1);
        }
    }

    private void LateUpdate()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsPaused)
            return;

        Time.timeScale = nomalTimeScale * timedSlowMotionValue * fadeTimeScaleValue;
    }

    public static void ResetTime()
    {
        Time.timeScale = instance.nomalTimeScale = 1;
    }

    /// <summary>
    /// 인수 : 목표 타임스케일
    /// </summary>
    public static void SetTimeScale(float value)
    {
        if (UIManager.Instance != null && UIManager.Instance.IsPaused)
            return;

        Time.timeScale = value * instance.nomalTimeScale;
    }

    /// <summary>
    /// 인수 : 동작 길이 - 목표 타임스케일
    /// </summary>
    public static void StartTimedSlowMotion(float duration, float multiplierValue)
    {
        if (UIManager.Instance != null && UIManager.Instance.IsPaused)
            return;

        instance.timedSlowMotionValue = multiplierValue * instance.nomalTimeScale;
        instance.StartCoroutine(instance.Co_StartTimedSlowMotion(duration));
    }

    IEnumerator Co_StartTimedSlowMotion(float duration)
    {
        float startTime = Time.realtimeSinceStartup;
        float targetTime = startTime + duration;
        yield return new WaitUntil(() => Time.realtimeSinceStartup >= targetTime);

        timedSlowMotionValue = nomalTimeScale;
    }

    /// <summary>
    /// 인수 : 기간(작동시간) - 목표 타임스케일
    /// </summary>
    public static void FadeTimeScale(float duration, float timeScale)
    {
        if (UIManager.Instance != null && UIManager.Instance.IsPaused)
            return;

        timeScale = Mathf.Max(timeScale, 0);
        if (instance.fadeTimeScaleCoroutine != null) instance.StopCoroutine(instance.fadeTimeScaleCoroutine);
        instance.fadeTimeScaleCoroutine = instance.StartCoroutine(instance.Co_FadeTimeScale(duration, timeScale));
    }

    IEnumerator Co_FadeTimeScale(float duration, float targetTimeScale)
    {
        if (duration > 0)
        {
            float startScale = Time.timeScale;
            float time = 0f;

            while (time < duration)
            {
                fadeTimeScaleValue = Mathf.Lerp(startScale, targetTimeScale, time / duration);
                time += Time.unscaledDeltaTime;
                yield return null;
            }
        }


        fadeTimeScaleValue = targetTimeScale;
    }
}
