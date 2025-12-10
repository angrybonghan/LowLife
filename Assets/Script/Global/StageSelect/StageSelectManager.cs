using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StageType
{
    public string StageSceneName;
    public bool isUnlocked;
    public Vector2 stageSelectPosition;
    public StageSelectPosition positionRef;
}

public class StageSelectManager : MonoBehaviour
{
    public static StageSelectManager instance;
    public StageType[] stage = new StageType[8];

    [Header("이동 시간")]
    public float moveDuration = 0.2f;

    [Header("로딩씬 옵션")]
    public bool useLoadingScene = true;
    public string loadingSceneName = "StageLoading_1";
    public float waitTimeBeforeFade = 1f;
    public float fadeOutTime = 1.2f;

    [Header("잠금 아이콘 UI")]
    public Image lockIcon;

    int stageNumber = 1;
    bool isMoving = false;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            stageNumber--;
            if (!MoveStage(stageNumber)) stageNumber++;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            stageNumber++;
            if (!MoveStage(stageNumber)) stageNumber--;
        }

        // Enter 키로 현재 선택된 스테이지 시작
        if (Input.GetKeyDown(KeyCode.Return))
        {
            TryStartStage();
        }
    }

    bool MoveStage(int stageNumber)
    {
        if (isMoving || stage == null || stageNumber < 1 || stageNumber > stage.Length) return false;

        var stageInfo = stage[stageNumber - 1];
        if (stageInfo == null) return false;

        Vector2 targetPos = stageInfo.stageSelectPosition;
        isMoving = true;
        StartCoroutine(MoveCoroutine(targetPos, moveDuration));
        this.stageNumber = stageNumber;

        // 잠금 아이콘 표시/숨김 처리
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(!stageInfo.isUnlocked);
        }

        return true;
    }

    IEnumerator MoveCoroutine(Vector2 targetPos, float duration)
    {
        if (duration <= 0f)
        {
            transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
            isMoving = false;
            yield break;
        }

        Vector2 startPos = new Vector2(transform.position.x, transform.position.y);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector2 newPos = Vector2.Lerp(startPos, targetPos, t);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
            yield return null;
        }

        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
        isMoving = false;
    }

    // 현재 선택된 스테이지가 해제되어 있으면 시작
    void TryStartStage()
    {
        var stageInfo = stage[stageNumber - 1];
        if (stageInfo != null && stageInfo.isUnlocked)
        {
            ScreenTransition.ScreenTransitionGoto(
                stageInfo.StageSceneName,
                loadingSceneName,
                Color.black,
                waitTimeBeforeFade,
                fadeOutTime,
                useLoadingScene ? 2 : 0,
                0.5f,
                0
            );
        }
        else
        {
            Debug.Log($"[잠김] {stageInfo?.StageSceneName} 씬은 아직 클리어하지 못했습니다.");
        }
    }
}