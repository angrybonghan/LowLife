using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataResetByKey : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("[키 입력 감지: Ctrl+Shift+R] 퀘스트 데이터 초기화 실행");

            // 저장 데이터 초기화
            QuestSaveSystemJSON.ClearQuests();
        }
    }
}
