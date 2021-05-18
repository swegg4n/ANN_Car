using UnityEngine;

public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance = null;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }


    [SerializeField] private bool debugRays = true;
    [SerializeField] private bool debugDistances = false;
    [SerializeField] private bool debugFitness = true;

    public bool DebugRays { get { return debugRays; } }
    public bool DebugDistances { get { return debugDistances; } }
    public bool DebugFitness { get { return debugFitness; } }
}
