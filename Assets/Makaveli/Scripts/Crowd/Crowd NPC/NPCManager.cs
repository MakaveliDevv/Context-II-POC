using UnityEngine;
public class NPCManager : MonoBehaviour
{   
    // private NPCPatrol nPCPatrol;
    // [SerializeField] private LayerMask layerMask;
    // [SerializeField] private float patrolSpeed = 2f;
    // [SerializeField] private float obstacleAvoidanceDistance = 1.5f;
    // [SerializeField] private float raycastLength = 2f;
    // [SerializeField] private float spacing = 1f;
    
    // Visual debugging
    // [SerializeField] private bool showDebugVisuals = true;

    // NPC Follow
    private NPCFollower nPCFollower;

    [Header("Movement Parameters")]
    public float smoothSpeed = 5f;
    public float stoppingThreshold = 0.1f;
    
    [Header("Positioning Parameters")]
    public float minDistanceBehindTarget = 2f;
    public float maxDistanceBehindTarget = 6f;
    public float minNPCDistance = 1.2f;
    public float maxNPCDistance = 3f;
    public float spreadFactor = 1.5f;
    public float fixedYPosition = 1f;


    void Awake()
    {
        MGameManager.instance.allNPCs.Add(this);
       
        // nPCPatrol = new NPCPatrol
        // (
        //     transform,
        //     layerMask,
        //     transform.position, // Start with current position as target
        //     patrolSpeed,
        //     obstacleAvoidanceDistance,
        //     raycastLength,
        //     spacing
        // );

        nPCFollower = new
        (
            transform,
            smoothSpeed,
            stoppingThreshold,
            minDistanceBehindTarget,
            maxDistanceBehindTarget,
            minNPCDistance,
            maxNPCDistance,
            spreadFactor,
            fixedYPosition
        );


    }

    void Start()
    {
        nPCFollower.CustomStart(this);
    }

    void Update()
    {
        // nPCPatrol.MoveNPC();

        nPCFollower.CustomUpdate(this);
        
    }
    
    private void OnDestroy()
    {
        if (MGameManager.instance != null && MGameManager.instance.allNPCs.Contains(this))
        {
            MGameManager.instance.allNPCs.Remove(this);
        }
        
        // NPCsManagement.UnregisterTriggerMovement(nPCPatrol);
    }

    // void OnDrawGizmos()
    // {
    //     if (!Application.isPlaying) return;

    //     // Draw center of mass
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawWireSphere(centerOfMass, 0.5f);

    //     // Draw NPC position
    //     Gizmos.color = isSpreading ? Color.yellow : Color.green;
    //     Gizmos.DrawWireSphere(transform.position, 0.5f);

    //     // Draw target position
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawSphere(targetPosition, 0.2f);
    // }
}