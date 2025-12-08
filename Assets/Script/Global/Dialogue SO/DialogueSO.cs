using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [Header("말풍선 위치")]
    public Vector3 bubblePosition;  // 말풍선의 월드 좌표
    // bubblePositionOffset 이 원점 (0,0,0) 이라면 말풍선 위치를 움직이지 않음
    public float bubbleBodyOffset;  // 말풍선 몸통의 좌-우 오프셋
    public bool isTailDown;   // 말풍선 꼬리표가 위쪽에 있을지에 대한 여부

    [Header("텍스트")]
    [TextArea(1, 10)]   // 인스펙터 창에서 표시되는 입력란 열 최소, 최대 크기
    public string[] sentence;    // 대사 내용 배열 (입력되지 않으면 문자열을 넘기거나, 다음 세션으로 넘어감)

    [Header("설정")]
    public float intervalTime; // 글자 하나의 출력 간격 시간
    public bool canSkip; // 플레이어가 이 대사를 임의의 지정된 키로 넘길 수 있는지에 대한 여부. true면 넘길 수 있음.
    public bool autoSkip; // 자동 스킵의 여부. true면 문자열 하나가 전부 재생되었을 때, autoSkipIntervalTime 초 대기 후 자동으로 다음 대사로 넘어감
    public float autoSkipIntervalTime; // 자동 스킵에서 대기할 시간
}

[CreateAssetMenu(fileName = "Totally Awesome Dialogue", menuName = "Dialogue/New Dialogue")]
public class DialogueSO : ScriptableObject
{
    public DialogueLine[] section;
}