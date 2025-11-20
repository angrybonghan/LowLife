using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public static void SwitchLayerTo(string layerName, GameObject target)
    {
        int layerIndex = LayerMask.NameToLayer("Particle");

        if (layerIndex != -1) target.layer = layerIndex;
    }
}
