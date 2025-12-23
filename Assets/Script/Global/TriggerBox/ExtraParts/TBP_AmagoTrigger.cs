using System.Collections;
using UnityEngine;

public class TBP_AmagoTrigger : MonoBehaviour, I_TriggerBox
{
    [Header("BGM")]
    public AudioClip amagoBgm;

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
        AmagoMovementManager.instance.StartMoveAmago();
        StartCoroutine(SpawnSound());
    }

    IEnumerator SpawnSound()
    {
        AudioManager.Instance.Play3DSound(spawnSound, new Vector3(14, -32, Camera.main.transform.position.z));
        yield return new WaitForSeconds(0.5f);

        AudioManager.Instance.Play2DSound(bgmLoop, "AmagoSoundLoop", 1f, 0.7f, true);
        AudioManager.Instance.Play2DSound(amagoBgm, "vsAmagoBGM", 1f, 0.5f, true);

        while (true)
        {
            CameraMovement.PositionShaking(0.2f, 0.05f, Mathf.Infinity);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
