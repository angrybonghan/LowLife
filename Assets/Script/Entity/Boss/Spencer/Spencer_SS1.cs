using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Spencer_SS1 : MonoBehaviour, I_Attackable
{
    [Header("재장전 시간")]
    public float reloadTime;

    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (SpencerManager.Instance.halfHP)
        {
            reloadTime = 0.1f;
        }
    }

    public void StartReload()
    {
        StartCoroutine(WaitForReloadTime());
    }

    IEnumerator WaitForReloadTime()
    {
        yield return new WaitForSeconds(reloadTime);
        anim.SetTrigger("end");
    }

    public void EndReload()
    {
        SpencerManager.Instance.canUseSklill = true;
    }

    public bool CanAttack(Transform attacker)
    {
        return true;
    }

    public void OnAttack(Transform attacker)
    {
        SpencerManager.Instance.TakeDamage();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.instance.ImmediateDeath();
        }
    }
}
