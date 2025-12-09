using System.Collections;
using UnityEngine;

public class SmallSwamp : MonoBehaviour
{
    [Header("빠져나오기")]
    public int spaceCount = 5;
    public float jumpOutPower = 16f;

    [Header("아이콘")]
    public SpriteRenderer spaceIcon;

    bool canCatchPlayer = true;
    bool isPlayerCatched = false;
    int currentSpaceCount = 0;

    PlayerController pc;
    Vector3 playerOffset;

    Coroutine jumpIconBlinkCoroutine = null;

    private void Start()
    {
        pc = PlayerController.instance;
        playerOffset = new Vector3(0, -0.1f, 0);
    }

    private void Update()
    {
        if (pc == null) return;

        if (isPlayerCatched)
        {
            pc.transform.position = transform.position + playerOffset;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentSpaceCount ++;
                if (currentSpaceCount >= spaceCount)
                {
                    ReleasePlayer();
                }
            }
        }
    }

    void ReleasePlayer()
    {
        isPlayerCatched = false;
        if (jumpIconBlinkCoroutine != null) StopCoroutine(jumpIconBlinkCoroutine);
        spaceIcon.color = Color.clear;
        pc.transform.position = transform.position + Vector3.up;
        PlayerController.canControl = true;
        pc.ExternalJump(jumpOutPower);

        StartCoroutine(Co_ReleasePlayer());
    }

    IEnumerator Co_ReleasePlayer()
    {
        yield return new WaitForSeconds(0.5f);

        canCatchPlayer = true;
    }

    void CatchPlayer()
    {
        isPlayerCatched = true;
        currentSpaceCount = 0;

        pc.AllStop();
        PlayerController.canControl = false;

        jumpIconBlinkCoroutine = StartCoroutine(JumpIconBlink());
    }

    IEnumerator JumpIconBlink()
    {
        while (true)
        {
            spaceIcon.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spaceIcon.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPlayerCatched || !canCatchPlayer) return;
        
        if (collision.CompareTag("Player"))
        {
            CatchPlayer();
        }
    }
}
