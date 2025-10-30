using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public static void ResetTime()
    {
        Time.timeScale = 1;
    }

    public static void SetTimeSpeed(float value)
    {
        Time.timeScale = value;
    }

    /// <summary>
    /// 인수 : 길이 - 타임스케일
    /// </summary>
    public static void StartTimedSlowMotion(float duration, float timeScale)
    {
        Time.timeScale = timeScale;
        instance.StartCoroutine(instance.RestoreTimeScaleRoutine(duration));
    }

    IEnumerator RestoreTimeScaleRoutine(float duration)
    {
        float startTime = Time.realtimeSinceStartup;
        float targetTime = startTime + duration;
        yield return new WaitUntil(() => Time.realtimeSinceStartup >= targetTime);

        Time.timeScale = 1f;
    }
}
