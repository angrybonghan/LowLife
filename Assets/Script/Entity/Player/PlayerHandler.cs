using System.Collections;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
    public static PlayerHandler instance { get; private set; }

    private Coroutine PlayerGotoCoroutine;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public static void PlayerGoto(Vector3 targetPos, float duration, bool facingRight)
    {
        if (instance.PlayerGotoCoroutine != null)
        {
            instance.StopCoroutine(instance.PlayerGotoCoroutine);
        }

        instance.PlayerGotoCoroutine = instance.StartCoroutine(instance.playerGoto(targetPos, duration));
        PlayerController.instance.LookRight(facingRight);
    }

    IEnumerator playerGoto(Vector3 targetPos, float duration)
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

        PlayerGotoCoroutine = null;
    }
}
