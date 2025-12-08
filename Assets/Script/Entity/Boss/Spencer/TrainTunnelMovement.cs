using System.Collections;
using UnityEngine;

public class TrainTunnelMovement : MonoBehaviour
{
    public static TrainTunnelMovement instance;

    [Header("이동")]
    public float moveSpeed = 5f;

    [Header("위치")]
    public float endXPosition = -20f;

    [Header("스프라이트 배경")]
    public float spriteInterval = 10f;

    [Header("스프라이트")]
    public float spriteScale = 3f;
    public Sprite tunnelStart;
    public Sprite tunnelLoop;
    public Sprite tunnelEnd;

    [Header("터널 시작")]
    public int startSpriteCount = 5;
    public Vector2 startPosition;
    Vector2 spriteLocalposition;

    [Header("가림막")]
    public Sprite_Animator backgroundCurtain;
    public Sprite_Animator frontCurtain;

    [Header("스피드라인")]
    public Vector2 speedLineSpawnPosition;
    public TrainTunnelSpeedLine speedLinePrefab;
    public float minSpeedLineInterval = 0.05f;
    public float maxSpeedLineInterval = 0.3f;

    [Header("소리")]
    public AudioClip tunnelEnterSound;
    public AudioClip tunnelLoopSound;
    public AudioClip tunnelExitSound;


    bool isActive = false;
    bool hasEndPoint = false;
    bool needEndPoint = false;
    const float resetDistance = 3000f;
    Coroutine speedLineRoutine = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ResetTunnel();
    }

    void Update()
    {
        if (isActive)
        {
            Vector2 move = new Vector2(-moveSpeed * Time.deltaTime, 0f);
            transform.position += (Vector3)move;
        }

        if (hasEndPoint)
        {
            if (!hasChildren())
            {
                ResetTunnel();
            }
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            StartTunnel();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            EndTunnel();
        }

        if (transform.position.x <= -resetDistance)
        {
            float overshoot = transform.position.x;
            transform.position = new Vector3(0, transform.position.y, transform.position.z);

            foreach (Transform child in transform)
            {
                child.localPosition += new Vector3(overshoot, 0, 0);
            }

            spriteLocalposition += new Vector2(overshoot, 0f);
        }
    }

    void ResetTunnel()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            Destroy(child);
        }

        transform.position = startPosition;
        spriteLocalposition = Vector2.zero;
        isActive = false;
        hasEndPoint = false;
        needEndPoint = false;

        if (speedLineRoutine != null)
        {
            StopCoroutine(speedLineRoutine);
            speedLineRoutine = null;
        }
    }

    public void StartTunnel()
    {
        ResetTunnel();

        SpawnNewSprite(tunnelStart);

        for (int i = 0; i < startSpriteCount - 1; i++)
        {
            SpawnNewSprite(tunnelLoop);
        }

        frontCurtain.SetAlpha(0.7f, 0.37887f);

        isActive = true;

        if (speedLinePrefab != null && speedLineRoutine == null)
        {
            speedLineRoutine = StartCoroutine(SpawnSpeedLinesLoop());
        }

        RandomTreeSpawner.instance.canSpawn = false;

        StartCoroutine(PlayTunnelEnterSound());
    }

    IEnumerator PlayTunnelEnterSound()
    {
        SoundManager.instance.PlaySoundAtPosition(Vector3.zero, tunnelEnterSound);
        yield return new WaitForSeconds(0.35f);
        SoundManager.instance.PlayLoopBgm(tunnelLoopSound, "TrainTunnelLoop", 1.2f, 0.35f);
    }

    public void EndTunnel()
    {
        if (!isActive) return;
        backgroundCurtain.SetAlpha(1, 0);
        needEndPoint = true;

        if (speedLineRoutine != null)
        {
            StopCoroutine(speedLineRoutine);
            speedLineRoutine = null;
        }

        SoundManager.instance.StopSound("TrainTunnelLoop");
        SoundManager.instance.PlaySoundAtPosition(Vector3.zero, tunnelExitSound);
        RandomTreeSpawner.instance.canSpawn = true;
    }

    public void SpriteDeleted()
    {
        if (hasEndPoint) return;

        if (needEndPoint)
        {
            SpawnNewSprite(tunnelEnd);
            backgroundCurtain.SetAlpha(0, 1);
            frontCurtain.SetAlpha(0, 0.1f);
            hasEndPoint = true;
        }
        else
        {
            SpawnNewSprite(tunnelLoop);
        }
    }

    void SpawnNewSprite(Sprite sprite)
    {
        GameObject go = new GameObject("TrainTunnel");
        TrainBackgroundSprite bg = go.AddComponent<TrainBackgroundSprite>();
        go.transform.SetParent(transform);
        go.transform.localPosition = spriteLocalposition;

        bg.SetSprite(sprite, -50);
        bg.SetSize(spriteScale);
        bg.endXPosition = endXPosition;

        bg.tunnelManager = this;
        bg.isTunnel = true;

        SetNextSpritePosition();
    }

    IEnumerator SpawnSpeedLinesLoop()
    {
        while (isActive && !needEndPoint)
        {
            SpawnSpeedLine();
            float wait = Random.Range(minSpeedLineInterval, maxSpeedLineInterval);
            yield return new WaitForSeconds(wait);
        }
        speedLineRoutine = null;
    }

    void SpawnSpeedLine()
    {
        if (speedLinePrefab == null) return;
        Instantiate(speedLinePrefab, speedLineSpawnPosition, Quaternion.identity);
    }

    void SetNextSpritePosition()
    {
        spriteLocalposition += new Vector2(spriteInterval, 0f);
    }

    bool hasChildren()
    {
        return transform.childCount > 0;
    }
}
