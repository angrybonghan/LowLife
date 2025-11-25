using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public static void SwitchLayerTo(string layerName, GameObject target)
    {
        int layerIndex = LayerMask.NameToLayer("Particle");

        if (layerIndex != -1) target.layer = layerIndex;
    }

    public void PlayerDeath()
    {
        Color fadeColor = new Color32(30, 30, 30, 255);
        string currentSceneName = SceneManager.GetActiveScene().name;
        ScreenTransition.ScreenTransitionGoto(currentSceneName, "PlayerDeathLoading", fadeColor, 2, 0.5f, 2, 0.5f, 0);
    }

}
