using System.Collections;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    public static PlayerHandler instance { get; private set; }
    [HideInInspector] public bool isPlayerBeingManipulated;

    private Coroutine PlayerGotoCoroutine;
    private Coroutine PlayerMoveForwardCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    PlayerMoveForwardTo(60);
        //}
    }

    public void PlayerGoto(Vector3 targetPos, float duration, bool facingRight)
    {
        if (PlayerGotoCoroutine != null)
        {
            StopCoroutine(PlayerGotoCoroutine);
        }

        isPlayerBeingManipulated = true;

        PlayerGotoCoroutine = StartCoroutine(Co_PlayerGoto(targetPos, duration));
        PlayerController.instance.LookRight(facingRight);
    }

    IEnumerator Co_PlayerGoto(Vector3 targetPos, float duration)
    {
        if (duration > 0)
        {
            float elapsedTime = 0;
            Vector3 originPos = PlayerController.instance.transform.position;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;

                float t = elapsedTime / duration;
                Vector3 targetpos = Vector3.Lerp(originPos, targetPos, t);

                PlayerController.instance.Goto(targetpos);
                yield return null;
            }
        }
        PlayerController.instance.Goto(targetPos);
        isPlayerBeingManipulated = false;
        PlayerGotoCoroutine = null;
    }

    public void PlayerMoveForwardTo(float targetX)
    {
        if (PlayerMoveForwardCoroutine != null)
        {
            StopCoroutine(PlayerMoveForwardCoroutine);
        }

        isPlayerBeingManipulated = true;

        Vector2 lookPos = new Vector2(targetX, transform.position.y);
        PlayerController.instance.AllStop();
        PlayerController.instance.LookPos(lookPos);
        PlayerController.instance.SetSpeed(true);
        PlayerController.canControl = false;

        PlayerMoveForwardCoroutine = StartCoroutine(Co_PlayerMoveForwardTo(targetX));
    }

    IEnumerator Co_PlayerMoveForwardTo(float targetX)
    {
        Vector3 targetPos = new Vector3(targetX, transform.position.y, transform.position.z);

        while (!HasArrived(targetPos))
        {
            targetPos = new Vector3(targetX, transform.position.y, transform.position.z);
            yield return null;
        }

        PlayerController.canControl = true;
        isPlayerBeingManipulated = false;
        PlayerMoveForwardCoroutine = null;
    }

    bool HasArrived(Vector3 pos)
    {
        float distance = Vector3.Distance(transform.position, pos);
        return distance <= 0.1f;
    }
}
