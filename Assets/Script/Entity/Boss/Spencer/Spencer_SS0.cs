using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Animator),typeof(Collider2D))]
public class Spencer_SS0 : MonoBehaviour, I_Attackable
{
    [Header("시간")]
    public float teleportTime = 1.0f;

    [Header("위치")]
    public float centerX;
    public float positionSpacing;
    

    Animator anim;
    Collider2D col;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    public void StartTeleport()
    {
        transform.position = GetTeleportPos();
        col.enabled = false;
        StartCoroutine(WaitTeleportDutation());
    }

    IEnumerator WaitTeleportDutation()
    {
        yield return new WaitForSeconds(teleportTime);
        anim.SetTrigger("tp");
    }

    public void EndTeleport()
    {
        col.enabled = true;
        SpencerManager.Instance.canUseSklill = true;
    }

    Vector2 GetTeleportPos()
    {
        Vector2 pos = transform.position;
        pos.x = centerX;

        if (SpencerManager.Instance.positionNumber == 2)
        {
            if (Random.value > 0.5f)
            {
                pos.x += positionSpacing;
                SpencerManager.Instance.positionNumber = 3;
            }
            else
            {
                pos.x -= positionSpacing;
                SpencerManager.Instance.positionNumber = 1;
            }
        }
        else
        {
            if (Random.value > 0.7f)
            {
                if (SpencerManager.Instance.positionNumber == 1)
                {
                    pos.x += positionSpacing;
                    SpencerManager.Instance.positionNumber = 3;
                }
                else
                {
                    pos.x -= positionSpacing;
                    SpencerManager.Instance.positionNumber = 1;
                }
            }
            else
            {
                SpencerManager.Instance.positionNumber = 2;
            }
        }

        return pos;
    }

    public bool CanAttack(Transform attacker)
    {
        return true;
    }

    public void OnAttack(Transform attacker)
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }
}
