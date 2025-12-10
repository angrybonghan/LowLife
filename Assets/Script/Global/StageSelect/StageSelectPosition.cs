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

        // SaveSystemJSON에서 클리어된 스테이지 목록 불러오기
        var clearedStages = SaveSystemJSON.DataLoadClearedStages();

        // 저장된 목록에 stageSceneName이 있으면 잠금 해제
        stageInfo.isUnlocked = clearedStages.Contains(stageSceneName);

        stageInfo.stageSelectPosition = transform.position;
        stageInfo.positionRef = this; // 참조 저장
    }
}