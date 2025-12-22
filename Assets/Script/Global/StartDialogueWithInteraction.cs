using System.Collections;
using UnityEngine;

public class StartDialogueWithInteraction : MonoBehaviour, I_Interactable, I_DialogueCallback
{
    [Header("대화")]
    public DialogueSO defaultDialogue; // 선행 퀘스트 미완료 시 보여줄 대화
    public DialogueSO questDialogue;   // 퀘스트 진행용 대화

    [Header("플레이어 이동/연출")]
    public Vector3 playerPos;
    public float duration = 0.2f;
    public bool facingRight = true;

    [Header("아이템 보상")]
    public string rewardItemID;
    public int rewardItemCount = 1;

    [Header("NPC 이미지 변경 (SpriteRenderer)")]
    public SpriteRenderer npcRenderer;
    public Sprite startDialogueSprite;
    public Sprite endDialogueSprite;

    [Header("대화후 다음씬으로 갈건가?")]
    public string nextSceneName;
    public bool NextScene = false;

    [Header("로딩씬 옵션")]
    public bool useLoadingScene = true;
    public string loadingSceneName = "StageLoading_1";
    public float waitTimeBeforeFade = 1f;
    public float fadeOutTime = 1.2f;

    [Header("스프라이트 플레이어 설정")]
    public bool stopSpritePlayerAtStart = false;

    public void InInteraction()
    {
        if (stopSpritePlayerAtStart)
        {
            var spritePlayer = GetComponent<SpritePlayer>();
            if (spritePlayer != null) spritePlayer.StopAnimation();
        }
        
        PlayerController.instance.AllStop();
        PlayerHandler.instance.PlayerGoto(playerPos, duration, facingRight);

        if (npcRenderer != null && startDialogueSprite != null)
            npcRenderer.sprite = startDialogueSprite;

        PlayerController.canControl = false;
    }

    public void OnDialogueEnd()
    {
        StartCoroutine(DialogueEndFunc());
    }

    IEnumerator DialogueEndFunc()
    {
        yield return new WaitForSeconds(0.2f);
        PlayerController.canControl = true;

        if (TryGetComponent<InteractionManager>(out InteractionManager interactionManager))
            interactionManager.ResetInteraction();

        // StageEnd처럼 로딩씬 -> 다음 씬 전환
        if (NextScene && !string.IsNullOrEmpty(nextSceneName))
        {
            ScreenTransition.ScreenTransitionGoto(
                nextSceneName,
                loadingSceneName,
                Color.black,
                waitTimeBeforeFade,
                fadeOutTime,
                useLoadingScene ? 2 : 0,
                0.5f,
                0
            );
        }
    }
}