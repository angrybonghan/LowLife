using System.Collections;
using UnityEngine;

public class TBP_AmagoTrigger : MonoBehaviour, I_TriggerBox
{
    [Header("¼Ò¸®")]
    public AudioClip spawnSound;
    public AudioClip bgmLoop;


    private void Start()
    {
        if (AmagoMovementManager.instance == null)
        {
            this.enabled = false;
            return;
        }
    }

    public void TriggerIn()
    {
        Trigger();
    }
    public void TriggerOut() { }

    void Trigger()
    {
        AmagoMovementManager.instance.StartSpawnAmago();
        StartCoroutine(SpawnSound());
    }

    IEnumerator SpawnSound()
    {
        SoundManager.instance.PlaySoundAtPosition(new Vector3(14, -32, Camera.main.transform.position.z), spawnSound);
        yield return new WaitForSeconds(0.5f);

        SoundManager.instance.PlayLoopBgm(bgmLoop, 0.7f);
    }
}
