using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmagoMovementManager : MonoBehaviour
{
    public static AmagoMovementManager instance {  get; private set; }

    [Header("첫 번째 길")]
    public AmagoRoadSelector firstRoad;

    [Header("이동 설정")]
    public float minMoveSpeed = 5f;
    public float maxMoveSpeed = 35f;
    public float speedIncreaseSensitivity = 7f;
    public float rotationDuration = 0.2f;

    [Header("몸")]
    public AmagoBodyMovement bodyPrefab;
    public float bodySpawnDistanceInterval = 0.4f;
    public float bodyAnimationInterval = 0.2f;
    public int maxBodyCount = 15;

    [Header("머리")]
    public AmagoHeadMovement headPrefab;


    Vector3 lastSpawnPartPos;

    int layerNumber = -10;
    bool canMoveAmago = false;
    List<AmagoBodyMovement> allBody = new List<AmagoBodyMovement>();
    AmagoHeadMovement head;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            this.enabled = false;
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        SummonFullAmago();
    }

    void Update()
    {
        if (!canMoveAmago) return;
        UpdateSpeed();
    }

    public void StartMoveAmago()
    {
        canMoveAmago = true;
    }

    void SummonFullAmago()
    {
        head = Instantiate(headPrefab, transform.position, Quaternion.identity);

        head.transform.position = transform.position;
        head.currentRoadTarget = firstRoad;
        head.rotationDuration = rotationDuration;
        head.transform.SetParent(transform);
        lastSpawnPartPos = transform.position;

        for (int i = 0; i < maxBodyCount; i++)
        {
            lastSpawnPartPos.x -= bodySpawnDistanceInterval;

            if (maxBodyCount - 1 == i) SummonBody(lastSpawnPartPos, i).lastBody = true;
            else SummonBody(lastSpawnPartPos, i);

            layerNumber--;
        }

        SetAmagoSpeed(0);
        StartCoroutine(ReloadBdyAnimation());
    }

    AmagoBodyMovement SummonBody(Vector3 spawnPos, int bodyNumber)
    {
        AmagoBodyMovement newBody = Instantiate(bodyPrefab, spawnPos, Quaternion.identity);
        newBody.currentRoadTarget = firstRoad;
        newBody.SetOrderInLayer(layerNumber);
        newBody.rotationDuration = rotationDuration;
        newBody.transform.SetParent(transform);
        newBody.name = $"AmagoBody_{bodyNumber}";
        allBody.Add(newBody);
        lastSpawnPartPos = newBody.transform.position;

        return newBody;
    }

    void UpdateSpeed()
    {
        if (head==null)
        {
            SetAmagoSpeed(maxMoveSpeed);
            return;
        }

        float distanceToPlayer = PlayerController.instance == null ?
            0f :
            Vector2.Distance(head.transform.position, PlayerController.instance.transform.position);
        // 삼항연산을 은밀하게 사용하기 튜토리얼!
        // 1. 핑크색 PS5 컨트롤러를 준비ㅎ

        float t = Mathf.Clamp01(distanceToPlayer / speedIncreaseSensitivity);
        float speed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, t);

        SetAmagoSpeed(speed);
    }

    void SetAmagoSpeed(float value)
    {
        if (allBody.Count != 0)
        {
            foreach (AmagoBodyMovement body in allBody)
            {
                body.speed = value;
            }
        }

        head.speed = value;
    }

    IEnumerator ReloadBdyAnimation()
    {
        float lastDistance = 0f;
        foreach (AmagoBodyMovement body in allBody)
        {
            body.ReloadBodyAnimtion();
            lastDistance = Vector2.Distance(head.transform.position, transform.position);
            yield return new WaitForSeconds(bodyAnimationInterval);
        }
    }
}
