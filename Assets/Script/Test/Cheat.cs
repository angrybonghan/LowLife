using System.Collections.Generic;
using UnityEngine;

public class Cheat : MonoBehaviour
{
    private string[] SceneList = {
    "GameStartLoding",
    "MainMenu",
    "RebelRoom_1",
    "RebelRoom_End",
    "Honeycomb_QueenRoom",
    "Honeycomb_Corridor",
    "Honeycomb_Fall",
    "DownTown_Bar_1",
    "Swomp_1",
    "Swomp_2",
    "Swomp_3_Cut",
    "Swomp_3_Boss",
    "Swamp_Boss_EndCut",
    "DownTown_Bar_2",
    "Train_Station",
    "Cave_1",
    "Cave_2",
    "Cave_3",
    "Cave_End",
    "Train_Inside",
    "Train_Boss",
    "Train_Boss_EndCut",
    "TheEnd",
    "TheEnd_2",
    };

    // 예외 처리 씬 리스트
    private List<string> disableScenes = new List<string>
    {
        "PlayerDeathLoading",
        "StageLoading_1",
    };

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            GotoNextScene();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            TimeManager.SetTimeScale(1f);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            TimeManager.SetTimeScale(1.5f);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            TimeManager.SetTimeScale(2f);
        }

    }

    void GotoNextScene()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (disableScenes.Contains(currentSceneName)) return;

        for (int i = 0; i < SceneList.Length; i++)
        {
            if (SceneList[i] == currentSceneName)
            {
                int nextIndex = (i + 1) % SceneList.Length;
                string nextSceneName = SceneList[nextIndex];
                ScreenTransition.ScreenTransitionGoto(nextSceneName, "none", Color.black, 0f, 0.4f, 0f, 0.4f, 0f);
                break;
            }
        }
    }
        
}
