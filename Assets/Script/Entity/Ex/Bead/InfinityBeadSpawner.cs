using System.Collections;
using UnityEngine;

public class InfinityBeadSpawner : MonoBehaviour
{
    [Header("积己 矫埃")]
    public float generationInterval = 1.0f;
    public float generationTime = 0.5f;

    [Header("积己 厚靛")]
    public GameObject bead;

    GameObject currentBead;
    Coroutine beadExtensionCoroutine;

    void Start()
    {
        if (bead == null)
        {
            Destroy(gameObject);
            return;
        }

        currentBead = Instantiate(bead, transform.position, Quaternion.identity);

        StartCoroutine(CreateBeadSequence());
    }

    IEnumerator CreateBeadSequence()
    {
        while (true)
        {
            yield return new WaitUntil(() => ShouldCreateBead());
            yield return new WaitForSeconds(generationInterval);

            currentBead = Instantiate(bead, transform.position, Quaternion.identity);
            currentBead.transform.localScale = Vector3.zero;
            if (beadExtensionCoroutine != null) StopCoroutine(beadExtensionCoroutine);
            beadExtensionCoroutine = StartCoroutine(BeadExtension());
        }
    }

    IEnumerator BeadExtension()
    {
        if (generationTime > 0)
        {
            float time = 0;

            while (time < generationTime)
            {
                if (currentBead == null)
                {
                    beadExtensionCoroutine = null;
                    yield break;
                }

                currentBead.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, time / generationTime);

                yield return null;
                time += Time.deltaTime;
            }
        }

        currentBead.transform.localScale = Vector3.one;
        beadExtensionCoroutine = null;
    }

    bool ShouldCreateBead()
    {
        return currentBead == null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.325f);
    }
}
