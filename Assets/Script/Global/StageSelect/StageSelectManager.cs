using System.Collections;
using UnityEngine;

public class StageType
{
    public string StageSceneName;
    public bool isUnlocked;
    public Vector2 stageSelectPosition;
}

public class StageSelectManager : MonoBehaviour
{
    public static StageSelectManager instance;
    [HideInInspector] public StageType[] stage = new StageType[8];

    [Header("이동 시간")]
    public float moveDuration = 0.2f;

    int stageNumber = 1;

    bool isMoving = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            stageNumber--;
            if (!MoveStage(stageNumber))
            {
                stageNumber++;
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            stageNumber++;

            if (!MoveStage(stageNumber))
            {
                stageNumber--;
            }
        }
    }

    bool MoveStage(int stageNumber)
    {
        if (isMoving || stage == null || stageNumber < 1 || stageNumber > stage.Length)
        {
            return false;
        }

        var stageInfo = stage[stageNumber - 1];
        if (stageInfo == null || !stageInfo.isUnlocked)
        {
            return false;
        }

        Vector2 targetPos = stageInfo.stageSelectPosition;
        isMoving = true;
        StartCoroutine(MoveCoroutine(targetPos, moveDuration));
        this.stageNumber = stageNumber;

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
}
