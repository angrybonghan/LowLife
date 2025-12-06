using UnityEngine;

public class StageSelectPosition : MonoBehaviour
{
    [Header("스테이지")]
    public int stageNumber;
    public string stageSceneName;

    void Start()
    {
        var manager = StageSelectManager.instance;
        if (manager == null || stageNumber < 1 || manager.stage == null || stageNumber - 1 >= manager.stage.Length)
        {
            return;
        }

        int index = stageNumber - 1;
        var stageInfo = manager.stage[index];
        if (stageInfo == null)
        {
            stageInfo = new StageType();
            manager.stage[index] = stageInfo;
        }

        stageInfo.StageSceneName = stageSceneName;
        stageInfo.isUnlocked = true;
        stageInfo.stageSelectPosition = transform.position;
    }
}
