using UnityEngine;
using UnityEngine.UI;

public class StageSelectUIController : MonoBehaviour
{
    public Button[] stageButtons;
    public string[] stageNames;
    public Image[] lockIcons;

    void Start()
    {
        var clearedStages = SaveSystemJSON.DataLoadClearedStages();

        for (int i = 0; i < stageButtons.Length; i++)
        {
            string stageName = stageNames[i];
            bool isCleared = clearedStages.Contains(stageName);

            if (isCleared)
            {
                lockIcons[i].gameObject.SetActive(false);
                stageButtons[i].onClick.AddListener(() =>
                {
                    ScreenTransition.ScreenTransitionGoto(
                        stageName, "LoadingScene", Color.black,
                        0f, 1f, 1f, 1f, 0f, true
                    );
                });
            }
            else
            {
                lockIcons[i].gameObject.SetActive(true);
                stageButtons[i].onClick.AddListener(() =>
                {
                    Debug.Log($"[잠김] {stageName} 씬은 아직 클리어하지 못했습니다.");
                });
            }
        }
    }
}